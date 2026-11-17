namespace UniMove.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
	IEnumerable<T> ObterTodos();
	T? ObterPorId(int id);
	int Adicionar(T entidade);
	void Atualizar(T entidade);
	void Remover(int id);
}