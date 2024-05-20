using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RedisDistLock;

static class Program
{
    static async Task Main()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var lockService = serviceProvider.GetRequiredService<RedisLockService>();

        // Test the lock mechanism
        await TestLockingMechanism(lockService);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        var redisConnection = new RedisConnection(configuration);
        services.AddSingleton(redisConnection.GetConnection());

        services.AddTransient<RedisLockService>();
    }

    private static async Task TestLockingMechanism(RedisLockService lockService)
    {
        var resource = "testResource";
        var expiryTime = TimeSpan.FromSeconds(5);

        // Task 1: Acquire lock and hold it
        var task1 = Task.Run(async () =>
        {
            Thread.CurrentThread.Name = "Task1";
            var result = await lockService.AcquireLockAsync(resource, expiryTime);
            Console.WriteLine(result ? "Task 1: Lock acquired and held." : "Task 1: Failed to acquire lock.");
        });

        // Delay to ensure Task 1 acquires the lock first
        await Task.Delay(100);

        // Task 2: Attempt to acquire lock while Task 1 is holding it
        var task2 = Task.Run(async () =>
        {
            Thread.CurrentThread.Name = "Task2";
            var result = await lockService.AcquireLockAsync(resource, expiryTime);
            Console.WriteLine(result ? "Task 2: Lock acquired and held." : "Task 2: Failed to acquire lock.");
        });

        await Task.WhenAll(task1, task2);

        // Give some time for the console output to appear
        await Task.Delay(1000);
    }
}