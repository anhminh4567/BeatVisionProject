using Microsoft.CodeAnalysis.CSharp.Syntax;
using Repository.Interface;
using Shared;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Models;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class LicenseFileService
	{
		private readonly FileService _fileService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly AppsettingBinding _appsettings;
		private const string _allowedFileNameRegrex = @"^[a-zA-Z0-9_,]+$";

		public LicenseFileService(FileService fileService, IUnitOfWork unitOfWork, AppsettingBinding appsettings)
		{
			_fileService = fileService;
			_unitOfWork = unitOfWork;
			_appsettings = appsettings;
		}
		public async Task<Result<string>> UploadLicenseFile(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
		{
			var error = new Error() { StatusCode = (int)HttpStatusCode.BadRequest };
			var allowedLicenseExtionsion = _appsettings.AppConstraints.AllowLicenseExtension;
			var licenseFolderDirectory = ApplicationStaticValue.BlobLicenseDirectory;
			var fileExtensionResult = FileHelper.ExtractFileExtention(fileName);
			if (fileExtensionResult.isSuccess is false)
			{
				error.ErrorMessage = "file extension problem";
				return Result<string>.Fail(error);
			}
			var getFileNameResult = FileHelper.ExtractFileNameFromExtension(fileName);
			if(getFileNameResult.isSuccess is false)
			{
				return Result<string>.Fail(getFileNameResult.Error);
			}
			if (FileHelper.CheckFileNameAgainstRegrex(getFileNameResult.Value, _allowedFileNameRegrex) is false)
			{
				error.ErrorMessage = "file name not allowed, only letter and number, no whitespace, allow underscore and comma";
				return Result<string>.Fail(error);
			}
			var fileExtension = fileExtensionResult.Value;
			var validateExtensionResult = FileHelper.IsExtensionAllowed(fileExtension, allowedLicenseExtionsion);
			if (validateExtensionResult.isSuccess is false)
			{
				error.ErrorMessage = "file extension is not allowed";
				return Result<string>.Fail(error);
			}
			var relativeFilePath = licenseFolderDirectory + "/" + fileName + "." + fileExtension;
			var uploadResult = await _fileService.UploadFileAsync(fileStream, contentType, relativeFilePath, BlobDirectoryType.Private);
			if (uploadResult.isSuccess is false)
			{
				error.ErrorMessage = "error in upload file, now return";
				return Result<string>.Fail(error);
			}
			return Result<string>.Success(relativeFilePath);
		}
		public async Task<Result> DeleteLicenseFile(string relativeFilePath, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var deleteResult = await _fileService.DeleteFileAsync(relativeFilePath, BlobDirectoryType.Private, cancellationToken);
			if (deleteResult.isSuccess is false)
			{
				error.ErrorMessage = "error in delete file";
				return Result.Fail();
			}
			return Result.Success();
		}
		public async Task<Result<BlobFileResponseDto>> DownloadLicensePdf(TrackLicense trackLicense, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var filePath = trackLicense.LicensePdfBlobPath;
			return await _fileService.DownloadFileAsync(filePath, BlobDirectoryType.Private, cancellationToken);
		}
	}
}
