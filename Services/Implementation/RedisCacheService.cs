using Services.Interface;
using Shared.Helper;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class RedisCacheService : ICacheService
	{

		private readonly IConnectionMultiplexer _connectionMultiplexer;
		public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
		{
			_connectionMultiplexer = connectionMultiplexer;
		}

		public async Task<Result<T>> GetData<T>(string key, CancellationToken cancellationToken = default)
		{
			var db = _connectionMultiplexer.GetDatabase();
			var value = await db.StringGetAsync(key);
			if (string.IsNullOrEmpty(value) is false)
			{
				return Result<T>.Success(JsonSerializer.Deserialize<T>(value));
			}
			return Result<T>.Fail();
		}

		public Task<Result> RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public async Task<Result> RemoveData(string key, CancellationToken cancellationToken = default)
		{
			var db = _connectionMultiplexer.GetDatabase();
			var exist = db.KeyExists(key);
			if (exist)
			{
				var success =  await db.KeyDeleteAsync(key);
				if (success)
					return Result.Success();
			}
			return Result.Fail();
		}

		public async Task<Result> SetData<T>(string key, T value, CancellationToken cancellationToken = default)
		{
			var db= _connectionMultiplexer.GetDatabase();
			//var expiryTime = DateTime.Now.AddMinutes(RandomMinuteExpireTime());
			var jsonifyValue = JsonSerializer.Serialize(value);
			var isSet = await db.StringSetAsync(key,jsonifyValue,TimeSpan.FromMinutes(RandomMinuteExpireTime()));
			if(isSet) 
			{
				return Result.Success();
			}
			return Result.Fail() ;
		}
		private int RandomMinuteExpireTime()
		{
			var random = new Random();
			return random.Next(10,30);
		}
	}
}
