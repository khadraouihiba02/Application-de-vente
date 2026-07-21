using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class EtatDesOffresFRS
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro d'état FRS est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "N° État FRS")]
        public string NumeroEtat { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de réception est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de Réception")]
        public DateTime DateReception { get; set; } = DateTime.Today;

        [StringLength(50)]
        [Display(Name = "Statut de Contrôle")]
        public string StatutControle { get; set; } = "En attente"; // Ex: "En attente", "Conforme", "Écart"

        [Required(ErrorMessage = "Veuillez sélectionner l'état des offres PNC associé.")]
        [Display(Name = "État des Offres PNC Associé")]
        public int EtatDesOffresId { get; set; }

        [ForeignKey("EtatDesOffresId")]
        public EtatDesOffres? EtatDesOffres { get; set; }

        // Relation One-to-Many avec les lignes d'offre FRS
        public ICollection<LigneOffreFRS> Lignes { get; set; } = new List<LigneOffreFRS>();
    }
}
