using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisDistLock;

public class RedisConnection
{
    private readonly ConnectionMultiplexer _redis;

    public RedisConnection(IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Value;
        _redis = ConnectionMultiplexer.Connect(redisConnectionString!);
    }

    public ConnectionMultiplexer GetConnection()
    {
        return _redis;
    }
}