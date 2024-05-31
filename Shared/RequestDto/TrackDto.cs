using Microsoft.AspNetCore.Http;

using Shared.Enums;
using Shared.Models;
using Shared.MyAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestDto
{
	public class CreateTrackDto
	{
		[Required]
		[FileType(new string[] { "audio/mpeg", "audio/wav" })]
		public IFormFile uploadedFile { get; set; }
		[AllowNull]
		[FileType(new string[] {"image/jpeg" , "image/png"})]
		public IFormFile? bannderFile { get; set; }
		[Required]
		[MinLength(1)]
		[RegularExpression(@"^[a-zA-Z0-9\s.,]*$")]
		public string TrackName { get; set; }
		[Required]
		public bool IsTrackPaidContent { get; set; }
		[Required]
		public IList<int> TagsId { get; set; } = new List<int>();
		[Required]
		public IList<int> LicenseIds { get; set; } = new List<int>();
		public string? ProfileBlobUrl { get; set; } = null;
	}
	public class UpdateTrackDto
	{
		[Required]
		public int TrackId { get; set; }
		[AllowNull]
		[FileType(new string[] { "image/jpeg", "image/png" })]
		public IFormFile? bannderFile { get; set; }
		[Required]
		[MinLength(1)]
		[RegularExpression(@"^[a-zA-Z0-9\s.,]*$")]
		public string TrackName { get; set; }
		[Required]
		public IList<int> TagsId { get; set; } = new List<int>();
		[Required]
		public IList<int> LicenseIds { get; set; } = new List<int>();
	}
	public class PublishTrackDto
	{
		[Required]
		[NotNull]
		public int TrackId { get; set; }
		[Required]
		[DataType(DataType.DateTime)]
		public DateTime PublishDate { get; set; }
		[Required]
		public bool IsPublishNow { get; set; } = true;
		[Required]
		public bool IsTrackPaid { get; set; } = false;
		[AllowNull]
		[Range(10000, 5000000)]
		public decimal? Price { get; set; } = 0;
	}
	public class RemovePublishTrackDto
	{

	}
	public class UpdatePublishtrackDto
	{
		[Required]
		public int TrackId { get; set; }
		[Required]
		public bool IsRemovePublish { get; set; } = false;
		[Required]
		public bool IsChangePublishDate { get; set; } = true;
		public DateTime PublishDate { get; set; }
		[Required]
		public bool IsChangeTrackPaid { get; set; }  = false;
		[Range(1000, 5000000)]
		public decimal Price { get; set; } = 0;
	}
	
	//public int Id { get; set; }
	//public string TrackName { get; set; }
	//public int OwnerId { get; set; }
	//public UserProfile? Owner { get; set; }
	//public int PlayCount { get; set; }
	//public bool IsAudioPrivate { get; set; }
	//public bool IsAudioRemoved { get; set; } = false;
	//public bool IsAudioForSale { get; set; } = false;
	//public double AudioLenghtSeconds { get; set; }
	//public int? AudioBpm { get; set; } = null;
	//public int? AudioChannels { get; set; }
	//public int? AudioSampleRate { get; set; }
	//public int? AudioBitPerSample { get; set; }
	//[Required]
	//public int AudioBlobId { get; set; }
	//public BlobFileData AudioFile { get; set; }
	//public string? ProfileBlobUrl { get; set; }
	//public int? BannerBlobId { get; set; }
	//public BlobFileData? BannerBlobFile{ get; set; }

	//[Column(TypeName = "nvarchar(30)")]
	//public TrackStatus Status { get; set; }
	//public bool IsPublished { get; set; } = false;
	//public DateTime PublishDateTime { get; set; }
}
