namespace IEEE_Application.Repository.Base
{
    public interface IRepo<T> where T : class
    {
        Task CreateAsync (T entity);
        Task<T?> GetByIdAsync (int id);
        Task<T?> GetByNameAsync(string Name);
        Task<List<T>> GetAllAsync ();
        Task UpdateAsync (T entity);
        Task DeleteAsync (T entity);


    }
}
