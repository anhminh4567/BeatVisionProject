using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public abstract class Comment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }	
		public string? Content { get; set; }
		public DateTime CreateDate { get; set; } = DateTime.Now;
		public int? AuthorId { get; set; }
		public UserProfile? Author { get; set; }
		public int LikesCount { get; set; }
		[Column(TypeName = "nvarchar(30)")]
		public CommentType CommentType { get; set; }
		public int? ReplyToCommentId { get; set; }
		public Comment? ReplyToComment { get; set; }
		public bool IsCommentRemoved { get; set; } = false;
	}
	public class TrackComment: Comment
	{
		public TrackComment()
		{
			CommentType = CommentType.TRACK;
		}

		public int? TrackId { get; set; } 
		public Track? Track { get; set; }
		
	}
}