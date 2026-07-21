using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class PNC
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La date du vol est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Day of Origin")]
        public DateTime Day_of_origin { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Le numéro de vol est obligatoire.")]
        [StringLength(20)]
        public string FlightNumber { get; set; } = string.Empty;

        [StringLength(10)]
        public string departure { get; set; } = string.Empty;

        [StringLength(10)]
        public string destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code TLC est obligatoire.")]
        [StringLength(20)]
        public string TLC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100)]
        public string name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [StringLength(100)]
        public string First_name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Rank { get; set; } = string.Empty;

        // Propriété calculée pour l'affichage dans les listes
        public string NomComplet => $"{TLC} - {name} {First_name} ({Rank})";
    }
}
