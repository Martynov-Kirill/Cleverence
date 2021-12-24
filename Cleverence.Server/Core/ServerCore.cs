using Cleverence.Entities;
using Cleverence.Entities.Entities;
using Cleverence.Entities.Entities.Enums;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Collections;

namespace Cleverence.Server.Core
{
	public class ServerCore
	{
		private Socket _server;
		private ILogger _logger;
		private BlockingCollection<Message> _blockCollection;
		public static ManualResetEvent allDone = new(false);


		public ServerCore(ILogger<ServerCore> logger)
		{
			_logger = logger;
			_blockCollection = new BlockingCollection<Message>()
			{
				new() {Content = "Server data 1"},
				new() {Content = "Server data 2"},
			};

			//Response = new ClientResponse<Message>();
			_ = SetUpConnectionAsync(); //Setup Config +  port Listener

			//Точка входа на считывание данных из коллекции внутри ReciveCallback()
			_ = AcceptAsync(); //AddToCount() by task ПИСАТЬ
		}

		private string Now => $"ID:{Thread.GetCurrentProcessorId(),2:0#}| {DateTime.Now.ToString("dd.M.yyyy HH:mm:ss")}";

		public HostConfig Settings { get; set; }
		private Socket Socket { get; set; }
		//private ClientResponse<Message> Response { get; set; }

		public async Task<Response> SetUpConnectionAsync()
		{
			_logger.LogInformation($"[{Now}] [{nameof(SetUpConnectionAsync)}] Start!");
			try
			{
				//Setup Config
				var configuration = new ConfigurationBuilder()
					.SetBasePath(AppContext.BaseDirectory)
					.AddJsonFile("appsettings.json", true, true).Build();
				Settings = configuration.GetSection(nameof(HostConfig)).Get<HostConfig>();

				if (Settings == null)
				{
					_logger.LogError($"[{Now}] [{nameof(SetUpConnectionAsync)}] Error!");
					return await new Task<Response>(() => new()
					{
						Status = Status.Error,
						Message = "Error: Nullable object settings"
					});
				}

				// Setup port Listener
				Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				Socket.Bind(new IPEndPoint(IPAddress.Parse(Settings.IpAddress), Settings.Port));
				Socket.Listen(Settings.Backlog);

				return await new Task<Response>(() => new()
				{
					Status = Status.Succes,
					Message = "Connected complete!",
				});
			}
			catch (Exception ex)
			{
				return await new Task<Response>(() => new()
				{
					Errors = new List<Exception>().Append(ex),
					Message = ex.Message,
					Status = Status.Error,
				});
			}
			finally
			{
				_logger.LogInformation($"[{Now}] [{nameof(SetUpConnectionAsync)}] End!");
			}
		}

		//-------------------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------------------
		public async Task<Response> AcceptAsync()
		{
			try
			{
				while (true)
				{
					// Set the event to nonsignaled state.
					allDone.Reset();

					// Start an asynchronous socket to listen for connections.
					_logger.LogInformation($"[{Now}] [{nameof(AcceptAsync)}] Waiting for a connection...");

					//Socket.BeginAccept(AcceptCallback, Socket);
					var handler = await Socket.AcceptAsync();
					if (handler.Connected)
						// Wait until a connection is made before continuing.
						_logger.LogInformation($"[{Now}] [{nameof(AcceptAsync)}] Connection complete!");

					var response = new ClientResponse<Message>() { WorkSocket = handler };

					await handler.ReceiveAsync(response.Buffer, 0);

					response.Data = JsonConvert.DeserializeObject<Message>(
						Encoding.UTF8.GetString(response.Buffer));

					_logger.LogInformation($"[{Now}] [{nameof(AcceptAsync)}] " +
					                       $"Read {response.Data.Content.Length} bytes from socket. \n\t" +
					                       $"Data : {response.Data.Content}");

					if (response.Data.Role == Role.Sender)
					{
						while (_blockCollection.IsAddingCompleted == false)
						{
							_blockCollection.Add(response.Data); // Данные от клиента 
							_ = SendAsync(handler); // Отправляем обратно обновленную коллекцию
							_blockCollection.CompleteAdding();
						}
					}
					else
					{
						_ = SendAsync(Socket); //GetCount() by task ЧИТАТЬ(Отправлять)
					}

					allDone.WaitOne(); //Контроль потоков внутри самого сервера
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(AcceptAsync)}] Error!\n{ex.Message}");
				return await new Task<Response>(() => new()
				{
					Status = Status.Error,
					Errors = new List<Exception>().Append(ex),
					Message = ex.Message,
				});
			}
		}

		//-------------------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------------------
		public async Task<Response> SendAsync(Socket handler)
		{
			try
			{
				_logger.LogInformation($"[{Now}] [{nameof(SendAsync)}] Start");
	
				var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_blockCollection));

				// Begin sending the data to the remote device.
				//handler.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, handler);

				var reciveSocket = await handler.SendAsync(byteData, 0);

				allDone.Set();
				return await new Task<Response>(() => new() { Status = Status.Succes });
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(SendCallback)}] Error! {ex.Message}");
			}
			finally { _logger.LogInformation($"[{Now}] [{nameof(SendAsync)}] End!"); }

			return await new Task<Response>(() => new() { Status = Status.Succes });
		}

		private void SendCallback(IAsyncResult asyncResult)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket handler = asyncResult.AsyncState as Socket;

				// Complete sending the data to the remote device.
				int bytesSent = handler.EndSend(asyncResult);

				_logger.LogInformation($"[{Now}] [{nameof(SendCallback)}] " +
									   $"Sent {bytesSent} bytes to client");
				
				allDone.Set();
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(SendCallback)}] Error! {ex.Message}");
				throw;
			}
			finally { _logger.LogInformation($"[{Now}] [{nameof(SendCallback)}] End!"); }
		}
	}
}