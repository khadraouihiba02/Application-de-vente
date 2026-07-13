using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class TauxChange
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Devise Source")]
        public string DeviseSource { get; set; } = "EUR";

        [Required]
        [StringLength(3)]
        [Display(Name = "Devise Cible")]
        public string DeviseCible { get; set; } = "TND";

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Taux de Change")]
        public decimal Taux { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; }
    }
}
