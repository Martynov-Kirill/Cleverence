using Cleverence.Entities.Entities.Enums;

namespace Cleverence.Entities.Entities.Interfaces
{
	public interface IResponse : IEntity
	{
		public string Message { get; set; }

		public Status Status { get; set; }

		public IEnumerable<Exception> Errors { get; set; }
	}

	public interface IResponse<T> : IResponse
	{
		public Task<IEnumerable<T>> Data { get; set; }
	}
}
