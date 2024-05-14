using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Services.Interface;
using Shared.Enums;
using Shared.Helper;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Error = Shared.Helper.Error;

namespace Services.Implementation
{
    public class FileService : IFileService
	{
		private BlobServiceClient _blobServiceClient;
		private const string PublicBlob = "public";
		private const string PrivateBlob = "private";
		private const string TestBlob = "testBlob";
		private const string PaidContent = "paidcontent";
		private const int BufferSize = 4096;
		public FileService(BlobServiceClient blobServiceClient)
		{
			_blobServiceClient = blobServiceClient;
		}

		public async Task<Result<string>> UploadFileAsync(Stream stream, string contentType,BlobDirectoryType blobDirectoryType, CancellationToken cancellationToken = default)
		{
			var getBlobContainerClient = GetCorrectBlobClient(blobDirectoryType);
			if (getBlobContainerClient.isSuccess is false)
				return Result.Fail();
			var blobContainerClient = getBlobContainerClient.Value;
			var fileIdDemo = Guid.NewGuid();
			var blobClient = blobContainerClient.GetBlobClient(fileIdDemo.ToString());
			var uploadResult = await blobClient.UploadAsync(
				stream,
				new BlobHttpHeaders { ContentType = contentType, },
				cancellationToken: cancellationToken);
			return Result<string>.Success(fileIdDemo.ToString());
		}
		public async Task<Result<BlobFileResponseDto>> DownloadFileAsync(string fileId, BlobDirectoryType blobDirectoryType, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			try
			{
				var getBlobContainerClient = GetCorrectBlobClient(blobDirectoryType);
				if (getBlobContainerClient.isSuccess is false)
					return Result<BlobFileResponseDto>.Fail();
				var blobContainerClient = getBlobContainerClient.Value;
				var blobClient = blobContainerClient.GetBlobClient(fileId.ToString());
				var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
				return Result<BlobFileResponseDto>.Success(new BlobFileResponseDto()
				{
					Stream = response.Value.Content,
					ContentType = response.Value.Details.ContentType,
				});
			}
			catch(RequestFailedException ex) 
			{
				error.StatusCode = StatusCodes.Status500InternalServerError;
				error.ErrorMessage = ex.Message;
				return Result<BlobFileResponseDto>.Fail(error);
			}
			
		}
		public async IAsyncEnumerable<byte[]> StreamFileSegmentAsync(string filename,HttpContext httpContext,CancellationToken cancellationToken = default) 
		{
			yield break;
			throw new NotImplementedException();
			
		}
		public async Task<Result> DeleteFileAsync(string fileId, BlobDirectoryType blobDirectoryType, CancellationToken cancellationToken = default) 
		{
			var getBlobContainerClient = GetCorrectBlobClient(blobDirectoryType);
			if (getBlobContainerClient.isSuccess is false)
				return Result.Fail();
			var blobContainerClient = getBlobContainerClient.Value;
			var blobClient = blobContainerClient.GetBlobClient(fileId);
			var deleteResult = await blobClient.DeleteAsync(cancellationToken:cancellationToken);
			if (deleteResult.IsError)
				return Result.Fail(new Shared.Helper.Error()
				{
					StatusCode = deleteResult.Status,
				});
			return Result.Success();
		}
		private Result<BlobContainerClient> GetCorrectBlobClient(BlobDirectoryType blobDirectoryType)
		{
			BlobContainerClient blobContainerClient;// = _blobServiceClient.GetBlobContainerClient(PublicBlob);
			switch (blobDirectoryType)
			{
				case BlobDirectoryType.Pulbic:
					blobContainerClient = _blobServiceClient.GetBlobContainerClient(PublicBlob);
					break;
				case BlobDirectoryType.PaidContent:
					blobContainerClient = _blobServiceClient.GetBlobContainerClient(PaidContent);
					break;
				case BlobDirectoryType.Private:
					blobContainerClient = _blobServiceClient.GetBlobContainerClient(PrivateBlob);
					break;
				default:
					return Result<BlobContainerClient>.Fail();
			}
			return Result<BlobContainerClient>.Success(blobContainerClient);

		}
	}

	
}
