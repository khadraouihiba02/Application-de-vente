using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class Facture
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro de facture est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "N° Facture")]
        public string NumeroFacture { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date de la Facture")]
        public DateTime DateFacture { get; set; } = DateTime.Today;

        [Required]
        [StringLength(50)]
        [Display(Name = "Type de Facture")]
        public string Type { get; set; } = "Offres"; // "Offres", "Déficit Caisse", "Redevance"

        [Required]
        [Display(Name = "Montant (EUR)")]
        [DataType(DataType.Currency)]
        public decimal Montant { get; set; } = 0;

        [Required]
        [StringLength(50)]
        [Display(Name = "Statut")]
        public string Statut { get; set; } = "En attente"; // "En attente", "Approuvée", "Rejetée", "Payée"

        [Display(Name = "Commentaires")]
        public string? Commentaires { get; set; }
    }
}
