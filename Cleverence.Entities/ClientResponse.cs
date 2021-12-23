using Cleverence.Entities.Entities;

using System.Net.Sockets;

namespace Cleverence.Entities
{
	public class ClientResponse<T> : Response<T> where T : class
	{
		// Client socket.
		public Socket WorkSocket { get; set; } = null;

		// Size of receive buffer.
		public int BufferSize => 512;

		// Receive buffer.
		public byte[] Buffer { get; set; } = new byte[512];
    }
}
