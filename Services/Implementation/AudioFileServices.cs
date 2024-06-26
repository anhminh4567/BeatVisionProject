﻿using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using NAudio.Lame;
using NAudio.Wave;
using Repository.Interface;
using Shared;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.Poco;
using Shared.ResponseDto;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Services.Implementation
{
	public class AudioFileServices
	{
		private readonly FileService _fileService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly AppsettingBinding _appsettings;
		// AUDIO FILE CONVENTION
		// PRIVATE CONTAINER:
		// - PAID       : audio/paid/{filenameGen}/{filenameGen.mp3/wav}  
		// - PRIVATE    : audio/{filenameGen}/{filenameGen.mp3/wav}

		// PUBLIC CONTAINER:
		// - PUBLIC     : audio/{filenameGen.mp3}
		public AudioFileServices(FileService fileService, IUnitOfWork unitOfWork, AppsettingBinding appsettings)
		{
			_fileService = fileService;
			_unitOfWork = unitOfWork;
			_appsettings = appsettings;
		}
		public async Task<Result<string>> UploadWavAudioFile(Stream fileStream, string contentType, string fileName, string randomFileName, bool isPaidContent, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var audioFolderDirectory = ApplicationStaticValue.BlobAudioDirectory;
			var paidDirectory = ApplicationStaticValue.BlobPaidDirectory;
			var fileExtensionResult = await ValidateFileExtension(fileName);
			if (fileExtensionResult.isSuccess is false)
			{
				error.ErrorMessage = "file extension problem";
				return Result.Fail(error);
			}
			var fileExtension = "wav";//fileExtensionResult.Value;
			if (isPaidContent)
			{
				//var relativeFilePath = audioFolderDirectory + "/" + userProfile.Id + "/" + paidDirectory + "/" + randomFileName + "." + fileExtension;
				var relativeFilePath = audioFolderDirectory + "/"  + paidDirectory + "/" + randomFileName + "/" + randomFileName + "." + fileExtension;
				var uploadResult = await _fileService.UploadFileAsync(fileStream, contentType, relativeFilePath, BlobDirectoryType.Private, cancellationToken);
				if (uploadResult.isSuccess is false)
				{
					error.ErrorMessage = "error in upload file, now return";
					return Result.Fail(error);
				}
				return Result<string>.Success(relativeFilePath);
			}
			else
			{
				//var relativeFilePath = audioFolderDirectory + "/" + userProfile.Id + "/" + randomFileName + "." + fileExtension;
				var relativeFilePath = audioFolderDirectory + "/" + randomFileName + "/" + randomFileName + "." + fileExtension;
				var uploadResult = await _fileService.UploadFileAsync(fileStream, contentType, relativeFilePath, BlobDirectoryType.Private, cancellationToken);
				if (uploadResult.isSuccess is false)
				{
					error.ErrorMessage = "error in upload file, now return";
					return Result.Fail(error);
				}
				return Result<string>.Success(relativeFilePath);
			}
		}
		public async Task<Result<string>> UploadMp3AudioFile(Stream fileStream, string contentType,string fileName, string randomFileName, bool isPublic, bool isPaidContent, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var audioFolderDirectory = ApplicationStaticValue.BlobAudioDirectory;
			var paidDirectory = ApplicationStaticValue.BlobPaidDirectory;
			var fileExtensionResult = await ValidateFileExtension(fileName);
			if (fileExtensionResult.isSuccess is false)
			{
				error.ErrorMessage = "file extension problem";
				return Result.Fail(error);
			}
			var fileExtension = "mp3";
			if (isPaidContent)
			{
				var relativeFilePath = audioFolderDirectory + "/" + paidDirectory + "/" + randomFileName + "/" + randomFileName + "." + fileExtension;
				
				var uploadResult = await _fileService.UploadFileAsync(fileStream, contentType, relativeFilePath, BlobDirectoryType.Private, cancellationToken);
				if (uploadResult.isSuccess is false)
				{
					error.ErrorMessage = "error in upload file, now return";
					return Result.Fail(error);
				}
				return Result<string>.Success(relativeFilePath);

			}
			else if (isPublic is false)
			{
				var relativeFilePath = audioFolderDirectory + "/" + randomFileName + "/" + randomFileName + "." + fileExtension;

				var uploadResult = await _fileService.UploadFileAsync(fileStream, contentType, relativeFilePath, BlobDirectoryType.Private, cancellationToken);
				if (uploadResult.isSuccess is false)
				{
					error.ErrorMessage = "error in upload file, now return";
					return Result.Fail(error);
				}
				return Result<string>.Success(relativeFilePath);

			}
			else if (isPublic)
			{
				var relativeFilePath = audioFolderDirectory + "/"  + randomFileName + "." + fileExtension;
				var uploadResult = await _fileService.UploadFileAsync(fileStream, contentType, relativeFilePath, BlobDirectoryType.Public, cancellationToken);
				if (uploadResult.isSuccess is false)
				{
					error.ErrorMessage = "error in upload file, now return";
					return Result.Fail(error);
				}
				return Result<string>.Success(relativeFilePath);
			}
			else
			{
				error.ErrorMessage = "unknown where to save the file";
				return Result.Fail();
			}
		}
		public async Task<Result<BlobFileResponseDto>> DownloadPrivateMp3Audio( int trackId, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(trackId, "AudioFile");
			if (getTrack == null)
			{
				error.ErrorMessage = "no track exist";
				return Result<BlobFileResponseDto>.Fail(error);
			}
			var blobFile = getTrack.AudioFile;
			var fileName = blobFile.GeneratedName + "." + blobFile.FileExtension;
			// just get the mp3 file, which mean the id does not equal the one on track AudioBlobId
			var getAllBlobFilesWithNames = await _unitOfWork.Repositories.blobFileDataRepository
				.GetByCondition(f => f.GeneratedName == blobFile.GeneratedName && f.Id != getTrack.AudioBlobId);
			if (blobFile.IsPaidContent)
			{
				BlobFileData? getCorrectMp3File = getAllBlobFilesWithNames
					.FirstOrDefault(f => f.IsPaidContent == true && f.IsPublicAccess == false); 
				if (getCorrectMp3File == null)
				{
					error.ErrorMessage = "file not found on server";
					return Result<BlobFileResponseDto>.Fail();
				}
				return await _fileService.DownloadFileAsync(getCorrectMp3File.PathUrl, BlobDirectoryType.Private, cancellationToken);
			}
			else
			{
				var getCorrectMp3File = getAllBlobFilesWithNames
					.FirstOrDefault(f => f.IsPaidContent == false && f.IsPublicAccess == false);
				if (getCorrectMp3File == null)
				{
					error.ErrorMessage = "file not found on server";
					return Result<BlobFileResponseDto>.Fail();
				}
				return await _fileService.DownloadFileAsync(getCorrectMp3File.PathUrl, BlobDirectoryType.Private, cancellationToken);
			}
		}
		public async Task<Result<BlobFileResponseDto>> DownloadPrivateWavAudio(int trackId, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(trackId, "AudioFile");
			if (getTrack == null)
			{
				error.ErrorMessage = "no track exist";
				return Result<BlobFileResponseDto>.Fail(error);
			}
			var blobFile = getTrack.AudioFile;
			var fileName = blobFile.GeneratedName + "." + blobFile.FileExtension;
			return await _fileService.DownloadFileAsync(blobFile.PathUrl, BlobDirectoryType.Private, cancellationToken);


		}
		public async Task<Result<BlobFileResponseDto>> DownloadPublicMp3Audio( int trackId, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetByIdInclude(trackId, "AudioFile");
			if (getTrack == null)
			{
				error.ErrorMessage = "no such track";
				return Result<BlobFileResponseDto>.Fail();
			}
			var blobFile = getTrack.AudioFile;
			var fileName = blobFile.GeneratedName + "." + blobFile.FileExtension;
			// just get the mp3 file, which mean the id does not equal the one on track AudioBlobId
			var getAllBlobFilesWithNames = await _unitOfWork.Repositories.blobFileDataRepository
				.GetByCondition(f => f.GeneratedName == blobFile.GeneratedName && f.Id != getTrack.AudioBlobId);
			var getCorrectMp3File = getAllBlobFilesWithNames
				.FirstOrDefault(f => f.IsPublicAccess == true && f.DirectoryType == BlobDirectoryType.Public);
			if (getCorrectMp3File == null)
			{
				error.ErrorMessage = "file not found on server";
				return Result<BlobFileResponseDto>.Fail();
			}
			return await _fileService.DownloadFileAsync(getCorrectMp3File.PathUrl, BlobDirectoryType.Public, cancellationToken);
		}
		public async Task<Result> DeleteAudioFile_Any(string absoluteUri,BlobDirectoryType type, CancellationToken cancellationToken = default)
		{
			return await _fileService.DeleteFileAsync(absoluteUri,type,cancellationToken);
		}
		public async Task<Result> AttachAudioToMail()
		{
			return Result.Fail();
		}
		public async Task<Result<string>> ValidateFileExtension(string filename)
		{
			var allowedAudioExtension = _appsettings.AppConstraints.AllowAudioExtension;
			var getExtensionResult = FileHelper.ExtractFileExtention(filename);
			if (getExtensionResult.isSuccess is false)
			{
				return Result.Fail();
			}
			var getExtentions = getExtensionResult.Value;
			var validateExtensionResult = FileHelper.IsExtensionAllowed(getExtentions, allowedAudioExtension);
			if (validateExtensionResult.isSuccess is false)
			{
				return Result.Fail();
			}
			return Result<string>.Success(getExtentions);
		}
		public Result<WavFileMetadata> AnalizeWavAudioFile(Stream fileStream, string fileExtention)
		{
			var error = new Error();
			try
			{
				var metadata = new WavFileMetadata();
				using (var wavFileReader = new WaveFileReader(fileStream))
				{
					var secondLength = wavFileReader.TotalTime;
					var bitPerSample = wavFileReader.WaveFormat.BitsPerSample;
					var channelsAmount = wavFileReader.WaveFormat.Channels;
					var sampleRate = wavFileReader.WaveFormat.SampleRate;
					var sampleCount = wavFileReader.SampleCount;
					metadata.SecondLenght = secondLength.TotalSeconds;
					metadata.SampleCount = sampleCount;
					metadata.Channels = channelsAmount;
					metadata.SampleCount = sampleCount;
					metadata.SampleRate = sampleRate;
					metadata.BitPerSample = bitPerSample;
					var getSize = FileHelper.GetFileSizeMegabyte(fileStream.Length, 2);
					metadata.SizeMb = getSize;
				}
				return Result<WavFileMetadata>.Success(metadata);
			}
			catch (Exception ex)
			{
				error.isException = true;
				error.ErrorMessage = ex.Message;
				return Result<WavFileMetadata>.Fail(error);
			}
		}
		public Result<Mp3FileMetadata> AnalizeMp3AudioFile(Stream fileStream, string fileExtention)
		{
			var error = new Error();
			try
			{
				var metadata = new Mp3FileMetadata();
				using (var wavFileReader = new Mp3FileReader(fileStream))
				{
					var secondLength = wavFileReader.TotalTime;
					metadata.SecondLenght = secondLength.TotalSeconds;
					var getSize = FileHelper.GetFileSizeMegabyte(fileStream.Length,2);
					metadata.SizeMb = getSize;
				}
				return Result<Mp3FileMetadata>.Success(metadata);
			}
			catch (Exception ex)
			{
				error.isException = true;
				error.ErrorMessage = ex.Message;
				return Result<Mp3FileMetadata>.Fail(error);
			}
		}
		public Result ConvertWavToMp3(Stream wavStream, Stream mp3Stream)
		{
			var error = new Error();
			try
			{
				using (var wavReader = new WaveFileReader(wavStream))
				{
					using (var mp3Writer = new LameMP3FileWriter(mp3Stream, wavReader.WaveFormat, LAMEPreset.STANDARD))
					{
						wavReader.CopyTo(mp3Writer);
					}
				}
				return Result.Success();
			}
			catch (Exception ex)
			{
				error.isException = true;
				error.ErrorMessage = ex.Message;
				return Result.Fail();
			}
		}
		public Result ConvertMp3ToWav(Stream mp3Stream, Stream wavStream)
		{
			var error = new Error();
			try
			{
				using (var mp3Reader = new Mp3FileReader(mp3Stream))
				{
					using (var wavWriter = new WaveFileWriter(wavStream, mp3Reader.WaveFormat))
					{
						byte[] buffer = new byte[mp3Reader.WaveFormat.SampleRate * mp3Reader.WaveFormat.Channels * 2];
						int bytesRead;
						while ((bytesRead = mp3Reader.Read(buffer, 0, buffer.Length)) > 0)
						{
							wavWriter.Write(buffer, 0, bytesRead);
						}
					}
				}
				return Result.Success();
			}
			catch (Exception ex)
			{
				error.isException = true;
				error.ErrorMessage = ex.Message;
				return Result.Fail();
			}
		}
		public Result<string> ConvertFileNameExtension(string originalName, string extentsionToChangeTo) 
		{
			if (string.IsNullOrEmpty(originalName))
			{
				return Result.Fail();
			}
			var splitedName = originalName.Split(".");
			if(splitedName.Length <= 1 ) 
			{
				return Result.Fail();
			}
			var listSplitedName = splitedName.ToList();
			listSplitedName.RemoveAt(splitedName.Length - 1) ;
			StringBuilder builder = new StringBuilder("");
			foreach(var item in listSplitedName)
			{
				builder.Append(item);
			}
			builder.Append(".");
			builder.Append(extentsionToChangeTo);
			return Result<string>.Success(builder.ToString());
		}
		public Result<string> ConvertFilename_To_Mp3(string wavFileName) 
		{
			return ConvertFileNameExtension(wavFileName, "mp3");
		}
		public Result<string> ConvertFilename_To_Wav(string mp3Filename)
		{
			return ConvertFileNameExtension(mp3Filename, "wav");
		}
		public Result IsWavFile(Stream fs, string filename)
		{
			var error = new Error();
			var fileExtResult = FileHelper.ExtractFileExtention(filename);
			if(fileExtResult.isSuccess is false || fileExtResult.Value != "wav")
			{
				error.ErrorMessage = "file is not wav file, cannot upload, require wav file for high quality";
				return Result.Fail(error);
			}
			var tryReadResult = AnalizeWavAudioFile(fs, fileExtResult.Value);
			if(tryReadResult.isSuccess is false)
			{
				error.isException = tryReadResult.Error.isException;
				error.ErrorMessage = tryReadResult.Error.ErrorMessage;
				return Result.Fail(error);
			}

			return Result.Success() ;
		}
		public Result IsMp3File(Stream fs, string filename)
		{
			var error = new Error();
			var fileExtResult = FileHelper.ExtractFileExtention(filename);
			if (fileExtResult.isSuccess is false || fileExtResult.Value != "mp3")
			{
				error.ErrorMessage = "file is not mp3 file, cannot upload";
				return Result.Fail(error);
			}
			var tryReadResult = AnalizeMp3AudioFile(fs, fileExtResult.Value);
			if (tryReadResult.isSuccess is false)
			{
				error.isException = tryReadResult.Error.isException;
				error.ErrorMessage = tryReadResult.Error.ErrorMessage;
				return Result.Fail(error);
			}
			return Result.Success();
		}
	}
}
