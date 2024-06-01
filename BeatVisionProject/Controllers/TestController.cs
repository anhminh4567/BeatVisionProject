using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Services.Implementation;
using Services.Interface;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.Poco;
using Shared.RequestDto;
using Shared.ResponseDto;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TestController : ControllerBase
	{
		private readonly IMyEmailService _mailService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly AppsettingBinding _appsettings;
		private readonly AudioFileServices _audioFileService;
		private readonly IMapper _mapper;
		public TestController(IMyEmailService mailService, IUnitOfWork unitOfWork, AppsettingBinding appsettings, AudioFileServices audioFileService, IMapper mapper)
		{
			_mailService = mailService;
			_unitOfWork = unitOfWork;
			_appsettings = appsettings;
			_audioFileService = audioFileService;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<ActionResult> SendmailWithAttachment()
		{
			var getTrack = await _unitOfWork.Repositories.trackRepository.GetById(8);
			var getMp3File = await _audioFileService.DownloadPrivateMp3Audio(8);
			var getWavFile = await _audioFileService.DownloadPrivateWavAudio(8);
			using var mp3Stream = getMp3File.Value.Stream;
			using var wavStream = getWavFile.Value.Stream;
			var fileList = new List<EmailAttachments>();
			fileList.Add(new EmailAttachments()
			{
				FileStream = mp3Stream,
				ContentType = getMp3File.Value.ContentType,
				FileName = "test.mp3"
			}) ;
			fileList.Add(new EmailAttachments()
			{
				FileStream = wavStream,
				ContentType = getWavFile.Value.ContentType,
				FileName = "test.wav"
			});
			var testMail = new EmailMetaData();
			testMail.Subject = "test mail";
			testMail.Attachments = fileList;
			testMail.ToEmail = "testingwebandstuff@gmail.com";
			var mailModel = new DownloadTrackEmailModel();
			mailModel.Username = "test";
			mailModel.TrackName = getTrack.TrackName;
			mailModel.LicenseName = "no license";
			mailModel.Timesend = DateTime.Now;
			mailModel.ReceiverEmail = "testingwebandstuff@gmail.com";
			var sendResult = await _mailService.SendEmailWithTemplate_WithAttachment(testMail, _appsettings.MailTemplateAbsolutePath.FirstOrDefault(p => p.TemplateName == "DownloadTrackEmail").TemplateAbsolutePath,mailModel);

			return Ok();
		}
		[HttpPost("validate-with-regrex")]
		public async Task<ActionResult> TestValidatoin([FromForm]CreateTrackDto createTrackDto)
		{
			return Ok();
		}
		[HttpPost("send-noti-mail")]
		public async Task<ActionResult> TestSendEmail()
		{
			var meta = new EmailMetaData()
			{
				ToEmail = "testingwebandstuff@gmail.com",
				Subject = "Test email",
			};
			var notiModel = new NotificationEmailModel()
			{
				Content = "This is the test email, dont share or do a damn thing",
				NotificationType = NotificationType.GROUP.ToString(),
				SendTime = DateTime.Now,
				Title = "Test email",
				TrackToPublish = null,
				UserToSend = _mapper.Map<UserProfileDto>( await _unitOfWork.Repositories.userProfileRepository.GetById(11) ),
				Weight = NotificationWeight.MINOR.ToString(),
				LogoImgBase64 = "logo"//ImageFile.ConvertImageToBase64String( (_appsettings.DefaultContentRelativePath.FirstOrDefault(c => c.ContentName == "DefaultLogo")).ContentPathWWWRoot)
			};
			await _mailService.SendEmailWithTemplate(meta, (_appsettings.MailTemplateAbsolutePath.FirstOrDefault(t => t.TemplateName == "NotificationEmail")).TemplateAbsolutePath ,notiModel);


			//await _mailService.SendEmailWithTemplate(meta, (_appsettings.MailTemplateAbsolutePath.FirstOrDefault(t => t.TemplateName == "ConfirmEmailTemplate")).TemplateAbsolutePath, notiModel);

			return Ok();
		}
	}
}
