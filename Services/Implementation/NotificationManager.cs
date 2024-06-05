using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Repository.Interface;
using Services.Interface;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.Poco;
using Shared.RequestDto;
using Shared.ResponseDto;
using StackExchange.Redis;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Services.Implementation
{
	public class NotificationManager
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IMyEmailService _mailService;
		private readonly AppsettingBinding _appsettings;
		public NotificationManager(IUnitOfWork unitOfWork, IMapper mapper, IMyEmailService mailService, AppsettingBinding appsettingBinding)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_mailService = mailService;
			_appsettings = appsettingBinding;
		}
		//public async Task<Result> CreateNotificationToGroups(UserProfile userProfile, CreateMessageDto create, IList<int> receiversId)
		//{
		//	return  await CreateNotification(create,NotificationType.GROUP,create.Weight,receiversId,false,userProfile.Id);
		//}
		//public async Task<Result> CreateNotificationToUser(UserProfile userProfile, CreateMessageDto create, int receiverId)
		//{
		//	IList<int> receiver = new List<int>{ receiverId };
		//	return await CreateNotification(create, NotificationType.SINGLE, create.Weight, receiver, false, userProfile.Id);
		//}
		//public async Task<Result> CreateNotificationToAll(UserProfile userProfile, CreateMessageDto create)
		//{
		//	return await CreateNotification(create, NotificationType.ALL, create.Weight, null, false, userProfile.Id);
		//}
		public async Task<Result> ServerCreateNotificationToGroups(CreateMessageDto create, IList<int> receiversId)
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
			var result = await CreateNotification(create, NotificationType.ALL, create.Weight, null, true);
			return result;
		}
		public async Task<Result> AdminCreateNotification(AdminCreateMessageDto adminCreateMessageDto)
		{
			var error = new Error();
			switch(adminCreateMessageDto.Type)
			{
				case NotificationType.ALL:
					var createResult =await ServerCreateNotificationToAll(adminCreateMessageDto);
					if (createResult.isSuccess is false)
						return Result.Fail(createResult.Error);
					break;
				case NotificationType.GROUP:
					var getSubscriber = (await GetAllSubscriber()).Select(s => s.Id); 
					var createResultGroup = await ServerSendNotificationMail_ToGroups(adminCreateMessageDto,getSubscriber.ToArray());
					if (createResultGroup.isSuccess is false)
						return Result.Fail(createResultGroup.Error);
					break;
				case NotificationType.SINGLE:
					var createResultSingle = await ServerSendNotificationMail_ToUser(adminCreateMessageDto,adminCreateMessageDto.UserId.Value);
					if (createResultSingle.isSuccess is false)
						return Result.Fail(createResultSingle.Error);
					break;
				default:
					error.ErrorMessage = "no appropriate type found";
					return Result.Fail(error);
			}
			return Result.Success();
		}
		public async Task<Result> AdminRemoveMessage(int messageId)
		{
			var error = new Error();
			var tryGetMessage = await _unitOfWork.Repositories.messageRepository.GetById(messageId);
			if(tryGetMessage is null)
			{
				error.ErrorMessage = "fail to get message";
				return Result.Fail(error);
			}
			await _unitOfWork.Repositories.messageRepository.Delete(tryGetMessage);
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
		public async Task<Result> ServerSendNotificationMail(CreateNotificationForNewTracks notificationForNewTracks)
		{
			var getSubscribers = (await GetAllSubscriber());
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(notificationForNewTracks.TrackId);
			if (getTrack is null)
				return Result.Fail();
			var result = await CreateNotification(notificationForNewTracks, NotificationType.GROUP, notificationForNewTracks.Weight, getSubscribers.Select(u => u.Id).ToList(), true);
			if (result.isSuccess is false)
				return result;
			var getSubsctiber = await GetAllSubscriber();
			foreach (var subscriber in getSubsctiber)
			{
				var meta = new EmailMetaData()
				{
					ToEmail = subscriber.IdentityUser.Email,
					Subject = notificationForNewTracks.MessageName,
				};
				var notiModel = new NotificationEmailModel()
				{
					Content = notificationForNewTracks.Content,
					NotificationType = NotificationType.GROUP.ToString(),
					SendTime = DateTime.Now,
					Title = "New track have been publish",
					TrackToPublish = _mapper.Map<TrackResponseDto>(getTrack),
					UserToSend = subscriber,
					Weight = notificationForNewTracks.Weight.ToString(),
				};
				_mailService.SendEmailWithTemplate<NotificationEmailModel>(meta, _appsettings.MailTemplateAbsolutePath.FirstOrDefault(m => m.TemplateName.Equals("NotificationEmail")).TemplateAbsolutePath, notiModel);
			}
			return Result.Success();
		}
		public async Task<Result> ServerSendNotificationMail_ToGroups(CreateMessageDto createMessageDto, params int[] profileId)
		{
			var getUser = await _unitOfWork.Repositories.userProfileRepository.GetByCondition(u => profileId.Contains(u.Id), null, "IdentityUser");
			if (getUser == null)
				return Result.Fail();
			var result = await CreateNotification(createMessageDto, NotificationType.GROUP, NotificationWeight.MAJOR, profileId, true);
			if (result.isSuccess is false)
				return Result.Fail();
			foreach (var user in getUser)
			{
				if( user.IsSubcribed is false) 
					continue;
				var meta = new EmailMetaData()
				{
					ToEmail = user.IdentityUser.Email,
					Subject = createMessageDto.MessageName,
				};
				var notiModel = new NotificationEmailModel()
				{
					Content = createMessageDto.Content,
					NotificationType = NotificationType.GROUP.ToString(),
					SendTime = DateTime.Now,
					Title = createMessageDto.MessageName,
					TrackToPublish = null,
					UserToSend = _mapper.Map<UserProfileDto>(user),
					Weight = createMessageDto.Weight.ToString(),
				};
				_mailService.SendEmailWithTemplate<NotificationEmailModel>(meta, _appsettings.MailTemplateAbsolutePath.FirstOrDefault(m => m.TemplateName.Equals("NotificationEmail")).TemplateAbsolutePath, notiModel);

			}
			return Result.Success();
		}
		public async Task<Result> ServerSendNotificationMail_ToUser(CreateMessageDto createMessageDto, int profileId)
		{
			var getUser = await _unitOfWork.Repositories.userProfileRepository.GetByIdInclude(profileId, "IdentityUser");//(u => profileId.Contains(u.Id), null, "IdentityUser");
			if (getUser == null)
				return Result.Fail();
			var result = await CreateNotification(createMessageDto, NotificationType.SINGLE, NotificationWeight.MINOR, new int[] { profileId }, true);
			if (result.isSuccess is false)
				return Result.Fail();
			var meta = new EmailMetaData()
			{
				ToEmail = getUser.IdentityUser.Email,
				Subject = createMessageDto.MessageName,
			};
			var notiModel = new NotificationEmailModel()
			{
				Content = createMessageDto.Content,
				NotificationType = NotificationType.GROUP.ToString(),
				SendTime = DateTime.Now,
				Title = createMessageDto.MessageName,
				TrackToPublish = null,
				UserToSend = _mapper.Map<UserProfileDto>(getUser),
				Weight = createMessageDto.Weight.ToString(),
			};
			if(getUser.IsSubcribed is false)
			{
				return Result.Success();
			}
			_mailService.SendEmailWithTemplate<NotificationEmailModel>(meta, _appsettings.MailTemplateAbsolutePath.FirstOrDefault(m => m.TemplateName.Equals("NotificationEmail")).TemplateAbsolutePath, notiModel);
			return Result.Success();
		}

		public async Task RemoveExpiredMessage()
		{
			var currentDateTime = DateTime.Now;
			var getExpiredNotification = (await _unitOfWork.Repositories.notificationRepository
				.GetByCondition(noti => noti.ExpiredDate <= currentDateTime));
			foreach (var noti in getExpiredNotification)
			{
				await _unitOfWork.Repositories.notificationRepository.Delete(noti);
			}
			await _unitOfWork.SaveChangesAsync();
		}
		public async Task<Result> CreateNotification(CreateMessageDto create,
			NotificationType sendScope,
			NotificationWeight weight,
			IList<int> receiverIds,
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
				CreatorId = isServerMessage ? null : creatorId,
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
							ExpiredDate = createDate.AddDays((int)weight),
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
		public async Task<Result<PagingResponseDto< IList<MessageDto>>>> GetServerNotificationRange(int start, int take, NotificationType? type = null, bool orderByCreateDate = true)  
		{
			var error = new Error();
			if (start < 0 || take < 0)
			{
				error.ErrorMessage = "start and take not legit";
				return Result<PagingResponseDto<IList<MessageDto>>>.Fail();
			}
			IList<Message> messages = new List<Message>();
			if(type == null)
			{
				messages = ( await _unitOfWork.Repositories.messageRepository.GetByCondition(m => m.CreatorId == null,null,"",start,take) ).ToList();
			}
			else
			{
				messages = (await _unitOfWork.Repositories.messageRepository.GetByCondition(m => m.CreatorId == null && m.Type == type, null, "", start, take)).ToList();
			}
			if(orderByCreateDate)
			{
				messages = messages.OrderBy(m => m.CreatedDate).ToList();
			}
			var mappedResult = _mapper.Map<IList<MessageDto>>(messages);
			return Result<PagingResponseDto<IList<MessageDto>>>.Success(new PagingResponseDto<IList<MessageDto>> 
			{ 
				TotalCount = _unitOfWork.Repositories.messageRepository.COUNT ,
				Value = mappedResult,
			});

		}
		public async Task<Result<IList<NotificationDto>>> GetUserNotifications(int userProfileId, bool getIsNotReadOnly = false)
		{
			var error = new Error();
			var getUserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if (getUserProfile == null)
			{
				error.ErrorMessage = "canot found user profile";
				return Result<IList<NotificationDto>>.Fail(error);
			}
			IList<Notification> getList;
			if(getIsNotReadOnly) 
			{
				getList = (await _unitOfWork.Repositories.notificationRepository
				.GetByCondition(noti => noti.ReceiverId == userProfileId && noti.IsReaded == false, null, "Message")).ToList();
			}
			else
			{
				getList = (await _unitOfWork.Repositories.notificationRepository
				.GetByCondition(noti => noti.ReceiverId == userProfileId, null, "Message")).ToList();
			}
			if(getList == null)
			{
				return Result<IList<NotificationDto>>.Fail(error);
			}
			var returnResult = _mapper.Map<IList<NotificationDto>>(getList);
			return Result<IList<NotificationDto>>.Success(returnResult);
		}
		public async Task<Result> ReadNotification(int userProfileId, int messageId)
		{
			var error = new Error();
			var getUserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if (getUserProfile == null)
			{
				error.ErrorMessage = "canot found user profile";
				return Result.Fail(error);
			}
			var getNoti = ( await _unitOfWork.Repositories.notificationRepository
				.GetByCondition(noti => noti.MessageId == messageId && noti.ReceiverId == userProfileId) ).FirstOrDefault();
			if(getNoti is null)
			{
				error.ErrorMessage = "cannot found notification";
				return Result.Fail(error);
			}
			getNoti.IsReaded = true;
			await _unitOfWork.Repositories.notificationRepository.Update(getNoti);
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
		public async Task<Result> ReadAllNotifications(int userProfileId)
		{
			var error = new Error();
			var getUserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if (getUserProfile == null)
			{
				error.ErrorMessage = "canot found user profile";
				return Result.Fail(error);
			}
			var getAllNotiNotRead = await _unitOfWork.Repositories.notificationRepository
				.GetByCondition(noti => noti.ReceiverId == userProfileId && noti.IsReaded == false);
			foreach(var noti in getAllNotiNotRead)
			{
				noti.IsReaded = true;
				await _unitOfWork.Repositories.notificationRepository.Update(noti);
			}
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
		private async Task<IList<UserProfileDto>> GetAllSubscriber()
		{
			//var getIdentities = await _unitOfWork.Repositories.customIdentityUser.GetByCondition(u => u.UserProfile.);
			var getList = await _unitOfWork.Repositories.userProfileRepository.GetByCondition(u => u.IsSubcribed == true, null, "IdentityUser");
			var mappedList = _mapper.Map<IList<UserProfileDto>>(getList);
			return mappedList;
		}
		private async Task<IList<UserProfileDto>> GetSubcribers(params int[] subcribersId)
		{
			var getList = await _unitOfWork.Repositories.userProfileRepository.GetByCondition(u => subcribersId.Contains(u.Id), null, "IdentityUser");
			return _mapper.Map<IList<UserProfileDto>>(getList);
		}

	}
}