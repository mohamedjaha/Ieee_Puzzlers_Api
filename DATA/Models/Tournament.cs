using System.ComponentModel.DataAnnotations;

namespace IEEE_Application.DATA.Models
{
    public class Tournament
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } //the name need to be unique
        [Required]
        public string Password { get; set; }
        [Required]
        public int PuzzelCount { get; set; }

        //navigation propority 
        public ICollection<Tournament_Puzzle> TournamentPuzzles { get; set; }
        public ICollection<Performance> Performances { get; set; }

    }
}
