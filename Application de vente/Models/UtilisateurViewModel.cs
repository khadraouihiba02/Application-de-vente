using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class UserWithRoleViewModel
    {
        public string Id { get; set; }
        public string NomComplet { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Actif { get; set; }
    }

    public class CreerUtilisateurViewModel
    {
        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Format d'adresse email invalide.")]
        [Display(Name = "Adresse Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        [StringLength(100, ErrorMessage = "Le {0} doit faire au moins {2} caractères.", MinimumLength = 6)]
        public string MotDePasse { get; set; }

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [Display(Name = "Rôle")]
        public string Role { get; set; }
    }

    public class ModifierUtilisateurViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Format d'adresse email invalide.")]
        [Display(Name = "Adresse Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe (laisser vide pour ne pas changer)")]
        [StringLength(100, ErrorMessage = "Le {0} doit faire au moins {2} caractères.", MinimumLength = 6)]
        public string MotDePasse { get; set; }

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [Display(Name = "Rôle")]
        public string Role { get; set; }

        [Display(Name = "Compte actif")]
        public bool Actif { get; set; }
    }
}
