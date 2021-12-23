using Cleverence.Entities;
using Cleverence.Entities.Entities;
using Cleverence.Entities.Entities.Enums;
using Cleverence.Test.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;

namespace Cleverence.Test.Core
{
	public class ClientCore
	{
		private Role _randomRole;
		private ILogger _logger;
		private BlockingCollection<Message> _blockCollection;

		private static ManualResetEvent connectDone = new ManualResetEvent(false);
		private static ManualResetEvent sendDone = new ManualResetEvent(false);
		private static ManualResetEvent receiveDone = new ManualResetEvent(false);

		public ClientCore(ILogger<ClientCore> logger, int role)
		{
			_logger = logger;
			_blockCollection = new BlockingCollection<Message>();
			_randomRole = (role % 2 == 0) ? Role.Sender : Role.Reciever;//Randomize Clients Type

			_ = SetUpConnectionAsync(); //Setup Config + port 
			_ = ConnectAsync();

			//Определяем что будет делать клиент либо писать и считывать либо просто считывать
			var message = new Message()
			{
				Id = Thread.GetCurrentProcessorId(),
				Content = $"{Settings.TestMessage} id:{Thread.GetCurrentProcessorId()}",
				Role = _randomRole
			};
			if (_randomRole == Role.Sender)
			{
				_ = Send(Socket, message); //AddToCount() by task ПИСАТЬ(Отправлять Данные)
				Thread.Sleep(1000);
				_ = Recive(); //GetCount() by task ЧИТАТЬ(Принимать Данные)
			}
			else
			{
				_ = Recive(); //GetCount() by task ЧИТАТЬ(Принимать Данные)
			}

			_logger.LogInformation($"[{Now}] [{nameof(ReceiveCallback)}] STOP CLIENT ACTION!");
		}

		private string Now => $"ID:{Thread.GetCurrentProcessorId(),2:0#}| {DateTime.Now.ToString("dd.M.yyyy HH:mm:ss")} {_randomRole}";
		public HostConfig Settings { get; set; }
		private Socket Socket { get; set; }
		private ClientResponse<Message> ClientResponse { get; set; }


		//-------------------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------------------
		public async Task<Response> SetUpConnectionAsync()
		{
			//MethodBase.GetCurrentMethod().Name not working instead of nameof(methodName)
			_logger.LogInformation($"[{Now}] [{nameof(SetUpConnectionAsync)}] Start! ");
			try
			{
				//Setup Config
				var configuration = new ConfigurationBuilder()
					.SetBasePath(AppContext.BaseDirectory)
					.AddJsonFile("appsettings.json", true, true).Build();
				Settings = configuration.GetSection(nameof(HostConfig)).Get<HostConfig>();
				Settings.Role = _randomRole;//Randomize Clients Type

				if (Settings == null)
				{
					_logger.LogError($"[{Now}] [{nameof(SetUpConnectionAsync)}] Error!");
					return await new Task<Response>(() => new()
					{
						Status = Status.Error,
						Message = "Error: Nullable object settings"
					});
				}

				// Create a TCP/IP socket.
				Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				Thread.Sleep(1000);
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
		private async Task<Response> ConnectAsync()
		{
			try
			{
				_logger.LogInformation($"[{Now}] [{nameof(ConnectAsync)}] Start!");

				Socket.BeginConnect(
					new IPEndPoint(IPAddress.Parse(Settings.IpAddress), Settings.Port),
					ConnectCallback, Socket);
				_logger.LogInformation($"[{Now}] [{nameof(ConnectAsync)}] Waiting for a connection...");

				connectDone.WaitOne();
				_logger.LogInformation($"[{Now}] [{nameof(ConnectAsync)}] Connection complete!");

				Thread.Sleep(1000);
				return await new Task<Response>(() => new()
				{
					Status = Status.Succes,
				});
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(ConnectAsync)}] Error!");
				return await new Task<Response>(() => new()
				{
					Status = Status.Error,
					Errors = new List<Exception>().Append(ex),
					Message = ex.Message,
				});
			}
			finally
			{
				_logger.LogInformation($"[{Now}] [{nameof(ConnectAsync)}] End!");
			}
		}
		private void ConnectCallback(IAsyncResult asyncResult)
		{
			try
			{
				_logger.LogInformation($"[{Now}] [{nameof(ConnectCallback)}] Start!");
				Socket client;
				// Retrieve the socket from the state object.
				if (asyncResult != null)
				{
					_logger.LogInformation($"[{Now}] [{nameof(ConnectCallback)}] Connecting");
					client = asyncResult.AsyncState as Socket;
				}
				else
				{
					connectDone.Close();
					return;
				}

				// Complete the connection.
				client.EndConnect(asyncResult);
				_logger.LogInformation($"[{Now}] [{nameof(ConnectCallback)}] Socket connected to {client.RemoteEndPoint}");

				Thread.Sleep(1000);
				// Signal that the connection has been made.
				connectDone.Set();
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(ConnectCallback)}] Connect Error! {ex.Message}");
				throw;
			}
			_logger.LogInformation($"[{Now}] [{nameof(ConnectCallback)}] End!");
		}


		//-------------------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------------------
		private async Task<Response> Send(Socket client, Message data)
		{
			try
			{
				_logger.LogInformation($"[{Now}] [{nameof(Send)}] Start!");

				string json = JsonConvert.SerializeObject(data);
				byte[] byteData = Encoding.UTF8.GetBytes(json.ToCharArray());

				// Begin sending the data to the remote device.
				client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
				sendDone.WaitOne();
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(Send)}] Error! {ex.Message}");
				return await new Task<Response>(() => new()
				{
					Status = Status.Error,
					Errors = new List<Exception>().Append(ex),
					Message = ex.Message,
				});
			}

			return await new Task<Response>(() => new());
		}
		private void SendCallback(IAsyncResult asyncResult)
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket client = asyncResult.AsyncState as Socket;

				// Complete sending the data to the remote device.
				int bytesSent = client.EndSend(asyncResult);
				_logger.LogInformation($"[{Now}] [{nameof(SendCallback)}] Sent {bytesSent} bytes to server.!");

				// Signal that all ok.
				sendDone.Set();
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(SendCallback)}] Error! {ex.Message}");
				throw;
			}
			finally
			{
				_logger.LogInformation($"[{Now}] [{nameof(SendCallback)}] End!");
			}
		}


		//-------------------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------------------
		private async Task<Response> Recive()
		{
			try
			{
				_logger.LogInformation($"[{Now}] [{nameof(Recive)}] Start!");

				ClientResponse = new ClientResponse<Message>() { WorkSocket = Socket };

				// Begin receiving the data from the remote device.
				Socket.BeginReceive(ClientResponse.Buffer, 0, ClientResponse.BufferSize, 0,
									ReceiveCallback, ClientResponse);
				receiveDone.WaitOne();

				return await new Task<Response>(() => new() { Status = Status.Succes });
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(Recive)}] Recive Error! {ex.Message}");
				throw;
			}
		}
		private void ReceiveCallback(IAsyncResult asyncResult)
		{
			try
			{
				_logger.LogInformation($"[{Now}] [{nameof(ReceiveCallback)}] Start!");
				// from the asynchronous state object.
				var response = asyncResult.AsyncState as ClientResponse<Message>;

				// Read data from the remote device.
				int bytesRead = Socket.EndReceive(asyncResult);

				var collection = JsonConvert.DeserializeObject<List<Message>>(Encoding.UTF8.GetString(response.Buffer));

				foreach (var item in collection)
				{
					if (_blockCollection.Contains(item) == false)
						_blockCollection.Add(item);
				}
				_blockCollection.CompleteAdding();

				//// Get the rest of the data.
				//Socket.BeginReceive(response.Buffer, 0, response.BufferSize, 0, ReceiveCallback, response);
				// All the data has arrived; put it in response.

				_logger.LogInformation(
					$"[{Now}] [{nameof(ReceiveCallback)}] " +
					$"Recive : {_blockCollection.ShowItems()}");

				// Signal that all ok.
				receiveDone.Set();
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{Now}] [{nameof(ReceiveCallback)}] Error! {ex.Message}");
				throw;
			}
			finally
			{
				_logger.LogInformation($"[{Now}] [{nameof(ReceiveCallback)}] End!");
			}
		}
	}
}
