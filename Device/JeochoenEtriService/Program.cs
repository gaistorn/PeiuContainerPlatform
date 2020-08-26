using Microsoft.Extensions.Hosting;
namespace PeiuPlatform.Hubbub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => ServiceInitializer.Configuration(args, hostContext.Configuration, services));
    }
}
