
using System.Diagnostics;
using System.Reflection;

var clientsCount = 3;// КОЛ-ВО КЛИЕНТОВ

var location = Assembly.GetExecutingAssembly().Location;
var clientApp = string.Concat(Directory.GetParent(location).Parent.Parent.Parent.Parent.FullName, "\\Cleverence.Test\\bin\\Debug\\net6.0\\Cleverence.Test.exe");

Random rnd = new Random();
List<Process> list = new List<Process>();

for (int i = 0; i < clientsCount; i++)
{
	list.Add(new()
	{
		StartInfo = new ProcessStartInfo()
		{
			FileName = clientApp,
			Arguments = rnd.Next(1000).ToString(),
		}
	});
}

foreach (var item in list)
{
	Parallel.Invoke(() => item.Start());
}

Console.ReadLine();
