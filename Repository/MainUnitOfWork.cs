using IEEE_Application.DATA;
using IEEE_Application.DATA.Models;
using IEEE_Application.Repository.Base;

namespace IEEE_Application.Repository
{
    public class MainUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _Dbcontext;
        public IRepo<Puzzle> RepoPuzzle { get; set; }
        public IRepo<Tournament> RepoTournament { get; set; }
        public MainUnitOfWork(AppDbContext Dbcontext)
        {
            _Dbcontext = Dbcontext;
            RepoPuzzle = new MainRepo<Puzzle>(_Dbcontext);
            RepoTournament = new MainRepo<Tournament>(_Dbcontext);
        }

        
    }
}
