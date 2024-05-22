using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestDto
{
	internal class PlayListDto
	{
		//public string Name { get; set; }
		//public DateTime CreateDate { get; set; } = DateTime.Now;
		//public int PlayCount { get; set; }
		//public int OwnerId { get; set; }
		//public UserProfile Owner { get; set; }
		//public IList<Track> Tracks { get; set; } = new List<Track>();
		//public bool IsPrivate { get; set; }
	}
	public class CreatePlayListDto
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public bool IsPrivate { get; set; } 
	}
	public class UpdatePlayListDto
	{
		[Required]
		[NotNull]
		public int PlayListId { get; set; }
		[Required]
		public string Name { get; set; }
		[Required]
		public bool IsPrivate { get; set; }
		public IList<int> AddedTrackId { get; set; } = new List<int>();
		public IList<int> RemovedTrackId { get; set; } = new List<int>();
	}
}
