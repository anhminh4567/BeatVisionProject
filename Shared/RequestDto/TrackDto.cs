using Microsoft.AspNetCore.Http;
using Shared.Enums;
using Shared.Models;
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
		public IFormFile uploadedFile { get; set; }
		[AllowNull]
		public IFormFile? bannderFile { get; set; }
		[Required]
		[MinLength(1)]
		public string TrackName { get; set; }
		[Required]
		public bool IsTrackPaidContent { get; set; }
		[Required]
		public IList<Tag> Tags { get; set; } = new List<Tag>();
		public string? ProfileBlobUrl { get; set; } = null;
	}
	public class PublishTrackDto
	{
		[Required]
		[NotNull]
		public int TrackId { get; set; }
		[Required]
		public DateTime PublishDate { get; set; }
		[Required]
		public bool IsPublishNow { get; set; } = true;
		[Required]
		public bool IsTrackPaid { get; set; } = false;
		[Range(0, 5000000)]
		public decimal Price { get; set; } = 0;
	}
	public class RemovePublishTrackDto
	{

	}
	public class UpdatePublishtrackDto
	{

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
