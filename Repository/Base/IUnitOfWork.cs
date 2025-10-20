using IEEE_Application.DATA.Models;

namespace IEEE_Application.Repository.Base
{
    public interface IUnitOfWork
    {
        ISpecialPuzzelRepository RepoPuzzle { get; set; }
        IRepo<Tournament> RepoTournament { get; set; }
        IRepo<User> RepoUser { get; set; }
    }
}
