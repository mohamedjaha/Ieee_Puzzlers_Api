using IEEE_Application.DATA.Models;

namespace IEEE_Application.Repository.Base
{
    public interface ISpecialPuzzelRepository : IRepo<Puzzle>
    {
        Task<List<Puzzle>> GetPuzzelsByDifficultyLevel (string DifficultyLevel);
        Task<List<Puzzle>> GetPuzzelsByCreatorId (string CreatorId);
    }
}
