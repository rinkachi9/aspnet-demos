using StackExchange.Redis;

namespace AdvancedDb.Redis.Services;

public class GeoService
{
    private readonly IDatabase _db;
    private const string DriversKey = "drivers:locations";

    public GeoService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task AddDriverLocationAsync(string driverId, double lat, double lon)
    {
        await _db.GeoAddAsync(DriversKey, new GeoEntry(lon, lat, driverId));
    }

    public async Task<GeoRadiusResult[]> FindDriversNearbyAsync(double lat, double lon, double radiusKm)
    {
        // GEORADIUS or GEOSEARCH
        // Returns drivers within X km
        return await _db.GeoRadiusAsync(DriversKey, lon, lat, radiusKm, GeoUnit.Kilometers);
    }
}
