using IEEE_Application.DATA.DTO;
using IEEE_Application.DATA.Models;
using IEEE_Application.Repository.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IEEE_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class PuzzelsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public PuzzelsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        [HttpGet("GetAllPuzzles")]
        public async Task<IActionResult> GetAllPuzzles()
        {
            var puzzles = await _unitOfWork.RepoPuzzle.GetAllAsync();
            if (puzzles.Count ==0)
            {
                return Ok("No puzzles found.");
            }
            return Ok(puzzles);
        }
        [Authorize]
        [HttpGet("GetPuzzleById")]
        public async Task<IActionResult> GetPuzzleById(int id)
        {
            var puzzle = await _unitOfWork.RepoPuzzle.GetByIdAsync(id);
            if (puzzle == null)
            {
                return NotFound("Puzzle not found.");
            }
            return Ok(puzzle);
        }
        [Authorize(Roles = "PUZZLE_CREATOR")]
        [HttpPost("CreatePuzzle")]
        public async Task<IActionResult> CreatePuzzle(PuzzleDTO puzzle)
        {
            if (ModelState.IsValid && puzzle != null)
            {
                if (await _unitOfWork.RepoPuzzle.GetByNameAsync(puzzle.Name) != null)
                {
                    return BadRequest("Puzzle with the same name already exists.");
                }
                using var stream = new MemoryStream();
                await puzzle.Image.CopyToAsync(stream);
                var newPuzzle = new Puzzle()
                {
                    Name = puzzle.Name,
                    DifficultyLevel = puzzle.DifficultyLevel,
                    Solution = puzzle.Solution,
                    Image = stream.ToArray()
                };
                _unitOfWork.RepoPuzzle.CreateAsync(newPuzzle);
                return Ok("Puzzle created successfully.");
            }
            return BadRequest(ModelState);
        }
    }
}
