using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IEEE_Application.DATA.Models
{
    public class Performance
    {
        [Key]
        
        public string UserId { get; set; }
        [Key]
        
        public int TournamentId { get; set; }
        public int SolvedCount { get; set; }
        //navigation properties
        public User User { get; set; }
        public Tournament Tournament { get; set; }

    }
}
