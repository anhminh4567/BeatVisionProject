using Shared.Enums;
using Shared.IdentityConfiguration;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
	public class UserProfileDto
	{
		public int Id { get; set; }
		//public int IdentityId { get; set; } // Required foreign key property
		//public CustomIdentityUser IdentityUser { get; set; } = null!;// Required reference navigation to principal
		public string? Description { get; set; }
		public string Fullname { get; set; }
		public string? ProfileBlobUrl { get; set; }
		public DateTime? Birthday { get; set; }
		public AcccountStatus AccountStatus { get; set; }
		public string? Caption { get; set; }
		public int TotalTrack { get; set; }
		public string? Instagram { get; set; }
		public string? Youtube { get; set; }
		public string? SoundCloud { get; set; }
		public string? Facebook { get; set; }
		public IList<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
		public IList<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
		public IList<MessageDto> CreatedMessage { get; set; } = new List<MessageDto>();
	}
}
