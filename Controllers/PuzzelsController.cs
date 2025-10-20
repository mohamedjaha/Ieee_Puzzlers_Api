using IEEE_Application.DATA;
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

        public PuzzelsController(IUnitOfWork unitOfWork , AppDbContext dbContext)
        {
            _unitOfWork = unitOfWork;

        }
        [AllowAnonymous]
        [HttpGet("GetAllPuzzles")]
        public async Task<IActionResult> GetAllPuzzles()
        {
            var puzzles = await _unitOfWork.RepoPuzzle.GetAllAsync();
            if (puzzles.Count == 0)
            {
                return Ok("No puzzles found.");
            }
            return Ok(puzzles);
        }
        [AllowAnonymous]
        [HttpGet("GetPuzzlesByDifficultyLevel")]
        public async Task<IActionResult> GetPuzzlesByDifficultyLevel(string level)
        {
            var puzzles = await _unitOfWork.RepoPuzzle.GetPuzzelsByDifficultyLevel(level.ToLower());
            if (puzzles == null)
            {
                return BadRequest("Invalid difficulty level. Please use 'easy', 'medium', or 'hard'.");
            }
            if (puzzles.Count == 0)
            {
                return Ok("No puzzles found for the specified difficulty level.");
            }
            return Ok(puzzles);
        }

        [AllowAnonymous]
        [HttpGet("GetPuzzleById/{id}")]
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
                // Check for duplicate puzzle name
                if (await _unitOfWork.RepoPuzzle.GetByNameAsync(puzzle.Name) != null)
                {
                    return BadRequest("Puzzle with the same name already exists.");
                }

                // Verify user 
                User creator = await _unitOfWork.RepoUser.GetByIdAsync(puzzle.CreatorId);
                if (creator == null)
                {
                    return BadRequest("Invalid creator ID ");
                }


                using var stream = new MemoryStream();
                await puzzle.Image.CopyToAsync(stream);
                var newPuzzle = new Puzzle()
                {
                    Name = puzzle.Name,
                    DifficultyLevel = puzzle.DifficultyLevel,
                    Solution = puzzle.Solution,
                    Image = stream.ToArray(),
                    CreatorId = puzzle.CreatorId
                };
                _unitOfWork.RepoPuzzle.CreateAsync(newPuzzle);
                return Ok("Puzzle created successfully.");
            }
            return BadRequest(ModelState);
        }
        [Authorize(Roles = "PUZZLE_CREATOR")]
        [HttpDelete("DeletePuzzle/{id}")]
        public async Task<IActionResult> DeletePuzzle(int id)
        {
            if (id == null || id<=0 )
            {
                return BadRequest("Invalid puzzle ID.");
            }
            var puzzle = await _unitOfWork.RepoPuzzle.GetByIdAsync(id);
            if (puzzle == null)
            {
                return NotFound("Puzzle not found.");
            }
            await _unitOfWork.RepoPuzzle.DeleteAsync(puzzle);
            return Ok("Puzzle deleted successfully.");
        }
        [Authorize(Roles = "PUZZLE_CREATOR")]
        [HttpGet("GetPuzzelsByCreatorId/{CreatorId}")]
        public async Task<IActionResult> GetPuzzelsByCreatorId(string CreatorId)
        {
            
            var puzzles = await _unitOfWork.RepoPuzzle.GetPuzzelsByCreatorId(CreatorId);

            if (puzzles.Count == 0)
            {
                return Ok("No puzzles found for the specified creator ID.");
            }
            return Ok(puzzles);
        }
        [Authorize(Roles = "PUZZLE_CREATOR")]
        [HttpPut("UpdatePuzzle")]
        public async Task<IActionResult> UpdatePuzzle(int id , PuzzleDTO puzzle)
        {
            if (id == null || id <= 0 || puzzle == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid input data.");
            }
            var existingPuzzle = await _unitOfWork.RepoPuzzle.GetByIdAsync(id);
            if (existingPuzzle == null)
            {
                return NotFound("Puzzle not found.");
            }
            // Check for duplicate puzzle name
            var puzzleWithSameName = await _unitOfWork.RepoPuzzle.GetByNameAsync(puzzle.Name);
            if (puzzleWithSameName != null && puzzleWithSameName.Id != id)
            {
                return BadRequest("Another puzzle with the same name already exists.");
            }
            // Verify user 
            User creator = await _unitOfWork.RepoUser.GetByIdAsync(puzzle.CreatorId);
            if (creator == null)
            {
                return BadRequest("Invalid creator ID ");
            }
            using var stream = new MemoryStream();
            await puzzle.Image.CopyToAsync(stream);
            existingPuzzle.Name = puzzle.Name;
            existingPuzzle.DifficultyLevel = puzzle.DifficultyLevel;
            existingPuzzle.Solution = puzzle.Solution;
            existingPuzzle.Image = stream.ToArray();
            existingPuzzle.CreatorId = puzzle.CreatorId;
            await _unitOfWork.RepoPuzzle.DeleteAsync(existingPuzzle); // Assuming DeleteAsync also updates the entity
            return Ok("Puzzle updated successfully.");
        }

    }
}
