using Cleverence.Test;
using Cleverence.Test.Core;

using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;

var serviceCollection = new ServiceCollection();

if(args.Length > 0) //Randomize Client Role by recived argument
	StartUpHelper.ConfigureServices(serviceCollection, int.Parse(args[0]));
else
	StartUpHelper.ConfigureServices(serviceCollection);

var buildServiceProvider = serviceCollection.BuildServiceProvider();

buildServiceProvider.GetService<ClientCore>();

Console.ReadLine();