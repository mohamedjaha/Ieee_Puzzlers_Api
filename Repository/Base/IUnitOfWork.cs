using IEEE_Application.DATA.Models;

namespace IEEE_Application.Repository.Base
{
    public interface IUnitOfWork
    {
        IRepo<Puzzle> RepoPuzzle { get; set; }
        IRepo<Tournament> RepoTournament { get; set; }
    }
}
