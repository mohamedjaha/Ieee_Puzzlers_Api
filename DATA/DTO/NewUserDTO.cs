using System.ComponentModel.DataAnnotations;

namespace IEEE_Application.DATA.DTO
{
    public class NewUserDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [RegularExpression("^(PUZZLE_CREATOR|GAME_CREATOR|GAMER|ADMIN)$", ErrorMessage = "Role must be either PUZZLE_CREATOR , GAME_CREATOR , GAMER or ADMIN.")]
        public string Role { get; set; }
    }
}
