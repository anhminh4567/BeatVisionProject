using Shared.Models;

namespace Shared.ResponseDto
{
	public class NotificationDto
	{
		public int? ReceiverId { get; set; }
		public int? MessageId { get; set; }
		public bool IsReaded { get; set; }
		public DateTime ExpiredDate { get; set; }
	}
}