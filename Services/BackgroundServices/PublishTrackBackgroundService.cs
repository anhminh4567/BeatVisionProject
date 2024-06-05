using Quartz;
using Repository.Interface;
using Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BackgroundServices
{
	public class PublishTrackBackgroundService : IJob
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly TrackManager _trackManager;

		public PublishTrackBackgroundService(IUnitOfWork unitOfWork, TrackManager trackManager)
		{
			_unitOfWork = unitOfWork;
			_trackManager = trackManager;
		}

		public async  Task Execute(IJobExecutionContext context)
		{
			Console.WriteLine("background publish is triggered");
			await _trackManager.PublishTrack();
			//Console.WriteLine("count: " + ThreadPool.ThreadCount);
			//Console.WriteLine("count current: " + Task.CurrentId);
		}
	}
}
