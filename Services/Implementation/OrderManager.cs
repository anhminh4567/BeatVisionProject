﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Identity.Client;
using Net.payOS.Types;
using Repository.Interface;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public partial class OrderManager
	{
		//Flow
		//PlaceOrder --> OnPlaceOrder_Createlink --> return Ok(link);
		//	OnSuccess --> OnReturnUrl --> CheckPaymentSuccess(long orderCode) --> [backgroundService]
		//	OnCancel  --> OnCancelUrl
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly AppUserManager _userManager;
		private readonly PayosService _payosService;
		private readonly TrackManager _trackManager;
		private readonly UserIdentityServices _userIdentityServices;
		private readonly NotificationManager _notificationManager;
		private readonly UserIdentityServices _userIdentityService;
		private readonly AppsettingBinding _appsettingBinding;

		public OrderManager(IUnitOfWork unitOfWork, IMapper mappter, AppUserManager userManager, PayosService payosService, TrackManager trackManager, UserIdentityServices userIdentityServices, NotificationManager notificationManager, UserIdentityServices userIdentityService, AppsettingBinding appsettingBinding)
		{
			_unitOfWork = unitOfWork;
			_mapper = mappter;
			_userManager = userManager;
			_payosService = payosService;
			_trackManager = trackManager;
			_userIdentityServices = userIdentityServices;
			_notificationManager = notificationManager;
			_userIdentityService = userIdentityService;
			_appsettingBinding = appsettingBinding;
		}
		public async Task<Result<CreatePaymentResultDto>> Checkout(int userProfileId) 
		{
			var error = new Error();
			var getuserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if(getuserProfile == null)
			{
				return Result<CreatePaymentResultDto>.Fail();
			}
			var placeOrderResult = await PlaceOrder(getuserProfile);
			if(placeOrderResult.isSuccess is false)
			{
				return Result<CreatePaymentResultDto>.Fail(placeOrderResult.Error);
			}
			var newOrder = placeOrderResult.Value;
			if (newOrder.Price == 0) //FREE ORDER
			{
				var result = await OnFreeOrder(newOrder);
				return Result<CreatePaymentResultDto>.Success(null);
			}
			else
			{
				var createPaymentResult = await OnPlaceOrder_CreateLink(newOrder);
				if(createPaymentResult.isSuccess is false)
				{
					return Result<CreatePaymentResultDto>.Fail(createPaymentResult.Error);
				}
				return Result<CreatePaymentResultDto>.Success(createPaymentResult.Value);
			}

		}
		public async Task<Result<Order>> PlaceOrder(UserProfile userProfile)
		{
			var error = new Error();
			var getUserCartItem = (await _unitOfWork.Repositories.cartItemRepository.GetByCondition(c => c.UserId == userProfile.Id)).ToList(); //_userManager.GetAllUserCartItems(userProfile);
			if (getUserCartItem is null || getUserCartItem.Count == 0)
			{
				error.ErrorMessage = "nothing to buy in cart";
				return Result<Order>.Fail(error);
			}
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				var getTracksFromItems = await GetTrackItems(getUserCartItem.ToList());
				IList<OrderItem> orderItems = new List<OrderItem>();
				int totalCartItemPrice = CountTotalPriceFromTrackItem(getTracksFromItems);
				Order order = new Order()
				{
					CreateDate = DateTime.Now,
					Price = totalCartItemPrice,
					OriginalPrice = totalCartItemPrice,
					PriceRemain = totalCartItemPrice,
					PricePaid = 0,
					Status = Shared.Enums.OrderStatus.PENDING,
					UserId = userProfile.Id,
					Description = ""
				};
				var createResult = await _unitOfWork.Repositories.orderRepository.Create(order);
				await _unitOfWork.SaveChangesAsync();
				foreach (var cartItem in getUserCartItem)
				{
					var track = getTracksFromItems.First(t => t.Id == cartItem.ItemId);
					var orderItem = new OrderItem
					{
						IsSale = false,
						OriginalPrice = track.Price,
						TrackName = track.TrackName,
						TrackId = track.Id,
						CurrentPrice = track.Price,
						OrderId = order.Id,
					};
					orderItems.Add(orderItem);
				};
				var createItemsResult = await _unitOfWork.Repositories.orderItemRepository.CreateRange(orderItems);
				await _unitOfWork.SaveChangesAsync();
				if (createItemsResult is false)
				{
					throw new Exception("Error in creating order item");
				}
				order.OrderItems = orderItems;
				await _unitOfWork.Repositories.orderRepository.Update(order);
				await _unitOfWork.SaveChangesAsync();

				await _unitOfWork.CommitAsync();
				
				return Result<Order>.Success(order);
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollBackAsync();
				error.isException = true;
				error.ErrorMessage = ex.Message;
				error.StatusCode = StatusCodes.Status500InternalServerError;
				return Result<Order>.Fail(error);
			}
		}
		public async Task<Result<CreatePaymentResultDto>> OnPlaceOrder_CreateLink(Order order)
		{
			// neu tao link ko thanh cong, coi nhu huy don, yeb, cho chac cu
			var error = new Error();
			var createPaymentLinkResult = await _payosService.CreateOrderPaymentLink(order);
			if(createPaymentLinkResult.isSuccess is false)
			{
				error.ErrorMessage = "failt to create paymentlink, order is cancel now, please place a new order";
				order.Status = OrderStatus.CANCELLED;
				order.CancelAt = DateTime.Now;
				order.CancellationReasons = "Fail to create payment link, user should create another order";
				await _unitOfWork.Repositories.orderRepository.Update(order);
				await _unitOfWork.SaveChangesAsync();
				return Result<CreatePaymentResultDto>.Fail(error);
			}
			var getPaymentResultValue = createPaymentLinkResult.Value;
			order.OrderCode = getPaymentResultValue.orderCode;
			order.PaymentLinkId = getPaymentResultValue.paymentLinkId;
			await _unitOfWork.Repositories.orderRepository.Update(order);
			await _unitOfWork.SaveChangesAsync();
			var paymentLinkValue = createPaymentLinkResult.Value;
			return Result<CreatePaymentResultDto>.Success(paymentLinkValue);
		}
		public async Task<Result<Order>> OnReturnUrl(PayosReturnData returnData, Order order)
		{
			var error = new Error();
			if(order.Status == OrderStatus.CANCELLED || order.Status == OrderStatus.PAID) // la khi webhook no tra ve truoc khi ve return url
			{
				return Result<Order>.Success(order);
			}
			order.Status = OrderStatus.PROCESSING;
			order.PaidDate = DateTime.Now;
			order.PriceRemain = 0;
			await _unitOfWork.Repositories.orderRepository.Update(order);
			await _unitOfWork.SaveChangesAsync();
			//await CheckPaymentSuccess(order.OrderCode.Value);
			return Result<Order>.Success(order);
		}

		public async Task<Result> OnCancelUrl(PayosReturnData returnData, Order order)
		{
			var error = new Error();
			if ((order.Status == Shared.Enums.OrderStatus.PENDING) is false)
			{
				error.ErrorMessage = "order is not in a state to be canceled";
				return Result.Fail(error);
			}
			var getOrderCode = order.OrderCode;
			var getPaymentLinkId = order.PaymentLinkId;
			if (getOrderCode == null || getPaymentLinkId == null)
			{
				error.ErrorMessage = "no ordercode or link id is found, which means something wrong with this order";
				return Result.Fail();
			}
			var cancelPaymentLinkResult = await _payosService.CancelPaymentUrl(getOrderCode.Value, "none");
			if (cancelPaymentLinkResult.isSuccess is false)
			{
				error.ErrorMessage = "fail to cancel link payment, might be that the orderCode did not exists at all";
				return Result.Fail();
			}
			var returnedPaymentResult = cancelPaymentLinkResult.Value;
			order.CancelAt = returnedPaymentResult.canceledAt is null ? DateTime.Now : DateTimeHelper.UtcTimeToLocalTime(returnedPaymentResult.canceledAt);
			order.CancellationReasons = returnedPaymentResult.cancellationReason;
			order.Status = Shared.Enums.OrderStatus.CANCELLED;
			await _unitOfWork.Repositories.orderRepository.Update(order);
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
		public async Task<Result> OnFreeOrder(Order order)
		{
			order.Status = OrderStatus.PAID;
			order.PriceRemain = 0;
			order.PricePaid = order.Price;
			await _unitOfWork.Repositories.orderRepository.Update(order);
			await _unitOfWork.SaveChangesAsync();
			return await OnFinishOrder(order);
		}
		public async Task<Result> OnFinishOrder(Order order)
		{
			var getUserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(order.UserId);
			if(getUserProfile == null)
			{
				return Result.Fail();
			}
			var trackIds = order.OrderItems.Select(ot => ot.TrackId).ToList();
			var getTracks = new List<Track>();
			foreach(var trackId in trackIds)
			{
				var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(trackId);
				if (getTrack == null)
					continue;
				getTracks.Add(getTrack);
			}
			var sendResult = await _trackManager.SendMusicAttachmentToEmail(getUserProfile, getTracks);
			if (sendResult.isSuccess is false)
				return Result.Fail(sendResult.Error);
			return Result.Success();
		}
		public async Task<Result<Order>> OnWebhookPaymentReturn(WebhookType webhookType) 
		{
			var error = new Error();
			var orderCode = webhookType.data.orderCode;
			var getOrder = await GetOrderByOrderCode(orderCode);
			var verifyDataResult = await  _payosService.VerifyPayment(webhookType);
			if (verifyDataResult.isSuccess is false)
			{
				getOrder.CancelAt = DateTime.Now;
				getOrder.CancellationReasons = "payment verification fail, might be network attack, ask admin for refund directly";
				getOrder.Status = OrderStatus.CANCELLED;
				await _unitOfWork.Repositories.orderRepository.Update(getOrder);
				await _unitOfWork.SaveChangesAsync();
				error.ErrorMessage = "verify payment seems to fail, try contact manager to ask for refund";
				return Result<Order>.Fail(error);
			}
			var getPaymentLinkInformatoin = await _payosService.GetPaymentLinkInformation(orderCode);
			getOrder.Status = OrderStatus.PAID;
			getOrder.PricePaid = getOrder.Price;
			getOrder.PriceRemain = 0;
			getOrder.PaidDate = DateTime.Now;
			await _unitOfWork.Repositories.orderRepository.Update(getOrder);
			await _unitOfWork.SaveChangesAsync();
			// PHAN NAY TRO DI LA DE UPDATE ORDERTRANSACTION
			var getPaymentTransactions = getPaymentLinkInformatoin.Value.transactions;
			IList<OrderTransaction> orderTransaction = new List<OrderTransaction>();
			foreach(var transaction in getPaymentTransactions ) 
			{
				var newOrderTransaction = new OrderTransaction()
				{
					Reference = transaction.reference,
					Amount = transaction.amount,
					AccountNumber = transaction.accountNumber,
					CounterAccountBankId = transaction.counterAccountBankId,
					CounterAccountBankName = transaction.counterAccountBankName,
					CounterAccountName = transaction.counterAccountName,
					CounterAccountNumber = transaction.counterAccountNumber,
					VirtualAccountName = transaction.virtualAccountName,
					VirtualAccountNumber = transaction.virtualAccountNumber,
					TransactionDateTime = transaction.transactionDateTime,
					OrderId = getOrder.Id,
				};
				orderTransaction.Add(newOrderTransaction);
			}
			var getCurrentOrderTransaction = await _unitOfWork.Repositories.orderTransactionRepository.GetByCondition(ot => ot.OrderId == getOrder.Id);
			if(getCurrentOrderTransaction != null && getCurrentOrderTransaction.Count() > 0) 
			{
				await _unitOfWork.Repositories.orderTransactionRepository.DeleteRange(getCurrentOrderTransaction);
				await _unitOfWork.SaveChangesAsync();
			}
			var createResult = await _unitOfWork.Repositories.orderTransactionRepository.CreateRange(orderTransaction);
			await _unitOfWork.SaveChangesAsync();
			var getNewOrderTransaction = await _unitOfWork.Repositories.orderTransactionRepository.GetByCondition(ot => ot.OrderId == getOrder.Id);

			getOrder.OrderTransactions = getNewOrderTransaction.ToList();
			await _unitOfWork.Repositories.orderRepository.Update(getOrder);
			await _unitOfWork.SaveChangesAsync();
			return Result<Order>.Success(getOrder);
		}
		public async Task<Result> CheckPaymentSuccess(long orderCode)
		{
			var error = new Error();
			var getPaymentResult = await _payosService.GetPaymentLinkInformation(orderCode);
			if (getPaymentResult.isSuccess is false)
			{
				return Result.Fail();
			}
			var paymentResultValue = getPaymentResult.Value;
			var getOrder = await GetOrderByOrderCode(paymentResultValue.orderCode);
			if (getOrder is null)
				return Result.Fail();
			try 
			{
				await _unitOfWork.BeginTransactionAsync();
				switch (paymentResultValue.status)
				{
					case "PAID":
						getOrder.Status = OrderStatus.PAID;
						break;
					case "PROCESSING":
						await _unitOfWork.RollBackAsync();
						return Result.Fail() ;
					default:
						throw new  Exception("orderStatus cannot be detected");
				}
				IList<OrderTransaction> orderTransactions = new List<OrderTransaction>();
				foreach (var transaction in paymentResultValue.transactions)
				{
					var orderTransaction = new OrderTransaction()
					{
						OrderId = getOrder.Id,
						AccountNumber = transaction.accountNumber,
						Amount = transaction.amount,
						CounterAccountBankId = transaction.counterAccountBankId,
						CounterAccountBankName = transaction.counterAccountBankName,
						CounterAccountName = transaction.counterAccountName,
						CounterAccountNumber = transaction.counterAccountNumber,
						TransactionDateTime = transaction.transactionDateTime,
						Reference = transaction.reference,
						VirtualAccountName = transaction.virtualAccountName,
						VirtualAccountNumber = transaction.virtualAccountNumber,
					};
					orderTransactions.Add(orderTransaction);
				}
				getOrder.OrderTransactions.Clear();
				await _unitOfWork.SaveChangesAsync();
				getOrder.OrderTransactions = orderTransactions;
				await _unitOfWork.Repositories.orderRepository.Update(getOrder);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitAsync();
				return Result.Success();
			}
			catch (Exception ex) 
			{
				await _unitOfWork.RollBackAsync();
				error.isException = true;
				error.ErrorMessage = ex.Message;
				error.StatusCode = StatusCodes.Status500InternalServerError;
				return Result.Fail(error);
			}
			
		}
		public async Task<Order?> GetOrderByOrderCode(long orderCode)
		{
			var result = await _unitOfWork.Repositories.orderRepository.GetByCondition(order => order.OrderCode == orderCode, null, "OrderTransactions,OrderItems");
			return result.FirstOrDefault();
		}


		private async Task<IList<Track>> GetTrackItems(IList<CartItem> cartItems)
		{
			var getItemsId = cartItems.Select(ci => ci.ItemId);
			var getResult = await _unitOfWork.Repositories.trackRepository.GetByCondition(t => getItemsId.Contains(t.Id));
			return getResult.ToList();
		}
		private int CountTotalPriceFromTrackItem(IList<Track> tracks)
		{
			int totalPrice = 0;
			foreach (var track in tracks)
			{
				var parsedInt = Convert.ToInt32(track.Price);//.TryParse(track.Price.ToString(), out var result);
				totalPrice += parsedInt;
			}
			return totalPrice;

		}
	}
}
namespace Services.Implementation
{
	public partial class OrderManager
	{
		public async Task<IList<OrderDto>> GetOrdersRangeByUser(UserProfile userProfile, int start, int take)
		{
			if (start < 0 || take < 0)
			{
				return new List<OrderDto>();
			}
			var getOrders = await _unitOfWork.Repositories.orderRepository.GetByCondition(order => order.UserId == userProfile.Id, null, "OrderTransactions,OrderItems", start, take);
			if (getOrders is null)
			{
				return new List<OrderDto>();
			}
			return _mapper.Map<IList<OrderDto>>(getOrders);
		}
	}
}