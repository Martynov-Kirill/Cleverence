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

//static void SendMessageFromSocket(int port)
//{
//	// Буфер для входящих данных
//	byte[] bytes = new byte[1024];

//	// Соединяемся с удаленным устройством

//	// Устанавливаем удаленную точку для сокета
//	IPHostEntry ipHost = Dns.GetHostEntry("127.0.0.1");
//	IPAddress ipAddr = ipHost.AddressList[0];
//	IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

//	Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

//	// Соединяем сокет с удаленной точкой
//	sender.Connect(ipEndPoint);

//	Console.Write("Введите сообщение: ");
//	string message = Console.ReadLine();

//	Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
//	byte[] msg = Encoding.UTF8.GetBytes(message);

//	// Отправляем данные через сокет
//	int bytesSent = sender.Send(msg);

//	// Получаем ответ от сервера
//	int bytesRec = sender.Receive(bytes);

//	Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));

//	// Используем рекурсию для неоднократного вызова SendMessageFromSocket()
//	if (message.IndexOf("<TheEnd>") == -1)
//		SendMessageFromSocket(port);

//	// Освобождаем сокет
//	sender.Shutdown(SocketShutdown.Both);
//	sender.Close();
//}