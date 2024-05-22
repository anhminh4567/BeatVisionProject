using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
	public class Message
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string MessageName { get; set; }
		public string Content { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.Now;
		[Column(TypeName = "nvarchar(30)")]
		public NotificationType Type { get; set; }
		[Column(TypeName = "nvarchar(30)")]
		public NotificationWeight Weight { get; set; }
		public int CreatorId { get; set; } = -1;// -1 means server generated
		public UserProfile Creator { get; set; }
		public bool IsServerNotification { get; set; } = true;
		public IList<Notification> Notifications { get; set; }
		//public IList<UserProfile> Receivers { get; set; }
	}
}