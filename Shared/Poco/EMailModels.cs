using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Poco
{
    public class ConfirmEmailModel
    {
        [Required]
        public string ReceiverEmail { get; set; }
        [Required]
        public string CallbackUrl { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string ConfirmationToken { get; set; }
    }
    public class DownloadTrackEmailModel
    {
		[Required]
		public string ReceiverEmail { get; set; }
		[Required]
		public string Username { get; set; }
		[Required]
		public DateTime Timesend { get; set; }
		[Required]
		public string TrackName { get; set; }
		[Required]
		public string LicenseName { get; set; } 
    }
    public class NotificationEmailModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime SendTime { get; set; }
        public string NotificationType { get; set; }
        public string Weight { get; set; }
        public string? LogoImgBase64 { get; set; }
        public string? TrackImageBase64 { get; set; }

        public TrackResponseDto? TrackToPublish { get; set; }
        public UserProfileDto? UserToSend { get; set; }
    }
}
