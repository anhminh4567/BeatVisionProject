using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestDto
{
	public class CreateMessageDto
	{
		[Required]
		public string MessageName { get; set; }
		[Required]
		public string Content { get; set; }

		[Required]
		public NotificationWeight Weight { get; set; }
	}
	public class CreateNotificationForNewTracks : CreateMessageDto
	{
		public int TrackId { get; set; }
	}
	public class AdminCreateMessageDto : CreateMessageDto
	{
		[Required]
		public NotificationType Type { get; set; }
		public int? UserId { get; set; }
	}
}
