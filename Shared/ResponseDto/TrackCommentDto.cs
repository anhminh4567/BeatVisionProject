using Shared.Enums;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
	public class TrackCommentDto
	{
		public int Id { get; set; }
		public string? Content { get; set; }
		public DateTime CreateDate { get; set; }
		public int? AuthorId { get; set; }
		public int LikesCount { get; set; }
		public CommentType CommentType { get; set; }
		public int? ReplyToCommentId { get; set; }
		public bool IsCommentRemoved { get; set; }
		public int? TrackId { get; set; }


	}
	
}
