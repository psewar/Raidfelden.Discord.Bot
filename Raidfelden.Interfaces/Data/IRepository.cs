namespace Raidfelden.Interfaces.Data
{
	public interface IRepository
	{
		void Submit();
	}

	public interface IRepository<T> : IRepository where T : class { }
}
