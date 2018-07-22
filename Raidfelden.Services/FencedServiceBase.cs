using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Raidfelden.Configuration;
using Raidfelden.Data;
using Raidfelden.Entities;
using SimMetrics.Net.API;
using SimMetrics.Net.Metric;

namespace Raidfelden.Services
{
    public abstract class FencedEntityServiceBase<TEntity, TIdentifier> where TEntity : class, ILocation
	{
	    protected IGenericRepository<TEntity, TIdentifier> Repository { get; }
		protected LazyConcurrentDictionary<FenceConfiguration, TIdentifier[]> EntitiesByFences { get; set; }

		protected FencedEntityServiceBase(IGenericRepository<TEntity, TIdentifier> repository)
	    {
		    Repository = repository;
		    SimilarityThreshold = 0.5;
	    }

		public double SimilarityThreshold { get; set; }

		protected abstract TIdentifier GetEntityId(TEntity entity);
		protected abstract string GetEntityName(TEntity entity);

		protected virtual AbstractStringMetric GetAlgorithm()
		{
			return new JaroWinkler();
		}

		public async Task<Dictionary<TEntity, double>> GetBySimilarNameAsync(string name, FenceConfiguration[] fences = null, int limit = int.MaxValue)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return new Dictionary<TEntity, double>();
			}
			var algorithm = GetAlgorithm();
			var entities = await GetEntitiesByFenceAsync(fences);
			var rankedList =
				entities.Select(e => new { Entity = e, Similarity = algorithm.GetSimilarity(TrimString(GetEntityName(e)), TrimString(name)) })
					    .OrderByDescending(e => e.Similarity)
					    .Where(e => e.Similarity > SimilarityThreshold)
					    .Take(limit);
			return await Task.FromResult(rankedList.ToDictionary(k => k.Entity, v => v.Similarity));
		}

		private async Task<List<TEntity>> GetEntitiesByFenceAsync(FenceConfiguration[] fences = null)
		{
			if (fences == null || !fences.Any()) return await Repository.GetAllAsync();
			var identifiers = new List<TIdentifier>();
			foreach (var fence in fences)
			{
				var ids = EntitiesByFences.GetOrAdd(fence, GetEntityIdentifiersForFence);
				identifiers.AddRange(ids);
			}
			identifiers = identifiers.Distinct().ToList();

			return await Repository.FindAllAsync(e => identifiers.Contains(GetEntityId(e)));
		}

		private TIdentifier[] GetEntityIdentifiersForFence(FenceConfiguration fence)
		{
			var result = new List<TIdentifier>();
			var allEntities = Repository.GetAll().ToList();
			foreach (var entity in allEntities)
			{
				var coordinate = new Coordinate(entity.Latitude, entity.Longitude);
				if (fence.Area.Contains(coordinate))
				{
					result.Add(GetEntityId(entity));
				}
			}
			return result.ToArray();
		}

		private static string TrimString(string value)
		{
			return string.IsNullOrEmpty(value) ? value : value.Trim();
		}
	}
}
