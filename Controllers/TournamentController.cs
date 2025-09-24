using IEEE_Application.DATA;
using IEEE_Application.DATA.DTO;
using IEEE_Application.DATA.Models;
using IEEE_Application.Repository.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IEEE_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _db;
        public TournamentController(IUnitOfWork unitOfWork , AppDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        [HttpGet("GetAllTournaments")]
        public async Task<IActionResult> GetAllTournaments()
        {
            var tournaments = await _unitOfWork.RepoTournament.GetAllAsync();
            return Ok(tournaments);
        }
        [HttpGet("GetTournamentById/{id}")]
        public async Task<IActionResult> GetTournamentById(int id)
        {
            var tournament = _unitOfWork.RepoTournament.GetByIdAsync(id);
            var PuzzelsIds = await _db.Tournament_Puzzles.Where(pt => pt.TournamentId == id).Select(pt => pt.PuzzleId).ToListAsync();
            if (tournament == null)
            {
                return NotFound("Tournament not found.");
            }
            if (PuzzelsIds.Count() == 0)
            {
                PuzzelsIds = [];
            }
            return Ok(new { Tournament = tournament, Puzzels = PuzzelsIds });
        }
        [Authorize(Roles = "GAME_CREATOR")]
        [HttpPost("CreateTournament")]
        public async Task<IActionResult> CreateTournament(TournamentDTO tournament)
        {
            if (ModelState.IsValid && tournament != null)
            {
                if (await _unitOfWork.RepoTournament.GetByNameAsync(tournament.Name) != null)
                {
                    return BadRequest("Tournament with the same name already exists.");
                }
                var newTournament = new Tournament()
                {
                    Name = tournament.Name,
                    Password = tournament.Password,
                    PuzzelCount = 0
                };
                await _unitOfWork.RepoTournament.CreateAsync(newTournament);
                return Ok("Tournament created successfully.");
            }
            return BadRequest(ModelState);
        }


    }
}
