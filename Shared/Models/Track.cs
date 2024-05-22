using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public class Track
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }	
		public string TrackName { get; set; }
		public int OwnerId { get; set; }
		public UserProfile? Owner { get; set; }
		public int PlayCount { get; set; }
		public bool IsAudioPrivate { get; set; }
		public bool IsAudioRemoved { get; set; } = false;
		public bool IsAudioForSale { get; set; } = false;
		public decimal Price { get; set; } = decimal.Zero;
		public double AudioLenghtSeconds { get; set; }
		public int? AudioBpm { get; set; } = null;
		public int? AudioChannels { get; set; }
		public int? AudioSampleRate { get; set; }
		public int? AudioBitPerSample { get; set; }
		[Required]
		public int AudioBlobId { get; set; }
		public BlobFileData AudioFile { get; set; }
		public string? ProfileBlobUrl { get; set; }
		//public int? BannerBlobId { get; set; }
		//public BlobFileData? BannerBlobFile{ get; set; }

		[Column(TypeName = "nvarchar(30)")]
		public TrackStatus Status { get; set; } = TrackStatus.NOT_FOR_PUBLISH;
		public bool IsPublished { get; set; } = false;
		public DateTime? PublishDateTime { get; set; }
		public IList<TrackComment> Comments { get; set; } = new List<TrackComment>();
		public IList<Tag> Tags { get; set; } = new List<Tag>();
		public IList<TrackLicense> Licenses { get; set; } = new List<TrackLicense>();
	}
}