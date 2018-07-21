using System;
using System.Collections.Generic;
using System.Linq;
using Raidfelden.Entities;
using System.Threading.Tasks;
using Google.Common.Geometry;
using Raidfelden.Configuration;
using Raidfelden.Data;

namespace Raidfelden.Services
{
	public interface IPokestopService
	{
		Task<Dictionary<IPokestop, double>> GetBySimilarNameAsync(string name, FenceConfiguration[] fences = null, int limit = int.MaxValue);
		Task<ServiceResponse> ConvertToGymAsync(string name, string newName, FenceConfiguration[] fences = null);
	}

    public class PokestopService : FencedEntityServiceBase<IPokestop, int>, IPokestopService
	{
		protected IGymRepository GymRepository { get; }

		public PokestopService(IPokestopRepository pokestopRepository, IGymRepository gymRepository) : base(pokestopRepository)
		{
			GymRepository = gymRepository;
		}

	    protected override int GetEntityId(IPokestop entity)
	    {
		    return entity.Id;
	    }

	    protected override string GetEntityName(IPokestop entity)
	    {
		    return entity.Name;
	    }

		public async Task<ServiceResponse> ConvertToGymAsync(string name, string newName = null, FenceConfiguration[] fences = null)
		{
			var result = await GetBySimilarNameAsync(name, fences);
			switch (result.Count)
			{
				case 0:
					return new ServiceResponse(false, "No Pokestop found with that name.");
				case 1:
					return await ConvertPokestopToGym(result.First().Key, newName);
			}

			var first = result.First();
			if (first.Value == 1 && !result.Skip(1).Any(e => e.Value == 1))
			{
				// Only one exact match
				return await ConvertPokestopToGym(first.Key, newName);
			}

			//TODO: Resolve those with interactive
			throw new NotImplementedException();
		}

		private async Task<ServiceResponse> ConvertPokestopToGym(IPokestop pokestop, string newName)
		{
			var gym = GymRepository.CreateInstance();
			gym.Name = pokestop.Name;
			gym.ExternalId = pokestop.ExternalId;
			gym.PictureUrl = pokestop.Url;
			gym.Latitude = pokestop.Latitude;
			gym.Longitude = pokestop.Longitude;
			// Calculate WeatherCellId
			var latlng = S2LatLng.FromDegrees(pokestop.Latitude, pokestop.Longitude);
			var cellId = S2CellId.FromLatLng(latlng);
			var level10 = cellId.ParentForLevel(10);
			gym.WeatherCellId = level10.Id;
			//await GymRepository.AddAsync(gym);
			//await Repository.DeleteAsync(pokestop);
			return new ServiceResponse(true, "Pokestop converted to Gym. WeatherCellId: " + gym.WeatherCellId);
		}
	}
}
