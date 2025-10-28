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
        private const string CacheKeyAll = "AllPuzzles";
        private static readonly string[] DifficultyCacheKeys = new[] { "hard", "medium", "easy" };

        public SpecialPuzzelRepository(AppDbContext Dbcontext , IMemoryCache memoryCache) : base(Dbcontext)
        {
            _dbContext = Dbcontext;
            _memoryCache = memoryCache;
        }

        async Task IRepo<Puzzle>.CreateAsync(Puzzle entity)
        {
            await base.CreateAsync(entity);
            InvalidateCache();
        }

        async Task IRepo<Puzzle>.UpdateAsync(Puzzle entity)
        {
            await base.UpdateAsync(entity);
            InvalidateCache();
        }

        async Task IRepo<Puzzle>.DeleteAsync(Puzzle entity)
        {
            await base.DeleteAsync(entity);
            InvalidateCache();
        }

        public void clearPuzzelCache(string difficultyLevel)
        {
            InvalidateCache();
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
                        .SetSlidingExpiration(TimeSpan.FromSeconds(45))   
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1))
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
                    .SetSlidingExpiration(TimeSpan.FromSeconds(45))   
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetPriority(CacheItemPriority.High);
                _memoryCache.Set(CacheKeyAll, allPuzzles, cacheEntryOptions);
            }
            return allPuzzles;
        }
        private void InvalidateCache()
        {
            _memoryCache.Remove(CacheKeyAll);
            foreach (var key in DifficultyCacheKeys)
            {
                _memoryCache.Remove($"Puzzels{key}");
            }
        }
    }
}
