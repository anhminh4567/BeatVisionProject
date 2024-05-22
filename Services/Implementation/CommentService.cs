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
		public async Task<IList<AlbumComment>> GetAllUserAlbumnComment(UserProfile userProfile)
		{
			return (await _unitOfWork.Repositories.albumCommentRepository
				.GetByCondition(c => c.AuthorId == userProfile.Id)).ToList();
		}
		public async Task<TrackComment?> GetTrackCommentDetail(int commentId)
		{
			return (await _unitOfWork.Repositories.trackCommentRepository
				.GetByIdInclude(commentId, "Track"));
		}
		public async Task<AlbumComment?> GetAlbumCommentDetail(int albumnCommentId)
		{
			return await _unitOfWork.Repositories.albumCommentRepository
				.GetByIdInclude(albumnCommentId, "Album");
		}
		public async Task<TrackComment> CreateTrackCommment(UserProfile userProfile, CreateTrackCommentDto createTrackCommentDto)
		{
			var createResult = await _unitOfWork.Repositories.trackCommentRepository.Create(new TrackComment()
			{
				AuthorId = userProfile.Id,
				Content = createTrackCommentDto.Content,
				ReplyToCommentId = createTrackCommentDto.ReplyToCommentId,
				TrackId = createTrackCommentDto.TrackId,
			});
			await _unitOfWork.SaveChangesAsync();
			return createResult;
		}
		public async Task<AlbumComment> CreateAlbumnComment(UserProfile userProfile, CreateAlbumnCommentDto createTrackCommentDto)
		{
			var createResult = await _unitOfWork.Repositories.albumCommentRepository.Create(new AlbumComment()
			{
				AuthorId = userProfile.Id,
				Content = createTrackCommentDto.Content,
				ReplyToCommentId = createTrackCommentDto.ReplyToCommentId,
				AlbumId = createTrackCommentDto.AlbumId,
			});
			await _unitOfWork.SaveChangesAsync();
			return createResult;
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
