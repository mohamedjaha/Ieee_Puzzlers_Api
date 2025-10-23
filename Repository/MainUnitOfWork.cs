using IEEE_Application.DATA;
using IEEE_Application.DATA.Models;
using IEEE_Application.Repository.Base;
using Microsoft.Extensions.Caching.Memory;

namespace IEEE_Application.Repository
{
    public class MainUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _Dbcontext;
        private readonly IMemoryCache _memoryCache;
        public ISpecialPuzzelRepository RepoPuzzle { get; set; }
        public IRepo<Tournament> RepoTournament { get; set; }
        public IRepo<User> RepoUser { get; set; }
        public MainUnitOfWork(AppDbContext Dbcontext , IMemoryCache memoryCache)
        {
            _Dbcontext = Dbcontext;
            _memoryCache = memoryCache;
            RepoPuzzle = new SpecialPuzzelRepository(_Dbcontext, _memoryCache);
            RepoTournament = new MainRepo<Tournament>(_Dbcontext);
            RepoUser = new MainRepo<User>(_Dbcontext);
        }

        
    }
}
