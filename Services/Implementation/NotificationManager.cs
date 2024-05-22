using Repository.Interface;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.RequestDto;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Services.Implementation
{
	public class NotificationManager
	{
		private readonly IUnitOfWork _unitOfWork;

		public NotificationManager(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public async Task<Result> CreateNotificationToGroups(UserProfile userProfile, CreateMessageDto create, IList<int> receiversId)
		{
			return  await CreateNotification(create,NotificationType.GROUP,create.Weight,receiversId,false,userProfile.Id);
		}
		public async Task<Result> CreateNotificationToUser(UserProfile userProfile, CreateMessageDto create, int receiverId)
		{
			IList<int> receiver = new List<int>{ receiverId };
			return await CreateNotification(create, NotificationType.SINGLE, create.Weight, receiver, false, userProfile.Id);
		}
		public async Task<Result> CreateNotificationToAll(UserProfile userProfile, CreateMessageDto create)
		{
			return await CreateNotification(create,NotificationType.ALL,create.Weight,null,false,userProfile.Id);
		}
		public async Task<Result> ServerCreateNotificationToGroups( CreateMessageDto create, IList<int> receiversId)
		{
			return await CreateNotification(create, NotificationType.GROUP, create.Weight, receiversId, true);
		}
		public async Task<Result> ServerCreateNotificationToUser(CreateMessageDto create, int receiverId)
		{
			IList<int> receiver = new List<int> { receiverId };
			return await CreateNotification(create, NotificationType.SINGLE, create.Weight, receiver, true);
		}
		public async Task<Result> ServerCreateNotificationToAll(CreateMessageDto create)
		{
			return await CreateNotification(create, NotificationType.ALL, create.Weight, null, true);
		}
		public async Task RemoveExpiredMessage()
		{
			var currentDateTime = DateTime.Now;
			var getExpiredNotification = (await _unitOfWork.Repositories.notificationRepository
				.GetByCondition(noti => noti.ExpiredDate <= currentDateTime));
			foreach(var noti in getExpiredNotification) 
			{
				await _unitOfWork.Repositories.notificationRepository.Delete(noti);
			}
			await _unitOfWork.SaveChangesAsync();
			 
		}
		public async Task<Result> CreateNotification(CreateMessageDto create,
			NotificationType sendScope,
			NotificationWeight weight,
			IList<int> receiverIds ,
			bool isServerMessage = true,
			int creatorId = -1)
		{
			var error = new Error();
			if (sendScope != NotificationType.ALL && receiverIds is null)
			{
				error.ErrorMessage = "unknown who to send";
				return Result.Fail();
			}
			if (sendScope == NotificationType.ALL)
			{
				receiverIds = new List<int>();
			}
			var createDate = DateTime.Now;
			var newMessage = new Message()
			{
				Content = create.Content,
				CreatorId = creatorId,
				IsServerNotification = isServerMessage,
				CreatedDate = createDate,
				Type = sendScope,
				MessageName = create.MessageName,
				Weight = weight,
			};
			IList<Notification> receiversNoti = new List<Notification>();
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				var result = await _unitOfWork.Repositories.messageRepository.Create(newMessage);
				await _unitOfWork.SaveChangesAsync();
				switch (sendScope)
				{
					case NotificationType.ALL:
						receiverIds = await _unitOfWork.Repositories.userProfileRepository
							.GetByCondition_selectReturn<int>(select => select.Id);
						foreach (var receiver in receiverIds)
						{
							receiversNoti.Add(new Notification()
							{
								ExpiredDate = createDate.AddDays((int)weight),
								ReceiverId = receiver,
								IsReaded = false,
								MessageId = result.Id,
							});
						}
						break;
					case NotificationType.GROUP:
						foreach (var receiver in receiverIds)
						{
							receiversNoti.Add(new Notification()
							{
								ExpiredDate = createDate.AddDays((int)weight),
								ReceiverId = receiver,
								IsReaded = false,
								MessageId = result.Id,
							});
						}
						break;
					case NotificationType.SINGLE:
						var getFirstReceiverId = receiverIds.FirstOrDefault();
						receiversNoti.Add(new Notification() 
						{
							ExpiredDate = createDate.AddDays((int) weight),
							ReceiverId = getFirstReceiverId,
							IsReaded = false,
							MessageId = result.Id,
						});
						break;
					default:
						error.ErrorMessage = "send scope is not correct";
						await _unitOfWork.RollBackAsync();
						return Result.Fail(error);
				}
				var addResult = await _unitOfWork.Repositories.notificationRepository.CreateRange(receiversNoti);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitAsync();
				return Result.Success();
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollBackAsync();
				error.isException = true;
				error.ErrorMessage = ex.Message;
				return Result.Fail();
			}
		}
	}
}