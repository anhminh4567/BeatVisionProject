using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Services.Implementation;
using Shared;
using Shared.Helper;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AudioFileController : ControllerBase
	{
		private AudioFileServices _audioFileServices;
		private IUnitOfWork _unitOfWork;

		public AudioFileController(AudioFileServices audioFileServices, IUnitOfWork unitOfWork)
		{
			_audioFileServices = audioFileServices;
			_unitOfWork = unitOfWork;
		}
		[HttpPost]
		[RequestSizeLimit(long.MaxValue)]
		public async Task<IActionResult> UploadWavFile(IFormFile file, int userProfileId,bool isPaid, CancellationToken cancellation = default) 
		{
			var getFileExtension =await _audioFileServices.ValidateFileExtension(file.FileName);
			if (getFileExtension.Value != "wav")
			{
				return BadRequest();
			}
			var contentType = file.ContentType;
			var getUserProfile= await _unitOfWork.Repositories.userProfileRepository.GetById(userProfileId);
			var randomGeneratedFile = Guid.NewGuid().ToString();
			using Stream filestream = file.OpenReadStream();
			using Stream mp3StreamFile = new MemoryStream();
			var converResult = _audioFileServices.ConvertWavToMp3(filestream, mp3StreamFile);
			filestream.Position = 0;
			var analyseWavFile =  _audioFileServices.AnalizeWavAudioFile(filestream,getFileExtension.Value).Value;
			filestream.Position = 0;
			mp3StreamFile.Position = 0;
			var mp3Filename = _audioFileServices.ConvertFilename_To_Mp3(file.FileName).Value;
			await _unitOfWork.BeginTransactionAsync();
			if (isPaid)
			{
				var uploadResult = await _audioFileServices.UploadWavAudioFile(filestream,contentType,getUserProfile,file.FileName,randomGeneratedFile,true,cancellation);
				var mp3UploadResult = await _audioFileServices.UploadMp3AudioFile(mp3StreamFile, ApplicationStaticValue.ContentTypeMp3, getUserProfile, mp3Filename, randomGeneratedFile, false, true, cancellation);
				var createDate   =  DateTime.Now;
				var wavBlobCreateResult = await _unitOfWork.Repositories.blobFileDataRepository.Create(new Shared.Models.BlobFileData()
				{
					ContentType = contentType,
					DirectoryType = Shared.Enums.BlobDirectoryType.Private,
					FileExtension = getFileExtension.Value,
					GeneratedName = randomGeneratedFile,
					IsPaidContent = isPaid,
					IsPublicAccess = false,
					UploadedDate = createDate,
					OriginalFileName = file.FileName,
					PathUrl = uploadResult.Value,
					SizeMb = FileHelper.GetFileSizeMegabyte(file.Length, 1)
				});
				var mp3BlobCreateResult = await _unitOfWork.Repositories.blobFileDataRepository.Create(new Shared.Models.BlobFileData()
				{
					ContentType = ApplicationStaticValue.ContentTypeMp3,
					DirectoryType = Shared.Enums.BlobDirectoryType.Private,
					FileExtension = "mp3",
					GeneratedName = randomGeneratedFile,
					IsPaidContent = isPaid,
					IsPublicAccess = false,
					UploadedDate = createDate,
					OriginalFileName = file.FileName,
					PathUrl = mp3UploadResult.Value,
					SizeMb = FileHelper.GetFileSizeMegabyte(mp3StreamFile.Length, 1)
				});
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.Repositories.trackRepository.Create(new Shared.Models.Track()
				{
					OwnerId = getUserProfile.Id,
					AudioBlobId = wavBlobCreateResult.Id,
					IsAudioForSale = isPaid,
					IsPublished = false,
					IsAudioPrivate = true,
					IsAudioRemoved = false,
					PlayCount = 0,
					TrackName = "test",
					AudioBitPerSample = analyseWavFile.BitPerSample,
					AudioChannels = analyseWavFile.Channels,
					AudioBpm = analyseWavFile.BitPerSample,
					AudioLenghtSeconds = analyseWavFile.SecondLenght,
					AudioSampleRate = analyseWavFile.SampleRate,
					PublishDateTime = createDate,
				});
				await _unitOfWork.SaveChangesAsync();
			}
			if (isPaid is false)
			{
				var uploadResult = await _audioFileServices.UploadWavAudioFile(filestream, contentType, getUserProfile, file.FileName, randomGeneratedFile, false, cancellation);
				var mp3UploadResult = await _audioFileServices.UploadMp3AudioFile(mp3StreamFile, ApplicationStaticValue.ContentTypeMp3, getUserProfile, mp3Filename, randomGeneratedFile, false, false, cancellation);
			}
			//var uploadResult = await _audioFileServices.UploadWavAudioFile(filestream,contentType,getUserProfile, file.FileName,randomGeneratedFile,true, cancellation);
			await _unitOfWork.CommitAsync();
			return Ok();
		}
	}
}
