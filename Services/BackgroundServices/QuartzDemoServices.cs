﻿using Quartz;
using Repository.Interface;
using Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BackgroundServices
{
	public class QuartzDemoServices : IJob
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly TrackManager _trackManager;

		public QuartzDemoServices(IUnitOfWork unitOfWork, TrackManager trackManager)
		{
			_unitOfWork = unitOfWork;
			_trackManager = trackManager;
		}

		public Task Execute(IJobExecutionContext context)
		{
			//Console.WriteLine("	call from background " );
			
			return Task.CompletedTask;
		}
	}
}
