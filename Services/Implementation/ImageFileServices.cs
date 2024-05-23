using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Repository.Interface;
using Shared;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class ImageFileServices
	{
		private readonly FileService _fileService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly AppsettingBinding _appsettings;
		public ImageFileServices(FileService fileService, IUnitOfWork unitOfWork, AppsettingBinding appsettingBinding)
		{
			_fileService = fileService;
			_unitOfWork = unitOfWork;
			_appsettings = appsettingBinding;
		}
		//return the string to + with base url to query and save to database
		public async Task<Result<string>> UploadNewImage(Stream fileStream, string fileName, string randomFileName,string contentType,CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var imageFolderDirectory = ApplicationStaticValue.BlobImageDirectory;
			var fileExtensionResult = FileHelper.ExtractFileExtention(fileName);
			if(fileExtensionResult.isSuccess is false)
			{
				error.ErrorMessage = "file extension problem";
				return Result<string>.Fail(error);
			}
			var fileExtension = fileExtensionResult.Value;
			var validateExtensionResult = FileHelper.IsExtensionAllowed(fileExtension, _appsettings.AppConstraints.AllowImageExension);
			if(validateExtensionResult.isSuccess is false)
			{
				error.ErrorMessage = "file extension is not allowed";
				return Result<string>.Fail(error);
			}
			var relativeFilePath = imageFolderDirectory + "/" + randomFileName + "." + fileExtension;
			var uploadResult =  await _fileService.UploadFileAsync(fileStream,   contentType, relativeFilePath, BlobDirectoryType.Public);
			if(uploadResult.isSuccess is false)
			{
				error.ErrorMessage = "error in upload file, now return";
				return Result<string>.Fail(error);
			}
			return Result<string>.Success(relativeFilePath);
		}
		public async Task<Result> DeleteImageFile(string relativeFilePath, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var deleteResult = await _fileService.DeleteFileAsync(relativeFilePath, BlobDirectoryType.Public,cancellationToken);
			if(deleteResult.isSuccess is false) 
			{
				error.ErrorMessage = "error in delete file";
				return Result.Fail();
			}
			return Result.Success();
		}
	}
}
