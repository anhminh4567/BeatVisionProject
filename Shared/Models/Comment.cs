using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public class Comment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }	
		public string Content { get; set; }
		public DateTime CreateDate { get; set; } = DateTime.Now;
		public int AuthorId { get; set; }
		public UserProfile Author { get; set; }
		public int LikesCount { get; set; }
		//[Column(TypeName = "nvarchar(30)")]
		//public CommentType CommentType { get; set; }
		//public int SourcesId { get; set; } // TRACK OR ALBUMN, DEPEND
		public int? TrackId { get; set; } = null;
		public Track? Track { get; set; }
		public int? AlbumId { get; set; } = null;
		public Album Album { get; set; }
		public int? ReplyToCommentId { get; set; }
		public Comment? ReplyToComment { get; set; }
	}
}