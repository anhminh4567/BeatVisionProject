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
		public async Task<TrackComment?> GetTrackCommentDetail(int commentId)
		{
			return (await _unitOfWork.Repositories.trackCommentRepository.GetByIdInclude(commentId, "ReplyToComment,Track"));
		}
		public async Task<IList<TrackComment>> GetTrackComments(Track track)
		{
			var getTrackComments = (await _unitOfWork.Repositories.trackCommentRepository
				.GetByCondition(t => t.TrackId == track.Id)).ToList();
			return getTrackComments;
		}

		public async Task<IList<TrackComment>> GetTrackCommentsTopLevel(Track track)
		{
			var getTrackComments = (await _unitOfWork.Repositories.trackCommentRepository
				.GetByCondition(t => t.TrackId == track.Id && t.ReplyToCommentId == null)).ToList();
			return getTrackComments;
		}
		public async Task<IList<TrackComment>> GetCommentReplys(TrackComment trackComment)
		{
			return (await _unitOfWork.Repositories.trackCommentRepository.GetByCondition(t => t.ReplyToCommentId == trackComment.Id)).ToList();
		}
		public async Task<TrackComment> CreateTrackComment(UserProfile authorProfile, CreateTrackCommentDto createTrackCommentDto)
		{
			var createDate = DateTime.Now;
			var newComment = new TrackComment();
			newComment.ReplyToCommentId = createTrackCommentDto.ReplyToCommentId;
			newComment.IsCommentRemoved = false;
			newComment.TrackId = createTrackCommentDto.TrackId;
			newComment.CreateDate = createDate;
			newComment.Content = createTrackCommentDto.Content;
			newComment.AuthorId = authorProfile.Id;
			newComment.LikesCount = 0;
			var createResult = await _unitOfWork.Repositories.trackCommentRepository.Create(newComment);
			await _unitOfWork.SaveChangesAsync();
			return createResult;
		}
		public async Task<Result> RemoveComment(UserProfile userProfile, int commentId)
		{
			var getComment = await _unitOfWork.Repositories.commentRepository.GetById(commentId);
			if (getComment == null)
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
		//public async Task<Result> LikeComment(int commentId)
		//{
		//	var error = new Error();
		//	var getComment = await _unitOfWork.Repositories.commentRepository.GetById(commentId);
		//	if (getComment == null)
		//	{
		//		error.ErrorMessage = "fail to get command";
		//		return Result.Fail();
		//	}
		//	getComment
		//}
	}
}
