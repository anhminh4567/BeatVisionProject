using Shared.Enums;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Shared.RequestDto
{
	public class CreateCommentDto
	{
		[Required]
		public string Content { get; set; }
		[AllowNull]
		public CommentType? CommentType { get; set; }
		[AllowNull]
		public int? ReplyToCommentId { get; set; } = null;
	}
	public class CreateTrackCommentDto : CreateCommentDto
	{
		[Required]
		public int TrackId { get; set; }
		public CreateTrackCommentDto()
		{
			CommentType = Enums.CommentType.TRACK; 
		}
	}
}
