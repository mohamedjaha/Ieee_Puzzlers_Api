using IEEE_Application.DATA;
using IEEE_Application.DATA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IEEE_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentPuzzleController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TournamentPuzzleController(AppDbContext db)
        {
            _db = db;
        }
        [Authorize(Roles = "GAME_CREATOR")]
        [HttpPost("AddPuzzleToTournament")]
        public async Task<IActionResult> AddPuzzleToTournament(int tournamentId, int puzzleId)
        {
            var existingEntry = await _db.Tournament_Puzzles.FirstOrDefaultAsync(tp => tp.TournamentId == tournamentId && tp.PuzzleId == puzzleId);
            if (existingEntry != null)
            {
                return BadRequest("This puzzle is already added to the tournament.");
            }
            if (tournamentId == null || puzzleId == null)
            {
                return BadRequest("BadRequest");
            }
            if (await _db.Tournaments.FindAsync(tournamentId) == null)
            {
                return BadRequest("Tournament not found.");
            }
            if (await _db.Puzzles.FindAsync(puzzleId) == null)
            {
                return BadRequest("Tournament not found.");
            }
            var tournamentPuzzle = new Tournament_Puzzle{TournamentId = tournamentId,PuzzleId = puzzleId};
            _db.Tournament_Puzzles.Add(tournamentPuzzle);
            var tournament = await _db.Tournaments.FindAsync(tournamentId);
            tournament.PuzzelCount += 1;
            await _db.SaveChangesAsync();
            
            return Ok("Puzzle added to tournament successfully.");
        }
    }
}
