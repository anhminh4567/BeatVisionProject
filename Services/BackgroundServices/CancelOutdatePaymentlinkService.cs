using Microsoft.EntityFrameworkCore.Query.Internal;
using Quartz;
using Repository.Interface;
using Services.Implementation;
using Shared.ConfigurationBinding;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BackgroundServices
{
	internal class CancelOutdatePaymentlinkService : IJob
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly AppUserManager _appUserManager;
		private readonly OrderManager _orderManager;
		private readonly PayosService _payosService;
		private readonly AppsettingBinding _appsettings;

		public CancelOutdatePaymentlinkService(IUnitOfWork unitOfWork, AppUserManager appUserManager, OrderManager orderManager, PayosService payosService, AppsettingBinding appsettings)
		{
			_unitOfWork = unitOfWork;
			_appUserManager = appUserManager;
			_orderManager = orderManager;
			_payosService = payosService;
			_appsettings = appsettings;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var getTimeToExpireLink = _appsettings.AppConstraints.LinkExpirationTimeMinute;
			var getOrderThatExpired = (await _unitOfWork.Repositories.orderRepository
				.GetByCondition(order => 
					DateTime.Compare(order.CreateDate.Value.AddMinutes(getTimeToExpireLink),DateTime.Now) <= 0 && 
					order.Status == Shared.Enums.OrderStatus.PENDING) )
				.ToList();
			var taskList = new List<Task>();
			foreach(var order in getOrderThatExpired)
			{
				Console.WriteLine(order.Id);
				//taskList.Add(new Task(async () => await CancelPaymentOutOfTime(order)));
				await CancelPaymentOutOfTime(order);
				// await thay vi cho nhieu task chay cung luc do payos co rate limiting, goi qua nhieu lan no se block, 
				// con so ko ro la bao nhieu nen await moi task cho chac
			}
			
			
		}
		private async Task CancelPaymentOutOfTime(Order order)
		{
			var orderCode = order.OrderCode;
			var cancelPaymentLinkResult = await _payosService.CancelPaymentUrl(orderCode.Value,"time for this link is expired");
			if(cancelPaymentLinkResult.isSuccess is false )
			{
				return;
			}
			var getCancelValue = cancelPaymentLinkResult.Value;
			order.Status = Shared.Enums.OrderStatus.CANCELLED;
			await _unitOfWork.Repositories.orderRepository.Update(order);
			await _unitOfWork.SaveChangesAsync();
		}
	}
}
