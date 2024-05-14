using Microsoft.AspNetCore.Http;
using Services.Implementation;
using Shared.Enums;
using Shared.Helper;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IFileService
	{
		Task<Result<string>> UploadFileAsync(Stream stream, string contentType, BlobDirectoryType blobDirectoryType, CancellationToken cancellationToken = default);
		Task<Result<BlobFileResponseDto>> DownloadFileAsync(string fileId, BlobDirectoryType blobDirectoryType, CancellationToken cancellationToken = default);
		IAsyncEnumerable<byte[]> StreamFileSegmentAsync(string filename, HttpContext httpContext, CancellationToken cancellationToken = default);
		Task<Result> DeleteFileAsync(string fileId, BlobDirectoryType blobDirectoryType, CancellationToken cancellationToken = default);
	}
}
