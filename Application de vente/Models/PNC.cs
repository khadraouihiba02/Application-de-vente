using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class PNC
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le matricule est obligatoire.")]
        [StringLength(20)]
        [Display(Name = "Matricule PNC")]
        public string Matricule { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [StringLength(100)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        // Propriété calculée pour l'affichage dans les listes déroulantes
        public string NomComplet => $"{Matricule} - {Nom} {Prenom}";
    }
}
