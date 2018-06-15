using NodaTime;

namespace Raidfelden.Services.Extensions
{
    public static class NodaTimeExtensions
    {
        public static Offset FromTimeZoneToOffset(this Instant instant, string timeZone)
        {
            DateTimeZone zone = DateTimeZoneProviders.Tzdb[timeZone];
            return zone.GetUtcOffset(instant);
        }
    }
}
