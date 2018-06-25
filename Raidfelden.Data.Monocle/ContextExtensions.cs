using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Raidfelden.Data.Monocle.Entities;

namespace Raidfelden.Data.Monocle
{
    public partial class Hydro74000Context
    {
        public Task<Spawnpoints> GetNearestSpawnpointAsync(double latitude, double longitude)
        {
            return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync<Spawnpoints>(Spawnpoints.FromSql($@"
SELECT * FROM spawnpoints
 WHERE id IN 
 (
  SELECT id FROM
  (
   SELECT id, despawn_time, IFNULL(duration, 30), lat, lon, 111.045 * DEGREES(ACOS(COS(RADIANS({latitude}))
   * COS(RADIANS(lat))
   * COS(RADIANS(lon) - RADIANS({longitude}))
   + SIN(RADIANS({latitude}))
   * SIN(RADIANS(lat)))) 
   AS distance_in_km
   FROM spawnpoints
   ORDER BY distance_in_km ASC
  ) AS temp WHERE distance_in_km < 0.1
 )
 LIMIT 0,1"));
        }

        public Task<List<Spawnpoints>> GetNearestSpawnpointsAsync(double latitude, double longitude)
        {
            return EntityFrameworkQueryableExtensions.ToListAsync<Spawnpoints>(Spawnpoints.FromSql($@"
SELECT s.* FROM spawnpoints AS s INNER JOIN (
SELECT id, (
	6371 * 1000 * 
	acos(cos(radians({latitude})) *
	cos(radians(lat)) *
	cos(radians(lon) - radians({longitude})) +
	sin(radians({latitude})) *
	sin(radians(lat))) 
) AS distance_in_m
FROM spawnpoints
HAVING distance_in_m < 100
ORDER BY distance_in_m  ASC
) AS f on s.id = f.id
"));
        }
    }
}
