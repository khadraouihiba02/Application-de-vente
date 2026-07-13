using Microsoft.AspNetCore.Identity;

namespace ApplicationDeVente.Models
{
    public class Utilisateur : IdentityUser
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public bool Actif { get; set; } = true;

        public string NomComplet => $"{Prenom} {Nom}";
    }
}

