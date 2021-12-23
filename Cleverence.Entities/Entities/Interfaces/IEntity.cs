namespace Cleverence.Entities.Entities.Interfaces
{
	public interface IEntity
	{
		public long Id { get; set; }
	}

	public interface IEntity<T> : IEntity
	{
		public new T Id { get; set; }
	}
}
