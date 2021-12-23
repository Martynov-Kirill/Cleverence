using Cleverence.Entities.Entities.Enums;
using Cleverence.Entities.Entities.Interfaces;

namespace Cleverence.Entities
{
	public class HostConfig
	{
		public string IpAddress { get; set; }

		public int Port { get; set; }

		public int Backlog { get; set; }

		public Role Role { get; set; }

		public string From { get; set; }

		public string TestMessage { get; set; }

	}
}
