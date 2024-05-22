using AutoMapper;
using Repository.Interface;
using Shared.ConfigurationBinding;
using Shared.Helper;
using Shared.Models;
using Shared.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class CommentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ImageFileServices _imageFileServices;
		private readonly AppsettingBinding _appsettings;
		private readonly IMapper _mapper;

		public CommentService(IUnitOfWork unitOfWork, ImageFileServices imageFileServices, AppsettingBinding appsettings, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_imageFileServices = imageFileServices;
			_appsettings = appsettings;
			_mapper = mapper;
		}
		public async Task<IList<Comment>> GetAllUserComment(UserProfile userProfile)
		{
			return (await _unitOfWork.Repositories.commentRepository
				.GetByCondition(c => c.AuthorId == userProfile.Id)).ToList();
		}
		public async Task<IList<TrackComment>> GetAllUserTrackComment(UserProfile userProfile)
		{
			return (await _unitOfWork.Repositories.trackCommentRepository
				.GetByCondition(c => c.AuthorId == userProfile.Id)).ToList();
		}



		public async Task<Result> RemoveComment(UserProfile userProfile ,int commentId)
		{
			var getComment = await _unitOfWork.Repositories.commentRepository.GetById(commentId);
			if(getComment == null)
			{
				return Result.Fail();
			}
			getComment.IsCommentRemoved = true;
			getComment.Content = null;
			getComment.AuthorId = null;
			getComment.LikesCount = 0;
			await _unitOfWork.Repositories.commentRepository.Update(getComment);
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
	}
}
