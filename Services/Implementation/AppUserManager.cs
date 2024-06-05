using AutoMapper;
using CloudinaryDotNet.Actions;
using NAudio.Codecs;
using Repository.Interface;
using Services.Interface;
using Shared;
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
using Error = Shared.Helper.Error;

namespace Services.Implementation
{
	public class AppUserManager
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ImageFileServices _imageFileServices;
		private readonly CommentService _commentService;
		private readonly AppsettingBinding _appsettings;
		private readonly IUserIdentityService _userIdentityService;
		private readonly IMapper _mapper;

		public AppUserManager(IUnitOfWork unitOfWork, ImageFileServices imageFileServices, CommentService commentService, AppsettingBinding appsettings, IMapper mapper, IUserIdentityService userIdentityService)
		{
			_unitOfWork = unitOfWork;
			_imageFileServices = imageFileServices;
			_commentService = commentService;
			_appsettings = appsettings;
			_mapper = mapper;
			_userIdentityService = userIdentityService;
		}
		public async Task<Result> IsUserLegit(UserProfile userProfile)
		{
			var error = new Error();
			if(userProfile.IdentityUser is null)
			{
				var getIdentity = await _userIdentityService.UserManager.FindByIdAsync(userProfile.IdentityId.ToString());
				userProfile.IdentityUser = getIdentity;
			}
			var identityLegitResult = await _userIdentityService.IsUserIdentityLegit(userProfile.IdentityUser);
			if (identityLegitResult.isSuccess is false)
			{
				return Result.Fail(identityLegitResult.Error);
			}
			if(userProfile.AccountStatus == AcccountStatus.BANNED 
				|| userProfile.AccountStatus == AcccountStatus.SUSPENDED)
			{
				error.ErrorMessage = "account is banned or suspended";
				return Result.Fail();
			}
			return Result.Success();
				
		}
		public async Task<Result<IList<CartItemDto>>>GetAllUserCartItems(int userprofileid)
		{
			var getUserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(userprofileid);
			if (getUserProfile is null)
				return Result<IList<CartItemDto>>.Fail();
			var getResult = await GetAllUserCartItems(getUserProfile);
			return Result<IList<CartItemDto>>.Success(getResult);
		}
		public async Task<IList<CartItemDto>> GetAllUserCartItems(UserProfile userProfile)
		{
			var getResult = (await _unitOfWork.Repositories.cartItemRepository
				.GetByCondition(c => c.UserId == userProfile.Id)).ToList();
			var mappedResult = _mapper.Map<IList<CartItemDto>>(getResult);
			foreach (var cartItem in mappedResult)
			{
				var trackId = cartItem.ItemId;
				if (cartItem.ItemType.Equals(CartItemType.TRACK))
				{
					var getTrack = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(trackId, "AudioFile,Tags,Licenses");
					if(getTrack is not null)
					{
						cartItem.Track = _mapper.Map<TrackResponseDto>(getTrack);
						MapCorrectTrackBannderUrl(cartItem.Track);
					}
				}
			}
			return mappedResult;
		}
		//public async Task<CartItemDto?> GetUserCartItem(UserProfile userProfile, int itemId)
		//{
		//	var getResult = (await _unitOfWork.Repositories.cartItemRepository
		//		.GetByCondition(item => item.UserId == userProfile.Id && item.Id == itemId)).FirstOrDefault();
		//	var mappedResult = _mapper.Map<CartItemDto>(getResult);
		//	return mappedResult;
		//}
		public async Task<Result> RemoveUserCartItem(int userProfileId, int itemId)
		{
			var error = new Error();
			var getuserprofile = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if (getuserprofile is null)
			{
				error.ErrorMessage = " not user profile found";
				return Result.Fail();
			}
			 await RemoveUserCartItem(getuserprofile, itemId);
			return Result.Success();
		}
		public async Task RemoveUserCartItem(UserProfile userProfile, int itemId)
		{
			var getResult = (await _unitOfWork.Repositories.cartItemRepository
				.GetByCondition(item => item.UserId == userProfile.Id && item.ItemId == itemId)).FirstOrDefault();
			//var getItem = await GetUserItem(userProfile, itemId);
			if (getResult is null)
				return;
			await _unitOfWork.Repositories.cartItemRepository.Delete(getResult);
			await _unitOfWork.SaveChangesAsync();

		}
		public async Task<Result<CartItemDto>> AddUserCartItem(int userProfileId, CartItemType typem, int itemId) 
		{
			var error = new Error();
			var getuserprofile = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if(getuserprofile is null)
			{
				error.ErrorMessage = " not user profile found";
				return Result<CartItemDto>.Fail(error);
			}
			var cartItemDto = await AddUserCartItem(getuserprofile, typem, itemId);
			if (cartItemDto is null)
			{
				error.ErrorMessage = "item already in cart";
				return Result<CartItemDto>.Fail(error);
			}
			return Result<CartItemDto>.Success(cartItemDto);
		}
		public async Task<CartItemDto?> AddUserCartItem(UserProfile userProfile, CartItemType type, int itemId)
		{
			var newItem = new CartItem()
			{
				UserId = userProfile.Id,
				ItemType = type,
				ItemId = itemId
			};
			var checkIfItemInCart = (await _unitOfWork.Repositories.cartItemRepository.GetByCondition(ci => ci.UserId == userProfile.Id && ci.ItemId == itemId)).FirstOrDefault();
			if (checkIfItemInCart is not null)// mean item exist
				return null;
			var result = await _unitOfWork.Repositories.cartItemRepository.Create(newItem);
			await _unitOfWork.SaveChangesAsync();
			var mappedResult = _mapper.Map<CartItemDto>(result);
			return mappedResult;
		}
		public async Task<UserProfileDto?> GetUserProfile(int id)
		{
			var getResult = await _unitOfWork.Repositories.userProfileRepository.GetById(id);
			var mappedResult = _mapper.Map<UserProfileDto>(getResult);
			MapCorrectProfileUrl(mappedResult);
			return mappedResult;
		}
		public async Task<UserProfileDto?> GetUserProfileByIdentity(int identityId)
		{
			var getResult = (await _unitOfWork.Repositories.userProfileRepository.GetByCondition(u => u.IdentityId == identityId)).FirstOrDefault();
			var mappedResult = _mapper.Map<UserProfileDto>(getResult);
			MapCorrectProfileUrl(mappedResult);
			return mappedResult;
		}
		public async Task<Result<UserProfileDto>> UpdateProfile(int userProfileId, UpdateUserProfileDto updateUserProfileDto)
		{
			var error = new Error();
			var getProfile = await GetUserProfileById(userProfileId);
			if(getProfile is null)
			{
				error.ErrorMessage = "cannot find profile";
				return Result<UserProfileDto>.Fail();
			}
			return await UpdateProfile(getProfile, updateUserProfileDto);
		}
		public async Task<Result<UserProfileDto>> UpdateProfile(UserProfile userProfile, UpdateUserProfileDto updateUserProfileDto)
		{
			if (updateUserProfileDto.Birthday is not null)
			{
				var DateUtc = DateTime.SpecifyKind(updateUserProfileDto.Birthday.Value, DateTimeKind.Utc);
				var DateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateUtc, TimeZoneInfo.Local);
				updateUserProfileDto.Birthday = DateLocal;
			}
			_mapper.Map(updateUserProfileDto, userProfile);
			await _unitOfWork.Repositories.userProfileRepository.Update(userProfile);
			await _unitOfWork.SaveChangesAsync();
			var mappedResult = _mapper.Map<UserProfileDto>(userProfile);
			MapCorrectProfileUrl(mappedResult);
			return Result<UserProfileDto>.Success(mappedResult);
		}
		public async Task<Result<string>> UpdateProfileImage(Stream filestream, string contentType, string fileName, int  userProfileId, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var getProfile = await GetUserProfileById(userProfileId);
			if (getProfile is null)
			{
				error.ErrorMessage = "cannot find profile";
				return Result.Fail();
			}
			return await UpdateProfileImage(filestream,contentType,fileName,getProfile,cancellationToken);
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
		public async Task<Result<IList<TrackCommentDto>>> GetUserTrackComments(int userProfileId)
		{
			var getProfile = await GetUserProfileById(userProfileId);
			if(getProfile is null)
			{
				var error = new Error();
				error.ErrorMessage = "cannot found profile";
				return Result<IList<TrackCommentDto>>.Fail();
			}
			return await GetUserTrackComments(getProfile);
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
		public async Task<Result<TrackCommentDto>> CreateComment(UserProfile userProfile, CreateTrackCommentDto createTrackCommentDto, int? replyToComment_Id = null)
		{
			var createResult = await _commentService.CreateTrackComment(userProfile, createTrackCommentDto);
			var mapResponse = _mapper.Map<TrackCommentDto>(createResult);
			return Result<TrackCommentDto>.Success(mapResponse);
		}
		public async Task<Result> RemoveComment(UserProfile userProfile, int userCommentId)
		{
			if (userCommentId == null)
				return Result.Fail();
			return await _commentService.RemoveComment(userProfile, userCommentId);
		}
		public async Task<Result<bool>> Subscribe(int userProfileId)
		{
			var error = new Error();
			var getUser = await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			if (getUser is null)
			{
				error.ErrorMessage = "fail to get user profile";
				return Result<bool>.Fail(error);
			}
			getUser.IsSubcribed = ! getUser.IsSubcribed;
			await _unitOfWork.Repositories.userProfileRepository.Update(getUser);
			await _unitOfWork.SaveChangesAsync();
			return Result<bool>.Success(getUser.IsSubcribed);
		}
		private async Task<UserProfile?> GetUserProfileById(int userProfileId)
		{
			return  await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
		}
		private void MapCorrectProfileUrl(UserProfileDto userProfileDto)
		{
			userProfileDto.ProfileBlobUrl =  string.IsNullOrEmpty(userProfileDto.ProfileBlobUrl) 
				? userProfileDto.ProfileBlobUrl = _appsettings.ExternalUrls.AzureBlobBaseUrl + "/public/"+ ApplicationStaticValue.DefaultProfileImageName
				: userProfileDto.ProfileBlobUrl = _appsettings.ExternalUrls.AzureBlobBaseUrl + "/public/" + userProfileDto.ProfileBlobUrl;
		}
		private void MapCorrectTrackBannderUrl(TrackResponseDto trackResponseDto) 
		{
			trackResponseDto.ProfileBlobUrl = string.IsNullOrEmpty(trackResponseDto.ProfileBlobUrl) 
				? trackResponseDto.ProfileBlobUrl = _appsettings.ExternalUrls.AzureBlobBaseUrl + "/public/" + ApplicationStaticValue.DefaultTrackImageName
				: trackResponseDto.ProfileBlobUrl = _appsettings.ExternalUrls.AzureBlobBaseUrl + "/public/" + trackResponseDto.ProfileBlobUrl;
		}
	}
}
