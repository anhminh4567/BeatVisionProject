using AutoMapper;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Pkcs;
using Repository.Interface;
using Services.Interface;
using Shared;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.Poco;
using Shared.RequestDto;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public partial class TrackManager
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly AudioFileServices _audioFileService;
		private readonly ImageFileServices _imageFileService;
		private readonly IMyEmailService _mailServices;
		private readonly TagManager _tagService;
		private readonly AppsettingBinding _appSettings;
		private readonly CommentService _commentService;
		private readonly IMapper _mapper;
		private readonly LicenseFileService _licenseFileService;

		public TrackManager(IUnitOfWork unitOfWork, AudioFileServices audioFileService, ImageFileServices imageFileService, IMyEmailService mailServices, TagManager tagService, AppsettingBinding appSettings, CommentService commentService, IMapper mapper, LicenseFileService licenseFileService)
		{
			_unitOfWork = unitOfWork;
			_audioFileService = audioFileService;
			_imageFileService = imageFileService;
			_mailServices = mailServices;
			_tagService = tagService;
			_appSettings = appSettings;
			_commentService = commentService;
			_mapper = mapper;
			_licenseFileService = licenseFileService;
		}
		//create a track will always be private and not for sales, not until the publish is made will the track be decided to be public or not

		public async Task<Result> UploadTrack(CreateTrackDto createTrackDto, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var formFile = createTrackDto.uploadedFile;
			var getTags = (await _unitOfWork.Repositories.tagRepository.GetByCondition(tag => createTrackDto.TagsId.Contains(tag.Id))).ToList();
			string RANDOM_GENERATED_NAME = Guid.NewGuid().ToString();
			DateTime CREATE_DATETIME = DateTime.Now;
			BlobDirectoryType DIRECTORY_TYPE = createTrackDto.IsTrackPaidContent
					? Shared.Enums.BlobDirectoryType.PaidContent
					: Shared.Enums.BlobDirectoryType.Private;
			using Stream fileStream = formFile.OpenReadStream();
			using Stream mp3StreamFile = new MemoryStream();
			var checkResult = _audioFileService.IsWavFile(fileStream, formFile.FileName);
			if (checkResult.isSuccess is false)
			{
				return Result.Fail(checkResult.Error);
			}
			// ANALIZE audio and validate and generate required data
			// ANALIZE audio and validate and generate required data
			fileStream.Position = 0;
			var converResult = _audioFileService.ConvertWavToMp3(fileStream, mp3StreamFile);
			fileStream.Position = 0;
			var analyseWavFile = _audioFileService.AnalizeWavAudioFile(fileStream, FileHelper.ExtractFileExtention(formFile.FileName).Value).Value;
			fileStream.Position = 0;
			mp3StreamFile.Position = 0;
			var analyseMp3File = _audioFileService.AnalizeMp3AudioFile(mp3StreamFile, "mp3").Value;
			mp3StreamFile.Position = 0;
			var mp3Filename = _audioFileService.ConvertFilename_To_Mp3(formFile.FileName).Value;
			// ANALIZE audio and validate and generate required data
			// ANALIZE audio and validate and generate required data
			var newTrack = new Track()
			{
				TrackName = createTrackDto.TrackName,
				AudioChannels = analyseWavFile.Channels,
				AudioBitPerSample = analyseWavFile.BitPerSample,
				AudioBpm = 0,
				AudioSampleRate = analyseWavFile.SampleRate,
				AudioLenghtSeconds = analyseWavFile.SecondLenght,
				IsAudioPrivate = true,
				IsAudioForSale = false,
				IsAudioRemoved = false,
				IsPublished = false,
				PlayCount = 0,
				//OwnerId = userProfile.Id,
				PublishDateTime = null,
				Tags = getTags,
				Status = Shared.Enums.TrackStatus.NOT_FOR_PUBLISH,
				//AudioBlobId = null,
				//ProfileBlobUrl = createTrackDto.ProfileBlobUrl,
			};
			var newWavFile = new BlobFileData()
			{
				ContentType = formFile.ContentType,
				DirectoryType = DIRECTORY_TYPE,
				IsPublicAccess = false,
				SizeMb = analyseWavFile.SizeMb,
				FileExtension = FileHelper.ExtractFileExtention(formFile.FileName).Value,
				GeneratedName = RANDOM_GENERATED_NAME,
				OriginalFileName = formFile.FileName,
				IsPaidContent = createTrackDto.IsTrackPaidContent,
				PathUrl = null,
				UploadedDate = CREATE_DATETIME,
			};
			var newMp3File = new BlobFileData()
			{
				ContentType = ApplicationStaticValue.ContentTypeMp3,
				DirectoryType = DIRECTORY_TYPE,
				IsPublicAccess = false,
				SizeMb = analyseMp3File.SizeMb,
				FileExtension = "mp3",
				GeneratedName = RANDOM_GENERATED_NAME,
				OriginalFileName = formFile.FileName,
				IsPaidContent = createTrackDto.IsTrackPaidContent,
				PathUrl = null,
				UploadedDate = CREATE_DATETIME,
			};
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				var uploadWAVResult = await _audioFileService.UploadWavAudioFile(fileStream, ApplicationStaticValue.ContentTypeWav, formFile.FileName, RANDOM_GENERATED_NAME, createTrackDto.IsTrackPaidContent, cancellationToken);
				if (uploadWAVResult.isSuccess is false)
				{
					error = uploadWAVResult.Error;
					return Result.Fail(error);
				}
				var uploadMP3Result = await _audioFileService.UploadMp3AudioFile(mp3StreamFile, ApplicationStaticValue.ContentTypeMp3, formFile.FileName, RANDOM_GENERATED_NAME, false, createTrackDto.IsTrackPaidContent, cancellationToken);
				if (uploadMP3Result.isSuccess is false)
				{
					await _audioFileService.DeleteAudioFile_Any(uploadWAVResult.Value, DIRECTORY_TYPE, cancellationToken);
					error = uploadMP3Result.Error;
					return Result.Fail(error);
				}
				newWavFile.PathUrl = uploadWAVResult.Value;
				newMp3File.PathUrl = uploadMP3Result.Value;
				await _unitOfWork.Repositories.blobFileDataRepository.Create(newWavFile);
				await _unitOfWork.Repositories.blobFileDataRepository.Create(newMp3File);
				await _unitOfWork.SaveChangesAsync();
				newTrack.AudioBlobId = newWavFile.Id;
				await _unitOfWork.Repositories.trackRepository.Create(newTrack);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitAsync();
				// Now check if banner is a url or a new image,
				if (string.IsNullOrEmpty(createTrackDto.ProfileBlobUrl) is false)
				{
					var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(newTrack.Id);
					getTrack.ProfileBlobUrl = createTrackDto.ProfileBlobUrl;
					await _unitOfWork.Repositories.trackRepository.Update(getTrack);
					await _unitOfWork.SaveChangesAsync();
				}
				else if (createTrackDto.bannderFile is not null)
				{
					var imageFile = createTrackDto.bannderFile;
					using Stream imageStream = imageFile.OpenReadStream();
					var uploadProfile = await _imageFileService.UploadNewImage(imageStream, imageFile.FileName, RANDOM_GENERATED_NAME, imageFile.ContentType, cancellationToken);
					var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(newTrack.Id);
					getTrack.ProfileBlobUrl = uploadProfile.Value;
					await _unitOfWork.Repositories.trackRepository.Update(getTrack);
					await _unitOfWork.SaveChangesAsync();
				}
				return Result.Success();

			}
			catch (Exception ex)
			{
				await _unitOfWork.RollBackAsync();
				error.isException = true;
				error.ErrorMessage = ex.Message;
				error.Exception = ex;
				return Result.Fail(error);
			}

		}

		public async Task<Result> SetPublishTrack(PublishTrackDto publishTrackDto)
		{
			var error = new Error();
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(publishTrackDto.TrackId);
			if (getTrack is null)
			{
				return Result.Fail();
			}
			if(getTrack.IsPublished) 
			{
				error.ErrorMessage = "track already published, cannot republish";
				return Result.Fail();
			}
			DateTime PUBLISH_DATETIME = DateTime.Now;
			if (publishTrackDto.IsPublishNow)
			{
				PUBLISH_DATETIME.AddMinutes(2);
				getTrack.PublishDateTime = PUBLISH_DATETIME;
			}
			else
			{
				if (DateTime.Compare(publishTrackDto.PublishDate, PUBLISH_DATETIME) <= 0)
				{
					error.ErrorMessage = "date is not appropriate, please make it later than now, about 1-2 minute";
					return Result.Fail();
				}
				PUBLISH_DATETIME = publishTrackDto.PublishDate;
				getTrack.PublishDateTime = PUBLISH_DATETIME;
			}
			if (publishTrackDto.IsTrackPaid)
			{
				if(publishTrackDto.Price is null)
				{
					return Result.Fail();
				}
				getTrack.IsAudioForSale = true;
				getTrack.Price = publishTrackDto.Price.Value;
			}
			getTrack.Status = TrackStatus.WAIT_FOR_PUBLISH;
			await _unitOfWork.Repositories.trackRepository.Update(getTrack);
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
		//for background service
		public async Task PublishTrack()
		{
			var getTrackTobePublish = (await _unitOfWork.Repositories.trackRepository
				.GetByCondition(t => t.IsPublished == false && t.Status == TrackStatus.WAIT_FOR_PUBLISH, null, "AudioFile")).ToList();
			foreach (var track in getTrackTobePublish)
			{
				if (track.PublishDateTime is null)
				{
					//do something, publish event c
					continue;
				}
				if (DateTime.Compare(track.PublishDateTime.Value, DateTime.Now) <= 0)
				{
					if (track.AudioFile is null)
					{
						//do something, publish event c
						continue;
					}
					var getFileFromPrivateDirectory = await _audioFileService.DownloadPrivateMp3Audio(track.Id);
					var getRandomFileName = track.AudioFile.GeneratedName;
					var getOriginalFileName = track.AudioFile.OriginalFileName;
					if (getFileFromPrivateDirectory.isSuccess is false)
					{
						//do something, publish event c
						continue;
					}
					using var downloadedFileStream = getFileFromPrivateDirectory.Value.Stream;
					// when publish, we just move an audio file from private to public, for user to get the file and listen to it, so the isPiadContent is false, and public is true, like below
					var uploadResult = await _audioFileService.UploadMp3AudioFile(downloadedFileStream, ApplicationStaticValue.ContentTypeMp3, getOriginalFileName, getRandomFileName, true, false);
					if(uploadResult.isSuccess is false)
					{
						continue;
					}
					var getPath = uploadResult.Value;
					var newBlobFileMp3 = new BlobFileData()
					{
						SizeMb = null,
						ContentType = getFileFromPrivateDirectory.Value.ContentType,
						DirectoryType = BlobDirectoryType.Public,
						FileExtension = "mp3",
						GeneratedName = getRandomFileName,
						IsPaidContent = false,
						IsPublicAccess = true,
						PathUrl = getPath,
						OriginalFileName = getOriginalFileName,
						UploadedDate = DateTime.Now,
					};
					await _unitOfWork.Repositories.blobFileDataRepository.Create(newBlobFileMp3);
					track.Status = TrackStatus.PUBLISH;
					track.IsPublished = true;
					await _unitOfWork.Repositories.trackRepository.Update(track);
					await _unitOfWork.SaveChangesAsync();
				}
			}
			await _unitOfWork.SaveChangesAsync();

		}
		// remove the file from public for access
		public async Task<Result> PulldownTrack(int trackId)
		{
			var error = new Error();
			var track = await _unitOfWork.Repositories.trackRepository.GetById(trackId);
			if (track is null)
			{
				error.ErrorMessage = "canot find track";
				return Result.Fail();
			}
			if (track.IsPublished is false)
			{
				error.ErrorMessage = "the track is not publish ";
				return Result.Fail();
			}
			var getWAVBlobFile = await _unitOfWork.Repositories.blobFileDataRepository
				.GetById(track.AudioBlobId);
			//track.Status = TrackStatus.REMOVED;
			//track.IsAudioRemoved = true;
			track.Status = TrackStatus.WAIT_FOR_PUBLISH;
			track.IsPublished = false;
			var WAVfileName = getWAVBlobFile.GeneratedName;
			var getAllRelatedFile = await _unitOfWork.Repositories.blobFileDataRepository
				.GetByCondition(f => f.GeneratedName == WAVfileName);
			var getPublicFile = getAllRelatedFile
				.FirstOrDefault(blob => blob.DirectoryType == BlobDirectoryType.Public && blob.IsPublicAccess ==true);
			var getRelativePath = getPublicFile.PathUrl;
			var getBlobType = BlobDirectoryType.Public;
			var deletePublishedAudioResult = await _audioFileService.DeleteAudioFile_Any(getRelativePath, getBlobType);
			if (deletePublishedAudioResult.isSuccess is false)
			{
				error.ErrorMessage = "fail to remove, try again, might be due to azure blob error";
				return Result.Fail();
			}
			track.IsPublished = false;
			track.Status = TrackStatus.NOT_FOR_PUBLISH;
			await _unitOfWork.Repositories.trackRepository.Update(track);
			await _unitOfWork.Repositories.blobFileDataRepository.Delete(getPublicFile);
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
		public async Task<Result> UpdatePublish(UpdatePublishtrackDto updatePublishtrackDto)
		{
			var error = new Error();
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(updatePublishtrackDto.TrackId);
			if (updatePublishtrackDto.IsRemovePublish)
			{
				var pullResult = await PulldownTrack(getTrack.Id);
				//if(pullResult.isSuccess is false)
				//{
				//	return Result.Fail(pullResult.Error);
				//} 
				getTrack.Status = TrackStatus.NOT_FOR_PUBLISH;
				await _unitOfWork.Repositories.trackRepository.Update(getTrack);
				await _unitOfWork.SaveChangesAsync();
			}
			else
			{
				if (updatePublishtrackDto.IsChangePublishDate)
				{
					var isPublishDayOk = ValidatePublishDay(updatePublishtrackDto.PublishDate);
				}
			}
			return Result.Fail();
		}
		public async Task<Result> SendMusicAttachmentToEmail(UserProfile user, IList<Track> tracksToSend)
		{
			IList<EmailAttachments> emailAttachments = new List<EmailAttachments>();
			foreach (var track in tracksToSend)
			{
				var mp3Download = await _audioFileService.DownloadPrivateMp3Audio(track.Id);
				emailAttachments.Add(new EmailAttachments()
				{
					ContentType = mp3Download.Value.ContentType,
					FileStream = mp3Download.Value.Stream,
					FileName = "mp3"
				}); ;
				var wavDownload = await _audioFileService.DownloadPrivateWavAudio(track.Id);
				emailAttachments.Add(new EmailAttachments()
				{
					ContentType = wavDownload.Value.ContentType,
					FileStream = wavDownload.Value.Stream,
					FileName = "wav"
				});
			}
			var getUserIdentity = await _unitOfWork.Repositories.customIdentityUser.GetById(user.IdentityId);
			var emailMetaData = new EmailMetaData()
			{
				Bccs = null,
				Ccs = null,
				Subject = "Track buy",
				ToEmail = getUserIdentity.Email,
				Attachments = emailAttachments,
			};
			var emailTemplatePath = _appSettings.MailTemplateRelativePath.FirstOrDefault(p => p.TemplateName == "DownloadTrackEmail")?.TemplatePathWWWRoot;
			if (string.IsNullOrEmpty(emailTemplatePath))
			{
				return Result.Fail();
			}
			var emailModel = new DownloadTrackEmailModel()
			{
				LicenseName = "asdf",
				ReceiverEmail = getUserIdentity.Email,
				Timesend = DateTime.Now,
				TrackName = "test",
				Username = user.Fullname,
			};
			await _mailServices.SendEmailWithTemplate_WithAttachment(emailMetaData,emailTemplatePath,emailModel);
			return Result.Success();
		}
		public bool ValidatePublishDay(DateTime date)
		{
			if (DateTime.Compare(date, DateTime.Now) <= 0)
			{
				return false;
			}
			if (DateTime.Compare(date.AddMinutes(2), DateTime.Now) <= 0)
			{
				return false;
			}
			return true;
		}
		public async Task<IList<TrackResponseDto>> GetTracks()
		{
			var tracks = await _unitOfWork.Repositories.trackRepository.GetByCondition(includeProperties: "Tags");
			var mappedList = _mapper.Map<IList<TrackResponseDto>>(tracks);
			return mappedList;
		}
		public async Task<Result<BlobFileResponseDto>> GetTrackMp3Public(int trackId)
		{
			return await _audioFileService.DownloadPublicMp3Audio(trackId);
		}
		
		public async Task<IList<TrackResponseDto>> GetTracksRange(int start, int amount) 
		{
			var tracks = await _unitOfWork.Repositories.trackRepository.GetByCondition(includeProperties: "Tags", skip: start, take: amount);
			var mappedList = _mapper.Map<IList<TrackResponseDto>>(tracks);
			foreach(var track in mappedList)
			{
				MapCorrectImageUrl(track);
			}
			return mappedList;
		}

		public async Task<TrackResponseDto> GetTrackDetail(int trackId)
		{
			//Track
			var getTrackDetail = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(trackId, "Tags,Comments,Licenses,AudioFile");
			var correctResult = _mapper.Map<TrackResponseDto>(getTrackDetail);
			 MapCorrectImageUrl(correctResult);
			return correctResult;
		}
		public async Task<IList<TrackResponseDto>> GetTracksWithTags(params int[] tagId)
		{
			if(tagId.Length == 0)
				return new List<TrackResponseDto>();
			 var getTags = await _unitOfWork.Repositories.tagRepository.GetByCondition(t => tagId.Contains(t.Id));
			ISet<Track> uniqueItemList = new HashSet<Track>();
			foreach( var t in getTags)
			{
				var getTagRelatedTracks = (await _unitOfWork.Repositories.tagRepository.GetByIdInclude(t.Id, "Tracks")).Tracks;
				uniqueItemList.UnionWith(getTagRelatedTracks);
			}
			IList<TrackResponseDto> returnList = _mapper.Map<IList<TrackResponseDto>>(uniqueItemList.ToList());
			foreach(var t in returnList)
			{
				MapCorrectImageUrl(t);
			}
			return returnList;
		}
		public async Task<Result> DeleteTrack(int trackId)
		{
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(trackId, "AudioFile");
			var getBlobAudio = getTrack.AudioFile;
			var getGeneratedName = getBlobAudio.GeneratedName;
			var getAllBlobAudioFile = await _unitOfWork.Repositories.blobFileDataRepository.GetByCondition(f => f.GeneratedName == getGeneratedName);
			foreach (var blobAudio in getAllBlobAudioFile)
			{
				 _audioFileService.DeleteAudioFile_Any(blobAudio.PathUrl, blobAudio.DirectoryType);
			}
			await _unitOfWork.Repositories.blobFileDataRepository.DeleteRange(getAllBlobAudioFile);
			await _unitOfWork.Repositories.trackRepository.Delete(getTrack);
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}

		public async Task<IList<TrackCommentDto>> GetTrackComments(int trackId)
		{
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(trackId);
			if (getTrack is null)
				return new List<TrackCommentDto>();
			var getResult = await _commentService.GetTrackComments(getTrack);
			return _mapper.Map<IList<TrackCommentDto>>(getResult);
		}
		public async Task<IList<TrackCommentDto>> GetTrackCommentReplies(int trackId,int commentId)
		{
			var getTrackComment = (await _unitOfWork.Repositories.trackCommentRepository.GetByCondition(c => c.TrackId == trackId && c.Id == commentId)).FirstOrDefault();
			if (getTrackComment is null)
				return new List<TrackCommentDto>();
			var getComments = _commentService.GetCommentReplys(getTrackComment);
			return _mapper.Map<IList<TrackCommentDto>>(getComments);
		}

	}
}
namespace Services.Implementation
{
	public partial class TrackManager {
		public async Task<IList<TrackLicenseDto>> GetTrackLicensePaging(int start, int amount)
		{
			var getReuslt = await _unitOfWork.Repositories.trackLicenseRepository.GetRange(start, amount);
			return _mapper.Map<IList<TrackLicenseDto>>(getReuslt);
		}
		public int GetTotalTrackCount()
		{
			return _unitOfWork.Repositories.trackRepository.COUNT;
		}
		public async Task<TrackLicenseDto> GetTrackLicense(int licenseId)
		{
			var getReuslt = await _unitOfWork.Repositories.trackLicenseRepository.GetById(licenseId);
			return _mapper.Map<TrackLicenseDto>(getReuslt);
		}
		public async Task<Result<BlobFileResponseDto>> DownloadTrackLicense(int licenseId)
		{
			var error = new Error();
			var getReuslt = await _unitOfWork.Repositories.trackLicenseRepository.GetById(licenseId);
			if (getReuslt is null)
			{
				error.ErrorMessage = "cant find license";
				return Result<BlobFileResponseDto>.Fail(error);
			}
			return await _licenseFileService.DownloadLicensePdf(getReuslt);

		}

		public async Task<Result<TrackLicenseDto>> AddTrackLicense(CreateTrackLicenseDto createTrackLicenseDto, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var newLicense = new TrackLicense();
			newLicense.StreamLimit = createTrackLicenseDto.StreamLimit is null ? -1 : createTrackLicenseDto.StreamLimit.Value;
			newLicense.DistributionLimit = createTrackLicenseDto.DistributionLimit is null ? -1 : createTrackLicenseDto.DistributionLimit.Value;
			newLicense.DefaultPrice = 0;
			newLicense.CurrentPrice = 0;
			newLicense.IsMP3Supported = true;
			newLicense.IsWAVSupported = true;
			newLicense.LicenceName = createTrackLicenseDto.LicenceName;
			var uploadResult = await _licenseFileService.UploadLicenseFile(fileStream, fileName, contentType, cancellationToken);
			if (uploadResult.isSuccess is false)
			{
				return Result<TrackLicenseDto>.Fail(uploadResult.Error);
			}
			var getRelativePath = uploadResult.Value;
			newLicense.LicensePdfBlobPath = getRelativePath;
			var createResult = await _unitOfWork.Repositories.trackLicenseRepository.Create(newLicense);
			await _unitOfWork.SaveChangesAsync();
			if (createResult is null)
			{
				error.ErrorMessage = "cant create now, error server";
				return Result<TrackLicenseDto>.Fail(error);
			}
			var mappedResult = _mapper.Map<TrackLicenseDto>(createResult);
			return Result<TrackLicenseDto>.Success(mappedResult);
		}
		public async Task<Result> RemoveTrackLicense(int licenseId)
		{
			var getTrackLicense = await _unitOfWork.Repositories.trackLicenseRepository.GetById(licenseId);
			if (getTrackLicense is null)
			{
				return Result.Fail();
			}
			await _unitOfWork.Repositories.trackLicenseRepository.Delete(getTrackLicense);
			await _unitOfWork.SaveChangesAsync();
			var removeFileResult = await _licenseFileService.DeleteLicenseFile(getTrackLicense.LicensePdfBlobPath);
			//if(removeFileResult.isSuccess is false)
			//{
			//	return Result.Fail();
			//}
			return Result.Success();
		}
		public async Task<Result> AddTrackToLicense(int trackId, int licenseId)
		{
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(trackId);
			var getLicense = await _unitOfWork.Repositories.trackLicenseRepository.GetById(licenseId);
			if (getTrack is null || getLicense is null)
			{
				return Result.Fail();
			}
			getTrack.Licenses.Add(getLicense);
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
		public async Task<Result> RemoveTrackFromLicense(int trackId, int licenseId)
		{
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(trackId);
			var getLicense = await _unitOfWork.Repositories.trackLicenseRepository.GetById(licenseId);
			if (getTrack is null || getLicense is null)
			{
				return Result.Fail();
			}
			var removeResult = getTrack.Licenses.Remove(getLicense);
			if (removeResult is false)
			{
				return Result.Fail();
			}
			await _unitOfWork.SaveChangesAsync();
			return Result.Success();
		}
		private void MapCorrectImageUrl(TrackResponseDto track)
		{
			track.ProfileBlobUrl = string.IsNullOrEmpty(track.ProfileBlobUrl)
					? _appSettings.ExternalUrls.AzureBlobBaseUrl + "/public/" + ApplicationStaticValue.DefaultTrackImageName
					: _appSettings.ExternalUrls.AzureBlobBaseUrl + "/public/" + track.ProfileBlobUrl;
		}
		
	}

}
