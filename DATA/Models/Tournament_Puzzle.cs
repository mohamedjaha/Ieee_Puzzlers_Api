using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IEEE_Application.DATA.Models
{
    public class Tournament_Puzzle
    {
        [Key]
        
        public int TournamentId { get; set; }
        [Key]
        
        public int PuzzleId { get; set; }
        //navigation properties

        public Puzzle Puzzle { get; set; }

        public Tournament Tournament { get; set; }
    }
}
