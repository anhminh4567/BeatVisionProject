using AutoMapper;
using Repository.Interface;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.RequestDto;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class AppUserManager
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ImageFileServices _imageFileServices;
		private readonly CommentService _commentService;
		private readonly AppsettingBinding _appsettings;
		private readonly IMapper _mapper;

		public AppUserManager(IUnitOfWork unitOfWork, ImageFileServices imageFileServices, AppsettingBinding appsettings, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_imageFileServices = imageFileServices;
			_appsettings = appsettings;
			_mapper = mapper;
		}
		public async Task<IList<CartItem>> GetAllUserItems(UserProfile userProfile)
		{
			return (await _unitOfWork.Repositories.cartItemRepository
				.GetByCondition(c => c.UserId == userProfile.Id)).ToList();
		}
		public async Task<CartItem?> GetUserItem(UserProfile userProfile, int itemId)
		{
			return (await _unitOfWork.Repositories.cartItemRepository
				.GetByCondition(item => item.UserId == userProfile.Id && item.Id == itemId)).FirstOrDefault();
		}
		public async Task RemoveUserItem(UserProfile userProfile, int itemId)
		{
			var getItem = await GetUserItem(userProfile, itemId);
			if (getItem is null)
				return;
			await _unitOfWork.Repositories.cartItemRepository.Delete(getItem);
			await _unitOfWork.SaveChangesAsync();
		}
		public async Task<CartItem> AddUserItem(UserProfile userProfile, CartItemType type, int itemId)
		{
			var newItem = new CartItem()
			{
				UserId = userProfile.Id,
				ItemType = type,
				ItemId = itemId
			};
			var result = await _unitOfWork.Repositories.cartItemRepository.Create(newItem);
			await _unitOfWork.SaveChangesAsync();
			return result;
		}
		public async Task<UserProfile?> GetUserProfile(int id)
		{
			return await _unitOfWork.Repositories.userProfileRepository.GetById(id);
		}
		public async Task<UserProfile?> GetUserProfileByIdentity(int identityId)
		{
			return (await _unitOfWork.Repositories.userProfileRepository.GetByCondition(u => u.IdentityId == identityId)).FirstOrDefault();
		}
		public async Task<Result<UserProfile>> UpdateProfile(UserProfile userProfile, UpdateUserProfileDto updateUserProfileDto)
		{
			_mapper.Map(updateUserProfileDto, userProfile);
			await _unitOfWork.Repositories.userProfileRepository.Update(userProfile);
			await _unitOfWork.SaveChangesAsync();
			return Result<UserProfile>.Success(userProfile);
		}
		public async Task<Result<string>> UpdateProfileImage(Stream filestream, string contentType, string fileName, UserProfile userProfile, CancellationToken cancellationToken = default)
		{
			var randomGeneratedFilename = Guid.NewGuid().ToString();
			var oldImageUrl = userProfile.ProfileBlobUrl;
			var returnedFileUrl = await _imageFileServices.UploadNewImage(filestream, fileName, randomGeneratedFilename, contentType, cancellationToken);
			if (returnedFileUrl.isSuccess is false)
			{
				return Result.Fail(returnedFileUrl.Error);
			}
			userProfile.ProfileBlobUrl = returnedFileUrl.Value;
			await _unitOfWork.Repositories.userProfileRepository.Update(userProfile);
			if (string.IsNullOrEmpty(oldImageUrl) is false)
			{
				var deleteResult = await _imageFileServices.DeleteImageFile(oldImageUrl, cancellationToken);
			}
			return Result.Success();
		}
		public async Task<Result> ReadNotification(UserProfile user, Message message)
		{
			var error = new Error();
			var getNotifcationOfUser = (await _unitOfWork.Repositories.notificationRepository
				.GetByCondition(noti => noti.ReceiverId == user.Id && noti.MessageId == message.Id)).FirstOrDefault();
			if (getNotifcationOfUser == null)
			{
				error.ErrorMessage = "can't find message";
				return Result.Fail();
			}
			var result = await _unitOfWork.Repositories.notificationRepository.Delete(getNotifcationOfUser);
			await _unitOfWork.SaveChangesAsync();
			return result is null ? Result.Fail() : Result.Success();
		}
		public async Task<Result> ReadAllNotification(UserProfile user)
		{
			var error = new Error();
			var getAllNotifcationsOfUser = (await _unitOfWork.Repositories.notificationRepository
				.GetByCondition(noti => noti.ReceiverId == user.Id));
			var result = await _unitOfWork.Repositories.notificationRepository.DeleteRange(getAllNotifcationsOfUser.ToList());
			await _unitOfWork.SaveChangesAsync();
			return result ? Result.Success() : Result.Fail();
		}
		public async Task<Result<IList<TrackCommentDto>>> GetUserTrackComments(UserProfile userProfile)
		{
			var error = new Error();
			var getUserTrackComments = await _commentService.GetAllUserTrackComment(userProfile);
			if (getUserTrackComments == null)
			{
				error.ErrorMessage = "Fail to get user track comments";
				return Result<IList<TrackCommentDto>>.Fail(error);
			}
			var returnResult = _mapper.Map<IList<TrackCommentDto>>(getUserTrackComments);
			return Result<IList<TrackCommentDto>>.Success(returnResult);
		}
		public async Task<Result<TrackCommentDto>> CreateComment(UserProfile userProfile,CreateTrackCommentDto createTrackCommentDto, int? replyToComment_Id = null)
		{
			var createResult = await _commentService.CreateTrackComment(userProfile, createTrackCommentDto);
			var mapResponse = _mapper.Map<TrackCommentDto>(createResult);
			return Result<TrackCommentDto>.Success(mapResponse);
		}
		public async Task<Result> RemoveComment(UserProfile userProfile, int userCommentId)
		{
			if (userCommentId == null)
				return Result.Fail();
			return  await _commentService.RemoveComment(userProfile,userCommentId);
		}
	}
}
