using Microsoft.Extensions.Caching.Distributed;
using Shared.Helper;
using Shared.IdentityConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Repository.Implementation.CachingImplementation
{
	public class CachingRepositoryBase<T> : RepositoryBase<T> where T : class
	{
		internal IDistributedCache _cacheDb;
		internal readonly string _cacheKey = $"{typeof(T)}";
		public CachingRepositoryBase(ApplicationDbContext context , IDistributedCache distributedCache) : base(context)
		{
			_cacheDb = distributedCache;
		}

		public override async Task<T?> GetByIdInclude(object id, string includeProperties = "")
		{
			//return base.GetByIdInclude(id, includeProperties);
			var idAsInteger = (int)id;
			var cacheKey = _cacheKey + idAsInteger + includeProperties;
			var value = await _cacheDb.GetAsync(cacheKey);
			if (value is not null)
			{
				var deserializedValue =  JsonSerializer.Deserialize<T>(value);
				_dbcontext.Attach<T>(deserializedValue);
				return deserializedValue;
			}
			else
			{
				var getResult = await base.GetByIdInclude(id, includeProperties);
				if (getResult is not null)
					_cacheDb.Set(cacheKey, JsonSerializer.SerializeToUtf8Bytes(getResult), DistributeCacheOptionFactory.CreateRandomTime());
				return getResult;
			}
		}

		public override async Task<T?> GetById(object id)
		{
			var idAsInteger = (int)id;
			var cacheKey = _cacheKey + idAsInteger;
			var value = await _cacheDb.GetAsync(cacheKey);
			if (value is not null) 
			{
				var deserializedValue = JsonSerializer.Deserialize<T>(value);
				_dbcontext.Attach<T>(deserializedValue);
				return deserializedValue;
			}
				//return JsonSerializer.Deserialize<T>(value);
			else
			{
				var getResult = await base.GetById(id);
				if (getResult is not null)
					_cacheDb.Set(cacheKey, JsonSerializer.SerializeToUtf8Bytes(getResult), DistributeCacheOptionFactory.CreateRandomTime());
				return getResult;
			}
		}

	}
}
