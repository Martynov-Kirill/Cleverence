using Cleverence.Entities.Entities.Enums;
using Cleverence.Entities.Entities.Interfaces;

namespace Cleverence.Entities.Entities
{
	public class Response : IResponse
	{
		public long Id { get; set; }

		public string? Message { get; set; }

		public Status Status { get; set; }

		public IEnumerable<Exception>? Errors { get; set; }
	}

	public class Response<T> : Response where T : class
	{
		public T? Data { get; set; }
	}
}
