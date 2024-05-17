using Quartz;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BackgroundServices
{
	public class QuartzDemoServices : IJob
	{
		private readonly IUnitOfWork unitOfWork;

		public QuartzDemoServices(IUnitOfWork unitOfWork)
		{
			this.unitOfWork = unitOfWork;
		}

		public Task Execute(IJobExecutionContext context)
		{
			//Console.WriteLine("	call from background " );
			return Task.CompletedTask;
		}
	}
}
