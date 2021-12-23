using Cleverence.Server;
using Cleverence.Server.Core;

using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
StartUpHelper.ConfigureServices(serviceCollection);

var buildServiceProvider = serviceCollection.BuildServiceProvider();

var Server = buildServiceProvider.GetService<ServerCore>();

Console.ReadLine();

// Назначаем сокет локальной конечной точке и слушаем входящие сокеты
//try
//{
//	sListener.Bind(ipEndPoint);
//	sListener.Listen(10);

//	// Начинаем слушать соединения
//	while (true)
//	{
//		Console.WriteLine("Ожидаем соединение через порт {0}", ipEndPoint);

//		// Программа приостанавливается, ожидая входящее соединение
//		Socket handler = sListener.BeginAccept(new AsyncCallback())
//		string data = null;

//		// Мы дождались клиента, пытающегося с нами соединиться

//		byte[] bytes = new byte[1024];
//		int bytesRec = handler.Receive(bytes);

//		data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

//		// Показываем данные на консоли
//		Console.Write("Полученный текст: " + data + "\n\n");

//		// Отправляем ответ клиенту\
//		string reply = "Спасибо за запрос в " + data.Length.ToString()
//				+ " символов";
//		byte[] msg = Encoding.UTF8.GetBytes(reply);
//		handler.Send(msg);

//		if (data.IndexOf("<TheEnd>") > -1)
//		{
//			Console.WriteLine("Сервер завершил соединение с клиентом.");
//			break;
//		}

//		handler.Shutdown(SocketShutdown.Both);
//		handler.Close();
//	}
//}
//catch (Exception ex)
//{
//	Console.WriteLine(ex.ToString());
//}
//finally
//{
//	Console.ReadLine();
//}