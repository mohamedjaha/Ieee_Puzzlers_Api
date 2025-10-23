using Microsoft.AspNetCore.Identity;

namespace IEEE_Application.DATA.Models
{
    public class User : IdentityUser
    {
        //navigation properity 
        public ICollection<Performance> Performances { get; set; }
        public ICollection<Puzzle> CreatedPuzzles { get; set; }
    }
}
