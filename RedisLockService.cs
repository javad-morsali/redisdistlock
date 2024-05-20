using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace RedisDistLock;

public class RedisLockService
{
    private readonly RedLockFactory _redLockFactory;

    public RedisLockService(ConnectionMultiplexer redis)
    {
        var multiplexers = new List<RedLockMultiplexer>
        {
            redis
        };

        _redLockFactory = RedLockFactory.Create(multiplexers);
    }

    public async Task<bool> AcquireLockAsync(string resource, TimeSpan expiryTime)
    {
        var lockKey = $"lock:{resource}";

        await using var redLock = await _redLockFactory.CreateLockAsync(lockKey, expiryTime);
        var threadName = Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString();

        if (redLock.IsAcquired)
        {
            // Lock acquired
            Console.WriteLine($"{DateTime.Now}: Lock acquired for {resource} :::: thread Name : {threadName}");
            await Task.Delay(expiryTime); // Simulate some work
            return true;
        }
        else
        {
            // Failed to acquire lock
            Console.WriteLine($"{DateTime.Now}: Failed to acquire lock for {resource}:::: thread Name : {threadName}");
            return false;
        }
    }
}