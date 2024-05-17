namespace Shared.Models
{
	public class Album
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int OwnerId { get; set; }
		public UserProfile Owner { get; set; }
		public int PlayCount { get; set; }
		public bool IsPrivate { get; set; }
		public string? ProfileBlobUrl { get; set; }
		//public int? BannerBlobId { get; set; } = null;
		//public BlobFileData? BannerBlobFile { get; set; }
		public IList<Tag> tags { get; set; } = new List<Tag>();
		public IList<AlbumComment> Comments { get; set; } = new List<AlbumComment>();
		public IList<Track> Tracks { get; set; } = new List<Track>();

	}
}