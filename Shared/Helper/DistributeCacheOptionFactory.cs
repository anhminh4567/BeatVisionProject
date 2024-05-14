using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helper
{
	public static class DistributeCacheOptionFactory
	{
		public static readonly int ExpiredTimeMinute_Max = 15;
		public static readonly int ExpiredTimeMinute_Min = 10;
		public static readonly int ExpiredSlidingMinute = 3;
		public static Random Random = new Random();
		public static DistributedCacheEntryOptions CreateRandomTime()
		{
			var randomExpiredTime = Random.Next(ExpiredTimeMinute_Min,ExpiredTimeMinute_Max);
			return new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(randomExpiredTime),
				SlidingExpiration = TimeSpan.FromMinutes(ExpiredSlidingMinute),
			};
		}

	}
}
