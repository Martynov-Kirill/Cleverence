using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cleverence.Entities.Entities.Enums;
using Cleverence.Entities.Entities.Interfaces;

namespace Cleverence.Entities
{
	public class Message : IEntity
	{
		public long Id { get; set; }

		public string Content { get; set; }	= string.Empty;

		public int Count { get; set; }	= 0;

		public Role Role { get; set; } = 0;
	}
}
