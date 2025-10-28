using IEEE_Application.DATA;
using IEEE_Application.DATA.Models;
using IEEE_Application.Repository.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IEEE_Application.Repository
{
    public class SpecialPuzzelRepository : MainRepo<Puzzle>, ISpecialPuzzelRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly string CacheKeyAll = "AllPuzzles";
        public SpecialPuzzelRepository(AppDbContext Dbcontext , IMemoryCache memoryCache) : base(Dbcontext)
        {
            _dbContext = Dbcontext;
            _memoryCache = memoryCache;
        }

        public async Task<List<Puzzle>> GetPuzzelsByDifficultyLevel(string DifficultyLevel)
        {
            if (DifficultyLevel == "hard" || DifficultyLevel == "medium" || DifficultyLevel == "easy")
            {
                var cacheKey = $"Puzzels{DifficultyLevel}";

                if (!_memoryCache.TryGetValue(cacheKey, out List<Puzzle> PuzzleByDifficultyLevel))
                {
                    PuzzleByDifficultyLevel = await _dbContext.Puzzles
                        .Where(p => p.DifficultyLevel == DifficultyLevel)
                        .ToListAsync();

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(450))   
                        .SetAbsoluteExpiration(TimeSpan.FromHours(6))
                        .SetPriority(CacheItemPriority.High);

                    _memoryCache.Set(cacheKey, PuzzleByDifficultyLevel, cacheEntryOptions);
                }

                return PuzzleByDifficultyLevel;
            }

            return null;
        }

        public async Task<List<Puzzle>> GetPuzzelsByCreatorId(string CreatorId)
        {
            return await _dbContext.Puzzles.Where(p => p.CreatorId == CreatorId).ToListAsync();
        }

        public async Task<List<Puzzle>> GetAllAsync()
        {
            
            if (!_memoryCache.TryGetValue(CacheKeyAll, out List<Puzzle> allPuzzles))
            {
                allPuzzles = await _dbContext.Puzzles.ToListAsync();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(450))   
                    .SetAbsoluteExpiration(TimeSpan.FromHours(6))
                    .SetPriority(CacheItemPriority.High);
                _memoryCache.Set(CacheKeyAll, allPuzzles, cacheEntryOptions);
            }
            return allPuzzles;
        }
        public void clearPuzzelCache( string DifficultyLevel )
        {
            if ( DifficultyLevel == "hard" || DifficultyLevel == "medium" || DifficultyLevel == "easy" )
            {
                var cacheKey = $"Puzzels{DifficultyLevel}";
                _memoryCache.Remove(cacheKey);
                _memoryCache.Remove(CacheKeyAll)
            }
        }
    }
}
