using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raidfelden.Discord.Bot.Monocle
{
    public partial class Hydro74000Context
    {
        public Task<Spawnpoints> GetNearestSpawnpointAsync(double latitude, double longitude)
        {
            return Spawnpoints.FromSql($@"
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
 LIMIT 0,1").FirstOrDefaultAsync();
        }

        public Task<List<Spawnpoints>> GetNearestSpawnpointsAsync(double latitude, double longitude)
        {
            return Spawnpoints.FromSql($@"
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
").ToListAsync();
        }
    }
}
