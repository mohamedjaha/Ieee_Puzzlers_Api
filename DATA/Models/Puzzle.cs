using System.ComponentModel.DataAnnotations;

namespace IEEE_Application.DATA.Models
{
    public class Puzzle
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Byte[] Image { get; set; }
        [Required]
        public string Solution { get; set; }
        [Required]
        [RegularExpression("^(hard|medium|easy)$", ErrorMessage = "DifficultyLevel must be hard , medium or easy.")]
        public string DifficultyLevel { get; set; }
        [Required]
        public string CreatorId { get; set; }

        //navigation properity 
        public ICollection<Tournament_Puzzle> TournamentPuzzles { get; set; }
        public User Creator { get; set; }


    }
}
