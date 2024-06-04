using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Identity.Client;
using Net.payOS.Types;
using Repository.Interface;
using Services.Interface;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.Poco;
using Shared.RequestDto;
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
		private readonly IMyEmailService _emailService;
		private readonly AppUserManager _userManager;
		private readonly PayosService _payosService;
		private readonly TrackManager _trackManager;
		private readonly UserIdentityServices _userIdentityServices;
		private readonly NotificationManager _notificationManager;
		private readonly UserIdentityServices _userIdentityService;
		private readonly AppsettingBinding _appsettingBinding;

		public OrderManager(IUnitOfWork unitOfWork, IMapper mappter, AppUserManager userManager, PayosService payosService, TrackManager trackManager, UserIdentityServices userIdentityServices, NotificationManager notificationManager, UserIdentityServices userIdentityService, AppsettingBinding appsettingBinding, IMyEmailService emailService)
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
			_emailService = emailService;
		}
		public async Task<Result<CreatePaymentResultDto>> Checkout(int userProfileId) 
		{
			var error = new Error();
			var getuserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if(getuserProfile == null)
			{
				//error.ErrorMessage = "canot found user profile";
				return Result<CreatePaymentResultDto>.Fail(error);
			}
			var checkIfAccountLegit = await _userManager.IsUserLegit(getuserProfile);
			if (checkIfAccountLegit.isSuccess is false) 
			{
				//error.ErrorMessage = "user is not confirmed email or account is banned";
				return Result<CreatePaymentResultDto>.Fail(checkIfAccountLegit.Error);
			}
			//var getUserOldOrders_include_orderItems = await _unitOfWork.Repositories.orderRepository
			//	.GetByCondition(order => order.UserId == getuserProfile.Id, null, "OrderItems");
			//var getUserCartItem = await _unitOfWork.Repositories.cartItemRepository
			//	.GetByCondition(item => item.UserId == getuserProfile.Id);
			//var checkIfCartItemLegit = IsUserCartItemLegit(getUserCartItem.ToList(), getUserOldOrders_include_orderItems.ToList());
			//if(checkIfCartItemLegit is false)
			//{
			//	error.ErrorMessage = "cart item contain item that have been bought before";
			//	return Result<CreatePaymentResultDto>.Fail();
			//}	
			var placeOrderResult = await PlaceOrder(getuserProfile);
			if(placeOrderResult.isSuccess is false)
			{
				//error.ErrorMessage = "fail to place an order, error in creating it";
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
			var getUserIdentity = await _userIdentityService.UserManager.FindByIdAsync(userProfile.IdentityId.ToString()); // _unitOfWork.Repositories.customIdentityUser.GetById(userProfile.IdentityId);
			if((await _userIdentityService.UserManager.IsEmailConfirmedAsync(getUserIdentity)) is false)// email not confirmed
			{

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
				return Result.Fail(error);
			}
			var cancelPaymentLinkResult = await _payosService.CancelPaymentUrl(getOrderCode.Value, "none");
			if (cancelPaymentLinkResult.isSuccess is false)
			{
				error.ErrorMessage = "fail to cancel link payment, might be that the orderCode did not exists at all";
				return Result.Fail(error);
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
			var error = new Error();
			var getUserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(order.UserId);
			if(getUserProfile == null)
			{
				error.ErrorMessage = "cannot found user profile";
				return Result.Fail();
			}
			var trackIds = order.OrderItems.Select(ot => ot.TrackId).ToList();
			var getTracks = new List<Track>();
			foreach (var trackId in trackIds)
			{
				var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(trackId);
				if (getTrack == null)
					continue;
				getTracks.Add(getTrack);
			}
			//var sendResult = await _trackManager.SendMusicAttachmentToEmail(getUserProfile, getTracks);
			//if (sendResult.isSuccess is false)
			//	return Result.Fail(sendResult.Error);
			var messageDto = new CreateMessageDto
			{
				Content = $"your have just bought {getTracks.Count} track",
				MessageName = "please get your track in the order section on our site",
				Weight = NotificationWeight.MINOR,
			};
			var sendNotificatoinMail = await _notificationManager.ServerSendNotificationMail_ToUser(messageDto,getUserProfile.Id);
			return Result.Success();
		}
		public async Task<Result<Order>> OnWebhookPaymentReturn(WebhookType webhookType) 
		{
			var error = new Error();
			var orderCode = webhookType.data.orderCode;
			var getOrder = await GetOrderByOrderCode(orderCode);
			if(getOrder.Status == OrderStatus.PAID) 
			{
				error.ErrorMessage = "ORDER IS PAID LONG AGO";
				return Result<Order>.Fail();
			}
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
			//var getPaymentLinkInformatoin = await _payosService.GetPaymentLinkInformation(orderCode);
			getOrder.Status = OrderStatus.PAID;
			getOrder.PricePaid = getOrder.Price;
			getOrder.PriceRemain = 0;
			getOrder.PaidDate = DateTime.Now;
			await _unitOfWork.Repositories.orderRepository.Update(getOrder);
			await _unitOfWork.SaveChangesAsync();
			var transactionData = webhookType.data;
			var newOrderTransaction = new OrderTransaction()
			{
				AccountNumber = transactionData.accountNumber,
				Amount = transactionData.amount,
				CounterAccountBankId = transactionData.counterAccountBankId,
				CounterAccountBankName = transactionData.counterAccountBankName,
				CounterAccountName = transactionData.counterAccountName,
				CounterAccountNumber = transactionData.counterAccountNumber,
				OrderId = getOrder.Id,
				Reference = transactionData.reference,
				VirtualAccountNumber = transactionData.virtualAccountNumber,
				TransactionDateTime = transactionData.transactionDateTime,
				VirtualAccountName = transactionData.virtualAccountName,
			};
			await _unitOfWork.Repositories.orderTransactionRepository.Create(newOrderTransaction);
			await _unitOfWork.SaveChangesAsync();
			getOrder.OrderTransactions.Add(newOrderTransaction);
			await _unitOfWork.Repositories.orderRepository.Update(getOrder);
			await _unitOfWork.SaveChangesAsync();
			//// PHAN NAY TRO DI LA DE UPDATE ORDERTRANSACTION
			//var getPaymentTransactions = getPaymentLinkInformatoin.Value.transactions;
			//IList<OrderTransaction> orderTransaction = new List<OrderTransaction>();
			//foreach(var transaction in getPaymentTransactions ) 
			//{
			//	var newOrderTransaction = new OrderTransaction()
			//	{
			//		Reference = transaction.reference,
			//		Amount = transaction.amount,
			//		AccountNumber = transaction.accountNumber,
			//		CounterAccountBankId = transaction.counterAccountBankId,
			//		CounterAccountBankName = transaction.counterAccountBankName,
			//		CounterAccountName = transaction.counterAccountName,
			//		CounterAccountNumber = transaction.counterAccountNumber,
			//		VirtualAccountName = transaction.virtualAccountName,
			//		VirtualAccountNumber = transaction.virtualAccountNumber,
			//		TransactionDateTime = transaction.transactionDateTime,
			//		OrderId = getOrder.Id,
			//	};
			//	orderTransaction.Add(newOrderTransaction);
			//}
			//var getCurrentOrderTransaction = await _unitOfWork.Repositories.orderTransactionRepository.GetByCondition(ot => ot.OrderId == getOrder.Id);
			//if(getCurrentOrderTransaction != null && getCurrentOrderTransaction.Count() > 0) 
			//{
			//	await _unitOfWork.Repositories.orderTransactionRepository.DeleteRange(getCurrentOrderTransaction);
			//	await _unitOfWork.SaveChangesAsync();
			//}
			//var createResult = await _unitOfWork.Repositories.orderTransactionRepository.CreateRange(orderTransaction);
			//await _unitOfWork.SaveChangesAsync();
			//var getNewOrderTransaction = await _unitOfWork.Repositories.orderTransactionRepository.GetByCondition(ot => ot.OrderId == getOrder.Id);

			//getOrder.OrderTransactions = getNewOrderTransaction.ToList();
			//await _unitOfWork.Repositories.orderRepository.Update(getOrder);
			//await _unitOfWork.SaveChangesAsync();
			SendBillingEmail(getOrder.Id,getOrder.UserId);
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
		public async Task<Result> SendBillingEmail(int orderId, int userProfileId)
		{
			var getOrderFull = await GetOrderDetail(orderId);
			if(getOrderFull == null) 
			{
				return Result.Fail();
			}
			var getUserProfile = await _unitOfWork.Repositories.userProfileRepository.GetByIdInclude(userProfileId, "IdentityUser");
			if(getUserProfile is null)
				return Result.Fail();
			var getBillingEmailTemplatePath = _appsettingBinding.MailTemplateAbsolutePath.FirstOrDefault(t => t.TemplateName == "BillingEmail")?.TemplateAbsolutePath;
			if (string.IsNullOrEmpty(getBillingEmailTemplatePath))
				return Result.Fail();
			var mailMetadata = new EmailMetaData()
			{
				ToEmail = getUserProfile.IdentityUser.Email,
				Subject = "Billing for your order",
			};
			var mappedOrderDto = _mapper.Map<OrderDto>(getOrderFull);
			var billingModel = new BillingEmailModel()
			{
				Order = mappedOrderDto,
				OrderItems = mappedOrderDto.OrderItems,
				ToMainPage = null,
				UserToSend = _mapper.Map<UserProfileDto>(getUserProfile),
			};
			return await _emailService.SendEmailWithTemplate<BillingEmailModel>(mailMetadata,getBillingEmailTemplatePath, billingModel) ;	
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
		// oldOrder phai include het item, de tranh viec db bi goi qua nhieu lan
		private bool IsUserCartItemLegit(IList<CartItem> cartItems, IList<Order> oldOrders) 
		{
			var cartTrackIds = cartItems.Where(item => item.ItemType == CartItemType.TRACK).Select(item => item.ItemId);
			IList<int> oldItem_track_id = new List<int>();
			foreach(var order in oldOrders)
			{
				if(order.OrderItems is null || order.OrderItems.Any() is false) 
					return false;
				foreach(var item in order.OrderItems)
				{
					oldItem_track_id.Add(item.TrackId);
				}
			}
			foreach (var trackcartId in cartTrackIds) 
			{
				if (oldItem_track_id.Contains(trackcartId));
				return false;
			}
			return true;
		}
	}
}
namespace Services.Implementation
{
	public partial class OrderManager
	{
		public async Task<Order?> GetOrderDetail(int orderId)
		{
			return (await _unitOfWork.Repositories.orderRepository.GetByIdInclude(orderId, "OrderItems,OrderTransactions"));
		}
		public Result IsOrderLegitForDownload(Order order)
		{
			var error = new Error();
			if(order.Status != OrderStatus.PAID)
			{
				error.ErrorMessage = "order is not paid yet";
				return Result.Fail();
			}
			return Result.Success();
		}
		public async Task<Result<IList<OrderDto>>> GetOrdersRangeByUser(int userProfileId, int start, int take, OrderStatus? status = null)
		{
			var error = new Error();
			var getProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if (getProfile is null)
			{
				error.ErrorMessage = "fail to get user profile";
				return Result<IList<OrderDto>>.Fail(error);
			}
			var getOrderList = await GetOrdersRangeByUser(getProfile,start, take, status);
			return Result<IList<OrderDto>>.Success(getOrderList);
		}
		public async Task<IList<OrderDto>> GetOrdersRangeByUser(UserProfile userProfile, int start, int take, OrderStatus? status = null)
		{
			if (start < 0 || take < 0)
			{
				return new List<OrderDto>();
			}
			IList<Order> getOrderList;// = new List<Order>();
			if(status is null)
			{
				getOrderList = (await _unitOfWork.Repositories.orderRepository.GetByCondition(order => order.UserId == userProfile.Id, null, "OrderTransactions,OrderItems", start, take)).ToList();
			}
			else
			{
				getOrderList = (await _unitOfWork.Repositories.orderRepository.GetByCondition(order => order.UserId == userProfile.Id && order.Status.Equals(status.Value), null, "OrderTransactions,OrderItems", start, take)).ToList();
			}
			if (getOrderList is null)
			{
				return new List<OrderDto>();
			}
			return _mapper.Map<IList<OrderDto>>(getOrderList);
		}
	}
}
