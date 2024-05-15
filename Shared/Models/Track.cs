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
		public int SecondLenghth { get; set; }
		public int PlayCount { get; set; }
		public bool IsAudioPrivate { get; set; }
		public bool IsAudioRemoved { get; set; } = false;
		public bool IsAudioForSale { get; set; }
		public int AudioLenghtSeconds { get; set; }
		public int? AudioBpm { get; set; } = null;
		[Required]
		public int AudioBlobId { get; set; }
		public BlobFileData AudioFile { get; set; }
		public int? BannerBlobId { get; set; }
		public BlobFileData? BannerBlobFile{ get; set; }

		[Column(TypeName = "nvarchar(30)")]
		public TrackStatus Status { get; set; }
		public IList<TrackComment> Comments { get; set; } = new List<TrackComment>();
		public IList<Tag> Tags { get; set; } = new List<Tag>();
		public IList<TrackLicense> Licenses { get; set; } = new List<TrackLicense>();
		public IList<Album> Albums { get; set; } = new List<Album>();
		public IList<PlayList> PlayLists { get; set; } = new List<PlayList>();
	}
}