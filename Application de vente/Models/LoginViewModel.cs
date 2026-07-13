using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format email invalide")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [DataType(DataType.Password)]
        [Display(Name = "MotDePasse")]
        public string MotDePasse { get; set; } = string.Empty;

        [Display(Name = "Se souvenir de moi")]
        public bool SeSouvenir { get; set; }
    }
}
