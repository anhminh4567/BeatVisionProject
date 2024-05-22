using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class AppUserManager
	{
		private readonly ImageFileServices _imageFileServices;
		private readonly IUnitOfWork _unitOfWork;
		private readonly NotificationManager _notificationManager;

		public AppUserManager(ImageFileServices imageFileServices, IUnitOfWork unitOfWork, NotificationManager notificationManager)
		{
			_imageFileServices = imageFileServices;
			_unitOfWork = unitOfWork;
			_notificationManager = notificationManager;
		}
	}
}
