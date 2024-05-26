using Shared.Enums;
using Shared.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shared.ResponseDto
{
	public class MessageDto
	{
		public int Id { get; set; }
		public string MessageName { get; set; }
		public string Content { get; set; }
		public DateTime CreatedDate { get; set; }
		public NotificationType Type { get; set; }
		public NotificationWeight Weight { get; set; }
		public int CreatorId { get; set; }
		public bool IsServerNotification { get; set; }
	}
}