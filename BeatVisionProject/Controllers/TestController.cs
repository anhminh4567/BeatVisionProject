using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Services.Implementation;
using Services.Interface;
using Shared.ConfigurationBinding;
using Shared.Poco;
using Shared.RequestDto;

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

		public TestController(IMyEmailService mailService, IUnitOfWork unitOfWork, AppsettingBinding appsettings, AudioFileServices audioFileService)
		{
			_mailService = mailService;
			_unitOfWork = unitOfWork;
			_appsettings = appsettings;
			_audioFileService = audioFileService;
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
	}
}
