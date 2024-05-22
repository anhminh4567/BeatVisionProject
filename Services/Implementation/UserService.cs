using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repository.Interface;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class UserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ImageFileServices _imageFileServices;
		private readonly AppsettingBinding _appsettings;
		private readonly IMapper _mapper;

		public UserService(IUnitOfWork unitOfWork, ImageFileServices imageFileServices, AppsettingBinding appsettings, IMapper mapper)
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

	}
}
