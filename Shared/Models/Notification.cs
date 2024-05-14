using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public class Notification
	{

		public int? SenderId { get; set; }
		public UserProfile? Sender { get; set; }
		public int? MessageId { get; set; }
		public Message? Message { get; set; }
		public bool IsImportant { get; set; } = false;
		public bool IsReaded { get; set; } = false;
		public DateTime ExpiredDate { get; set; }
		//public DateTime CreatedDate { get; set; }
		//public NotificationType Type { get; set; }
		//public bool IsServerNotification { get; set; } = false;


	}
}