using Microsoft.AspNetCore.Http;
using Repository.Interface;
using Shared;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.RequestDto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class TrackManager
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly AudioFileServices _audioFileService;
		private readonly ImageFileServices _imageFileService;
		private readonly TagService _tagService;
		private readonly AppsettingBinding _appSettings;

		public TrackManager(IUnitOfWork unitOfWork, AudioFileServices audioFileService, ImageFileServices imageFileService, TagService tagService, AppsettingBinding appSettings)
		{
			_unitOfWork = unitOfWork;
			_audioFileService = audioFileService;
			_imageFileService = imageFileService;
			_tagService = tagService;
			_appSettings = appSettings;
		}
		//create a track will always be private and not for sales, not until the publish is made will the track be decided to be public or not

		public async Task<Result> UploadTrack(CreateTrackDto createTrackDto,UserProfile userProfile, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var formFile = createTrackDto.uploadedFile;
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
				OwnerId = userProfile.Id,
				PublishDateTime = null,
				Tags = createTrackDto.Tags,
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
				var uploadWAVResult = await _audioFileService.UploadWavAudioFile(fileStream, ApplicationStaticValue.ContentTypeWav, userProfile, formFile.FileName, RANDOM_GENERATED_NAME, createTrackDto.IsTrackPaidContent, cancellationToken);
				if (uploadWAVResult.isSuccess is false)
				{
					error = uploadWAVResult.Error;
					return Result.Fail(error);
				}
				var uploadMP3Result = await _audioFileService.UploadMp3AudioFile(mp3StreamFile, ApplicationStaticValue.ContentTypeMp3, userProfile, formFile.FileName, RANDOM_GENERATED_NAME, false, createTrackDto.IsTrackPaidContent, cancellationToken);
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
				if (string.IsNullOrEmpty(createTrackDto.ProfileBlobUrl) )
				{
					var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(newTrack.Id);
					getTrack.ProfileBlobUrl = createTrackDto.ProfileBlobUrl;
					await _unitOfWork.Repositories.trackRepository.Update(getTrack);
					await _unitOfWork.SaveChangesAsync();
				} else if (createTrackDto.bannderFile is not null) 
				{
					var imageFile = createTrackDto.bannderFile;
					using Stream imageStream = imageFile.OpenReadStream();
					var uploadProfile = await _imageFileService.UploadNewImage(imageStream, imageFile.FileName,RANDOM_GENERATED_NAME,imageFile.ContentType,cancellationToken);
					var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(newTrack.Id);
					getTrack.ProfileBlobUrl = uploadProfile.Value;
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

		public async Task<Result> SetPublishTrack(PublishTrackDto publishTrackDto, UserProfile userProfile) 
		{
			var error = new Error();
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(publishTrackDto.TrackId);
			if(getTrack is null)
			{
				return Result.Fail();
			}
			DateTime PUBLISH_DATETIME = DateTime.Now;
			if (publishTrackDto.IsPublishNow)
			{
				PUBLISH_DATETIME.AddMinutes(1);
				getTrack.PublishDateTime = PUBLISH_DATETIME;
			}
			else
			{
				if (DateTime.Compare(publishTrackDto.PublishDate, PUBLISH_DATETIME) <=  0) 
				{
					error.ErrorMessage = "date is not appropriate, please make it later than now, about 1-2 minute";
					return Result.Fail();
				}
				PUBLISH_DATETIME = publishTrackDto.PublishDate;
				getTrack.PublishDateTime = PUBLISH_DATETIME;
			}
			if(publishTrackDto.IsTrackPaid)
			{
				getTrack.IsAudioForSale = true;
				getTrack.Price = publishTrackDto.Price;
			}
			getTrack.Status = TrackStatus.WAIT_FOR_PUBLISH;
			await _unitOfWork.Repositories.trackRepository.Update(getTrack);
			await _unitOfWork.SaveChangesAsync();
			return Result.Fail();
		}
		//for background service
		public async Task PublishTrack()
		{
			var getTrackTobePublish = await _unitOfWork.Repositories.trackRepository
				.GetByCondition(t => t.IsPublished == false && t.Status == TrackStatus.WAIT_FOR_PUBLISH);
			foreach(var track in getTrackTobePublish) 
			{
				if (track.PublishDateTime is null)
					continue;
				if(DateTime.Compare(track.PublishDateTime.Value, DateTime.Now) >= 0)
				{
					var getUserProfile = await _unitOfWork.Repositories.userProfileRepository.GetById(track.OwnerId);
					var getFileFromPrivateDirectory = await _audioFileService.DownloadPrivateMp3Audio(getUserProfile,track.Id);
					//await _audioFileService.UploadMp3AudioFile();
					track.Status = TrackStatus.PUBLISHED;
					track.IsPublished = true;
					await _unitOfWork.Repositories.trackRepository.Update(track);
				}
			}
			await _unitOfWork.SaveChangesAsync();
			
		}
		// remove the file from public for access
		public async Task<Result> PulldownTrack(Track track)
		{
			var getWAVBlobFile = await _unitOfWork.Repositories.blobFileDataRepository
				.GetById(track.AudioBlobId);
			track.Status = TrackStatus.REMOVED;
			track.IsAudioRemoved = true;
			var WAVfileName = getWAVBlobFile.GeneratedName;
			var getAllRelatedFile = await _unitOfWork.Repositories.blobFileDataRepository
				.GetByCondition(f => f.GeneratedName == WAVfileName);
			var getPublicFile = getAllRelatedFile
				.FirstOrDefault(blob => blob.DirectoryType == BlobDirectoryType.Public);

			return Result.Fail();
		}
		public async Task<Result> MakeAlbumn(CreateTrackDto createTrackDto, UserProfile userProfile)
		{
			var formFile = createTrackDto.uploadedFile;
			return Result.Fail();
		}

		public async Task<Result> PulldownAlbumn()
		{
			return Result.Fail();
		}
	}
}
