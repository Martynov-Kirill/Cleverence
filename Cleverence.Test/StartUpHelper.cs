using Cleverence.Test.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleverence.Test
{
	public class StartUpHelper
	{
		public static void ConfigureServices(IServiceCollection services,int rnd = 0)
		{
			var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger<ClientCore>();

			services.AddLogging(configure => configure.AddConsole())
				.AddTransient(x => new ClientCore(logger, rnd));
		}
	}
}
