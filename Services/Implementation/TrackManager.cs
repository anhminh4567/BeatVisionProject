using AutoMapper;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Pkcs;
using Repository.Implementation;
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
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
		private readonly NotificationManager _notificationManager;
		public TrackManager(IUnitOfWork unitOfWork, AudioFileServices audioFileService, ImageFileServices imageFileService, IMyEmailService mailServices, TagManager tagService, AppsettingBinding appSettings, CommentService commentService, IMapper mapper, LicenseFileService licenseFileService, NotificationManager notificationManager)
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
			_notificationManager = notificationManager;
		}
		//create a track will always be private and not for sales, not until the publish is made will the track be decided to be public or not

		public async Task<Result> UploadTrack(CreateTrackDto createTrackDto, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var formFile = createTrackDto.uploadedFile;
			var getTags = (await _unitOfWork.Repositories.tagRepository.GetByCondition(tag => createTrackDto.TagsId.Contains(tag.Id))).ToList();
			var getLicense = (await _unitOfWork.Repositories.trackLicenseRepository.GetByCondition(license => createTrackDto.LicenseIds.Contains(license.Id))).ToList();
			if (getTags.Count == 0 || getLicense.Count == 0)
			{
				error.ErrorMessage = "no tag or license found, add those 2 first";
				return Result.Fail();
			}
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
				Licenses = getLicense,
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
			var publishDateUtc = DateTime.SpecifyKind(publishTrackDto.PublishDate, DateTimeKind.Utc);
			var publishDateLocal = TimeZoneInfo.ConvertTimeFromUtc(publishDateUtc, TimeZoneInfo.Local);
			publishTrackDto.PublishDate = publishDateLocal;
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(publishTrackDto.TrackId);
			if (getTrack is null)
			{
				return Result.Fail();
			}
			if (getTrack.IsPublished)
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
				if (publishTrackDto.Price is null)
				{
					return Result.Fail();
				}
				getTrack.IsAudioForSale = true;
				getTrack.Price = publishTrackDto.Price.Value;
			}
			getTrack.Status = TrackStatus.WAIT_FOR_PUBLISH;
			await _unitOfWork.Repositories.trackRepository.Update(getTrack);
			await _unitOfWork.SaveChangesAsync();
			PublishTrack();
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
					if (uploadResult.isSuccess is false)
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
					_notificationManager.ServerSendNotificationMail(new CreateNotificationForNewTracks()
					{
						Content = "new track publish : " + track.TrackName,
						MessageName = "New Track Publish !",
						TrackId = track.Id,
						Weight = NotificationWeight.MINOR,
					});
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
				//if track is just set for publish, wait for it, then ofcourse you still can pull it down
				if (track.Status != TrackStatus.WAIT_FOR_PUBLISH)
				{
					error.ErrorMessage = "the track is not publish ";
					return Result.Fail();
				}
			}
			var getWAVBlobFile = await _unitOfWork.Repositories.blobFileDataRepository
				.GetById(track.AudioBlobId);
			//track.Status = TrackStatus.REMOVED;
			//track.IsAudioRemoved = true;
			//track.Status = TrackStatus.WAIT_FOR_PUBLISH;
			//track.IsPublished = false;
			var WAVfileName = getWAVBlobFile.GeneratedName;
			var getAllRelatedFile = await _unitOfWork.Repositories.blobFileDataRepository
				.GetByCondition(f => f.GeneratedName == WAVfileName);
			var getPublicFile = getAllRelatedFile
				.FirstOrDefault(blob => blob.DirectoryType == BlobDirectoryType.Public && blob.IsPublicAccess == true);
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
					var publishDateUtc = DateTime.SpecifyKind(updatePublishtrackDto.PublishDate, DateTimeKind.Utc);
					var publishDateLocal = TimeZoneInfo.ConvertTimeFromUtc(publishDateUtc, TimeZoneInfo.Local);
					var isPublishDayOk = ValidatePublishDay(publishDateLocal);
					if (isPublishDayOk)
					{

					}
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
			var emailTemplatePath = _appSettings.MailTemplateAbsolutePath.FirstOrDefault(p => p.TemplateName == "DownloadTrackEmail")?.TemplateAbsolutePath;
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
			await _mailServices.SendEmailWithTemplate_WithAttachment(emailMetaData, emailTemplatePath, emailModel);
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
		public async Task<string?> GetAbsoluteMp3PublicPath(Track track)
		{
			var getWavFile = await _unitOfWork.Repositories.blobFileDataRepository.GetById(track.AudioBlobId);
			if (getWavFile == null)
				return null;
			var tryGetMp3Path = (await _unitOfWork.Repositories.blobFileDataRepository
				.GetByCondition(t => t.DirectoryType == BlobDirectoryType.Public &&
				t.IsPublicAccess == true &&
				t.GeneratedName == getWavFile.GeneratedName)).FirstOrDefault();
			if (tryGetMp3Path == null)
				return null;
			return _appSettings.ExternalUrls.AzureBlobBaseUrl + "/public/" + tryGetMp3Path.PathUrl;
		}



		public async Task<IList<TrackResponseDto>> GetTracksRange(int start, int amount)
		{
			var tracks = await _unitOfWork.Repositories.trackRepository.GetByCondition(includeProperties: "Tags,Licenses", skip: start, take: amount);
			var mappedList = _mapper.Map<IList<TrackResponseDto>>(tracks);
			foreach (var track in mappedList)
			{
				MapCorrectImageUrl(track);
			}
			return mappedList;
		}
		public async Task<IList<TrackResponseDto>> GetTrackRange_Status(int start, int amount, TrackStatus STATUS)
		{
			//var tracks = await _unitOfWork.Repositories.trackRepository
			//	.GetByCondition(t => t.Status.ToString() == STATUS, null, includeProperties: "Tags", skip: start, take: amount);
			var tracks = await _unitOfWork.Repositories.trackRepository
				.GetByCondition(t => t.Status.Equals(STATUS), null, includeProperties: "Tags", skip: start, take: amount);
			var mappedList = _mapper.Map<IList<TrackResponseDto>>(tracks);
			foreach (var track in mappedList)
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
			var getMp3AbsolutePath = await GetAbsoluteMp3PublicPath(getTrackDetail);
			correctResult.Mp3AbsolutePath = getMp3AbsolutePath;
			return correctResult;
		}
		public async Task<IList<TrackResponseDto>> GetTracksWithTags(params int[] tagId)
		{
			if (tagId.Length == 0)
				return new List<TrackResponseDto>();
			var getTags = await _unitOfWork.Repositories.tagRepository.GetByCondition(t => tagId.Contains(t.Id));
			ISet<Track> uniqueItemList = new HashSet<Track>();
			foreach (var t in getTags)
			{
				var getTagRelatedTracks = (await _unitOfWork.Repositories.tagRepository.GetByIdInclude(t.Id, "Tracks")).Tracks;
				uniqueItemList.UnionWith(getTagRelatedTracks);
			}
			IList<TrackResponseDto> returnList = _mapper.Map<IList<TrackResponseDto>>(uniqueItemList.ToList());
			foreach (var t in returnList)
			{
				MapCorrectImageUrl(t);
			}
			return returnList;
		}

		public async Task<Result> UpdateTrack(UpdateTrackDto updateTrackDto)
		{
			var error = new Error();
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(updateTrackDto.TrackId, "AudioFile,Tags,Licenses");
			if (getTrack is null)
			{
				error.ErrorMessage = "track not found";
				return Result.Fail(error);
			}
			if (getTrack.Status != TrackStatus.NOT_FOR_PUBLISH)
			{
				error.ErrorMessage = "Track must not be for publish to be updated, cannot update when it is published or wait for published, might be confusing";
				return Result.Fail(error);
			}
			var getTags = await _unitOfWork.Repositories.tagRepository.GetByCondition(tag => updateTrackDto.TagsId.Contains(tag.Id));
			var getLicenses = await _unitOfWork.Repositories.trackLicenseRepository.GetByCondition(license => updateTrackDto.LicenseIds.Contains(license.Id));
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				getTrack.Tags.Clear();
				getTrack.Licenses.Clear();
				await _unitOfWork.Repositories.trackRepository.Update(getTrack);
				await _unitOfWork.SaveChangesAsync();

				getTrack.Tags = getTags.ToList();
				getTrack.Licenses = getLicenses.ToList();

				getTrack.TrackName = updateTrackDto.TrackName;
				if (updateTrackDto.bannderFile is not null)
				{
					var RANDOM_GENERATED_NAME = Guid.NewGuid().ToString();
					var imageFile = updateTrackDto.bannderFile;
					using Stream imageStream = imageFile.OpenReadStream();
					var uploadProfile = await _imageFileService.UploadNewImage(imageStream, imageFile.FileName, RANDOM_GENERATED_NAME, imageFile.ContentType);
					getTrack.ProfileBlobUrl = uploadProfile.Value;
				}
				var updateResult = await _unitOfWork.Repositories.trackRepository.Update(getTrack);
				await _unitOfWork.SaveChangesAsync();
				if (updateResult is null)
				{
					await _unitOfWork.RollBackAsync();
					error.ErrorMessage = "fail to update";
					Result.Fail(error);
				}
				await _unitOfWork.CommitAsync();
				return Result.Success();
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollBackAsync();
				error.isException = true;
				error.StatusCode = StatusCodes.Status500InternalServerError;
				error.ErrorMessage = ex.Message;
				return Result.Fail(error);
			}

		}
		public async Task<Result> DeleteTrack(int trackId)
		{
			var error = new Error();
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(trackId, "AudioFile");
			var getBlobAudio = getTrack?.AudioFile;
			if (getTrack is null || getBlobAudio is null)
			{
				error.ErrorMessage = "track not found";
				return Result.Fail();
			}
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
			var getTrack =  await _unitOfWork.Repositories.trackRepository.GetById(trackId);
			if (getTrack is null)
				return new List<TrackCommentDto>();
			var getResult = await _commentService.GetTrackCommentsTopLevel(getTrack);
			return _mapper.Map<IList<TrackCommentDto>>(getResult);
		}
		public async Task<IList<TrackCommentDto>> GetTrackCommentReplies(int trackId, int commentId)
		{
			var getTrackComment = (await _unitOfWork.Repositories.trackCommentRepository.GetByCondition(c => c.TrackId == trackId && c.Id == commentId)).FirstOrDefault();
			if (getTrackComment is null)
				return new List<TrackCommentDto>();
			var getComments = await _commentService.GetCommentReplys(getTrackComment);
			return _mapper.Map<IList<TrackCommentDto>>(getComments);
		}

	}
}
namespace Services.Implementation
{
	public partial class TrackManager
	{
		public int GetTrackLicenseCount()
		{
			return _unitOfWork.Repositories.trackLicenseRepository.COUNT;
		}
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
		public async Task<Result<BlobFileResponseDto>> DownloadTrackItems(int trackId)
		{
			var error = new Error();
			var getTrackDetails = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(trackId, "Licenses");
			if (getTrackDetails is null)
			{
				error.ErrorMessage = "fail to get track detail";
				return Result<BlobFileResponseDto>.Fail(error);
			}
			if (ValidateIfTrackIsForDownload(getTrackDetails) is false)
			{
				error.ErrorMessage = "track is not valid";
				return Result<BlobFileResponseDto>.Fail(error);
			}
			var getTrackLicenses = getTrackDetails.Licenses;
			var trackBlobFile = await _unitOfWork.Repositories.blobFileDataRepository.GetById(getTrackDetails.AudioBlobId);
			var getAudioGeneratedName = trackBlobFile.GeneratedName;
			var getAllAudioFile = (await _unitOfWork.Repositories.blobFileDataRepository
				.GetByCondition(file => file.GeneratedName == getAudioGeneratedName && file.IsPublicAccess == false)).ToList();
			var memoryStream = new MemoryStream();
			//using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true) ;
			try
			{
				using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
				{

					foreach (var audioFile in getAllAudioFile)
					{
						var fileName = $"{getTrackDetails.TrackName}.{audioFile.FileExtension}";
						if (audioFile.ContentType.Equals(ApplicationStaticValue.ContentTypeMp3))
						{
							var downloadResult = await _audioFileService.DownloadPrivateMp3Audio(getTrackDetails.Id);
							if (downloadResult.isSuccess is false)
								throw new Exception();
							var blobResult = downloadResult.Value;
							var entry = zipArchive.CreateEntry(fileName, CompressionLevel.Fastest);
							using (var entryStream = entry.Open())
							{
								if (blobResult.Stream.CanSeek)
								{
									blobResult.Stream.Seek(0, SeekOrigin.Begin);
								}
								//blobResult.Stream.Position = 0;
								await blobResult.Stream.CopyToAsync(entryStream);
							}
							blobResult.Stream.Dispose();
						}
						else if (audioFile.ContentType.Equals(ApplicationStaticValue.ContentTypeWav))
						{
							var downloadResult = await _audioFileService.DownloadPrivateWavAudio(getTrackDetails.Id);
							if (downloadResult.isSuccess is false)
								throw new Exception();
							var blobResult = downloadResult.Value;
							var entry = zipArchive.CreateEntry(fileName, CompressionLevel.Fastest);
							using (var entryStream = entry.Open())
							{
								if (blobResult.Stream.CanSeek)
								{
									blobResult.Stream.Seek(0, SeekOrigin.Begin);
								}
								//blobResult.Stream.Position = 0;
								await blobResult.Stream.CopyToAsync(entryStream);
							}
							blobResult.Stream.Dispose();
						}
						else
						{
							continue;
						}
					}
					foreach (var license in getTrackLicenses)
					{
						var fileName = $"{license.LicenceName}.pdf";
						var downloadLicenseResult = await _licenseFileService.DownloadLicensePdf(license);
						if (downloadLicenseResult.isSuccess is false)
							throw new Exception("license error");
						var licenseResult = downloadLicenseResult.Value;
						var entry = zipArchive.CreateEntry(fileName, CompressionLevel.Fastest);
						using (var entryStream = entry.Open())
						{
							if (licenseResult.Stream.CanSeek)
							{
								licenseResult.Stream.Seek(0, SeekOrigin.Begin);
							}
							//licenseResult.Stream.Position = 0;
							await licenseResult.Stream.CopyToAsync(entryStream);
						}
						licenseResult.Stream.Dispose();
					}
				};
				//memoryStream.Position = 0;
				//memoryStream.Seek(0, SeekOrigin.Begin);
				//using (var fileStream = new FileStream("D:\\Course_8_project_file\\EXE201\\BeatVisionProject\\BeatVisionProject\\wwwroot\\output.zip", FileMode.Create, FileAccess.Write))
				//{
				//	memoryStream.WriteTo(fileStream);
				//}
				memoryStream.Seek(0, SeekOrigin.Begin);
				var newResponseDto = new BlobFileResponseDto
				{
					Stream = memoryStream,
					ContentType = ApplicationStaticValue.ContentTypeZip,
				};
				return Result<BlobFileResponseDto>.Success(newResponseDto);
			}
			catch (Exception ex)
			{
				memoryStream.Dispose();
				error.isException = true;
				error.ErrorMessage = ex.Message;
				error.StatusCode = StatusCodes.Status500InternalServerError;
				return Result<BlobFileResponseDto>.Fail();
			}
		}
		private bool ValidateIfTrackIsForDownload(Track track)
		{
			if (track.PublishDateTime.HasValue is false)
			{
				return false;
			}
			return true;
		}
		private void MapCorrectImageUrl(TrackResponseDto track)
		{
			track.ProfileBlobUrl = string.IsNullOrEmpty(track.ProfileBlobUrl)
					? _appSettings.ExternalUrls.AzureBlobBaseUrl + "/public/" + ApplicationStaticValue.DefaultTrackImageName
					: _appSettings.ExternalUrls.AzureBlobBaseUrl + "/public/" + track.ProfileBlobUrl;
		
		}
	}

}
