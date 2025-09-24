using System.ComponentModel.DataAnnotations;

namespace IEEE_Application.DATA.DTO
{
    public class TournamentDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public int PuzzelCount { get; set; }
    }
}
