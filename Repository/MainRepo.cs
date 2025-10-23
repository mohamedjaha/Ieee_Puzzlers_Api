using IEEE_Application.DATA;
using IEEE_Application.DATA.Models;
using IEEE_Application.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace IEEE_Application.Repository
{
    public class MainRepo<T> : IRepo<T> where T : class
    {
        private readonly AppDbContext _Dbcontext;
        public MainRepo(AppDbContext Dbcontext)
        {
            _Dbcontext = Dbcontext;
        }
        public async Task CreateAsync(T entity)
        {
            if (entity != null)
            { 
            await _Dbcontext.Set<T>().AddAsync(entity);
            await _Dbcontext.SaveChangesAsync();
            }
            return;
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity != null)
            {
                _Dbcontext.Set<T>().Remove(entity);
                await _Dbcontext.SaveChangesAsync();
            }
        }

        public async Task<List<T>> GetAllAsync()
        {
            return (await _Dbcontext.Set<T>().ToListAsync());
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            if (id != null)
            {
                return (await _Dbcontext.Set<T>().FindAsync(id));
            }
            return null;
        }
        public async Task<T?> GetByIdAsync(string id)
        {
            if (id != null)
            {
                return (await _Dbcontext.Set<T>().FindAsync(id));
            }
            return null;
        }

        public  async Task<T?> GetByNameAsync(string Name)
        {
            if (Name != null)
            {
                return await _Dbcontext.Set<T>().FirstOrDefaultAsync(t => EF.Property<string>(t, "Name") == Name);
            }
            return null;
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity != null)
            {
                _Dbcontext.Set<T>().Update(entity);
                await _Dbcontext.SaveChangesAsync();
            }
            return;
        }
    }
}
