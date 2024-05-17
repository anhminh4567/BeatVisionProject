using Shared.Enums;
using Shared.IdentityConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class UserProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int IdentityId { get; set; } // Required foreign key property
        public CustomIdentityUser IdentityUser { get; set; } = null!;// Required reference navigation to principal
        public string? Description { get; set; }
        public string Fullname { get; set; }
        public string? ProfileBlobUrl { get; set; }
		//public int? BannerBlobId { get; set; }
		//public BlobFileData? BannerBlobFile { get; set; }
		public DateTime? Birthday { get; set; }
		[Column(TypeName = "nvarchar(30)")]
		public AcccountStatus AccountStatus { get; set; }
        public string? Caption { get; set; }
        public int TotalTrack { get; set; }
        public int TotalAlbumn { get; set; }
        public string? Instagram { get; set; }
        public string? Youtube { get; set; }
        public string? SoundCloud { get; set; }
        public string? Facebook { get; set; }
        public IList<Track> OwnedTracks { get; set; } = new List<Track>(); 
        public IList<Album> OwnedAlbumbs { get; set; } = new List<Album>();
        public IList<PlayList> SavedPlaylist { get; set; } = new List<PlayList>();
        
        public IList<Comment> Comments { get; set; } = new List<Comment>();
        //public IList<TrackComment> TrackComments { get; set; } = new List<TrackComment>();
        //public IList<AlbumComment> albumComments { get; set; } = new List<AlbumComment>();

        public IList<CartItem> CartItems { get; set; } = new List<CartItem>();

        public IList<UserProfile> Followers { get; set; } = new List<UserProfile>();
        public IList<UserProfile> Followings { get; set; } = new List<UserProfile>();
	
		public IList<Notification> Notifications { get; set; } = new List<Notification>();
		public IList<Message> CreatedMessage { get; set; } = new List<Message>(); 


	}
}
