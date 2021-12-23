using Cleverence.Server.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleverence.Server
{
	public class StartUpHelper
	{
		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(configure => configure.AddConsole())
				.AddTransient<ServerCore>();
		}
	}
}
