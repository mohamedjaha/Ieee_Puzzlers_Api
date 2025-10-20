using System.ComponentModel.DataAnnotations;

namespace IEEE_Application.DATA.DTO
{
    public class PuzzleDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        [Required]
        public string Solution { get; set; }
        [Required]
        [RegularExpression("^(hard|medium|easy)$", ErrorMessage = "DifficultyLevel must be hard , medium or easy.")]
        public string DifficultyLevel { get; set; }
        [Required]
        public string CreatorId { get; set; }
    }
}
