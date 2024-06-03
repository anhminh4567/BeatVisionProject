using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repository.Interface;
using Shared.ConfigurationBinding;
using Shared.Helper;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class OrderManager
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mappter;
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
			_mappter = mappter;
			_userManager = userManager;
			_payosService = payosService;
			_trackManager = trackManager;
			_userIdentityServices = userIdentityServices;
			_notificationManager = notificationManager;
			_userIdentityService = userIdentityService;
			_appsettingBinding = appsettingBinding;
		}

		public async Task<Result> PlaceOrder(UserProfile userProfile)
		{
			var error = new Error();
			var getUserCartItem = (await _unitOfWork.Repositories.cartItemRepository.GetByCondition(c => c.UserId == userProfile.Id)).ToList(); //_userManager.GetAllUserCartItems(userProfile);
			if (getUserCartItem is null || getUserCartItem.Count == 0)
			{
				error.ErrorMessage = "nothing to buy in cart";
				return Result.Fail(error);
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
		//public async Task<Result> CancelOrder(Order order) 
		//{
		//	//var pay
		//	//order.Status = Shared.Enums.OrderStatus.CANCELLED;
		//}
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
				var parsedInt = int.TryParse(track.Price.ToString(), out var result);
				if (parsedInt is false)
				{
					continue;
				}
				totalPrice += result;
			}
			return totalPrice;

		}
	}
}
