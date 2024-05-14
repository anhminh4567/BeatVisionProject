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
		public bool IsPrivate { get; set; }
		public bool IsRemoved { get; set; } = false;
		public bool IsForSale { get; set; } 
		public string AudioBlobPath { get; set; }
		public string BannerBlobPath { get; set; }
		[Column(TypeName = "nvarchar(30)")]
		public TrackStatus Status { get; set; }
		public IList<Comment> Comments { get; set; } = new List<Comment>();
		public IList<Tag> Tags { get; set; } = new List<Tag>();
		public IList<TrackLicense> Licenses { get; set; } = new List<TrackLicense>();
		public IList<Album> Albums { get; set; } = new List<Album>();
		public IList<PlayList> PlayLists { get; set; } = new List<PlayList>();
	}
}