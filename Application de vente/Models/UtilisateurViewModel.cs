using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class UserWithRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Actif { get; set; }
    }

    public class CreerUtilisateurViewModel
    {
        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Format d'adresse email invalide.")]
        [Display(Name = "Adresse Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        [StringLength(100, ErrorMessage = "Le {0} doit faire au moins {2} caractères.", MinimumLength = 6)]
        public string MotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = string.Empty;
    }

    public class ModifierUtilisateurViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Format d'adresse email invalide.")]
        [Display(Name = "Adresse Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe (laisser vide pour ne pas changer)")]
        [StringLength(100, ErrorMessage = "Le {0} doit faire au moins {2} caractères.", MinimumLength = 6)]
        public string? MotDePasse { get; set; }

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Compte actif")]
        public bool Actif { get; set; }
    }
}
