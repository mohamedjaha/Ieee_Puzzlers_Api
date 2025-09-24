using IEEE_Application.DATA;
using IEEE_Application.DATA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IEEE_Application.Services
{
    public class UserService
    {
        private readonly AppDbContext _Dbcontext;
        public UserService(AppDbContext Dbcontext)
        {
            _Dbcontext = Dbcontext;
        }
        public async Task<Tournament> GetTournamentByName(string Name)
        {
            var tournament = _Dbcontext.Tournaments.FirstOrDefault(t => t.Name == Name);
            if (tournament == null)
            {
                return null;
            }
            return tournament;
        }

        public async Task Add_Performance(int tourId , string userId)
        {
            Performance Per = new Performance(){TournamentId = tourId,UserId = userId,SolvedCount = 0};
            _Dbcontext.Performances.Add(Per);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task<bool> CheckExistingPerformance(string UserId, string groupName)
        {
            var tournament = await _Dbcontext.Tournaments.FirstOrDefaultAsync(T=> T.Name==groupName); // the tournament name is unique 
            var Check = await _Dbcontext.Performances.Where(p => p.UserId == UserId && p.TournamentId == tournament.Id).ToListAsync();
            if (Check.Count == 0)
            {
                return false;
            }
            return true;
        }
        public async Task DeletePerformance(string UserId, string groupName)
        {
            var tournament = await _Dbcontext.Tournaments.FirstOrDefaultAsync(T => T.Name == groupName);
            var performance = await _Dbcontext.Performances.FirstOrDefaultAsync(t => t.UserId == UserId && t.TournamentId == tournament.Id);
            if (performance != null)
            {
                _Dbcontext.Performances.Remove(performance);
                await _Dbcontext.SaveChangesAsync();
            }
        }
        public async Task<bool> verifierSolution(int puzzleId , string answer)
        {
            var puzzle = await _Dbcontext.Puzzles.FirstOrDefaultAsync(p => p.Id == puzzleId);
            if (puzzle == null)
            {
                return false;
            }
            if (puzzle.Solution == answer)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> UpdateSolvedCount(string userId, string groupName) // in this function we update the solved count and check if the user has finished the tournament or not
        {
            var tournament = await _Dbcontext.Tournaments
                .FirstOrDefaultAsync(T => T.Name.ToLowerInvariant() == groupName.ToLowerInvariant());

            var performance = await _Dbcontext.Performances
                .FirstOrDefaultAsync(t => t.UserId == userId && t.TournamentId == tournament.Id);

            performance.SolvedCount += 1;
            await _Dbcontext.SaveChangesAsync();

            return tournament.PuzzelCount == performance.SolvedCount;
        }

    }
}
