using IEEE_Application.DATA.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IEEE_Application.DATA
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext( DbContextOptions<AppDbContext> options ) : base(options)
        {
            
        }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Puzzle> Puzzles { get; set; }
        public DbSet<Tournament_Puzzle> Tournament_Puzzles { get; set; }
        public DbSet<Performance> Performances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== Tournament_Puzzle ==========

            // Composite primary key
            modelBuilder.Entity<Tournament_Puzzle>()
                .HasKey(tp => new { tp.TournamentId, tp.PuzzleId });

            // Relationships
            modelBuilder.Entity<Tournament_Puzzle>()
                .HasOne(tp => tp.Tournament)
                .WithMany(t => t.TournamentPuzzles)
                .HasForeignKey(tp => tp.TournamentId);

            modelBuilder.Entity<Tournament_Puzzle>()
                .HasOne(tp => tp.Puzzle)
                .WithMany(p => p.TournamentPuzzles)
                .HasForeignKey(tp => tp.PuzzleId);

            // ========== Performance ==========

            // Composite primary key
            modelBuilder.Entity<Performance>()
                .HasKey(tp => new { tp.TournamentId, tp.UserId });

            // Relationships
            modelBuilder.Entity<Performance>()
                .HasOne(tp => tp.Tournament)
                .WithMany(t => t.Performances)
                .HasForeignKey(tp => tp.TournamentId);

            modelBuilder.Entity<Performance>()
                .HasOne(tp => tp.User)
                .WithMany(p => p.Performances)
                .HasForeignKey(tp => tp.UserId);
        }


    }
}
