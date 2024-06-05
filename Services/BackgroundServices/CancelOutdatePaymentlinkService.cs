using Microsoft.EntityFrameworkCore.Query.Internal;
using Quartz;
using Repository.Interface;
using Services.Implementation;
using Shared.ConfigurationBinding;
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
			var getOrderThatExpired = (await _unitOfWork.Repositories.orderRepository.GetByCondition(order => 
			DateTime.Compare(order.CreateDate.Value.AddMinutes(getTimeToExpireLink),DateTime.Now) <= 0 ) ).ToList();
			
			foreach(var order in getOrderThatExpired)
			{
				Console.WriteLine(order.Id);
			}
			//Console.WriteLine("count: " + ThreadPool.ThreadCount);
			//Console.WriteLine("count current: " + Thread.CurrentThread.Name );

			//throw new NotImplementedException();
		}
	}
}
