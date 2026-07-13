using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code article est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "Code Article")]
        public string CodeArticle { get; set; }

        [Required(ErrorMessage = "Le nom de l'article est obligatoire.")]
        [StringLength(100)]
        [Display(Name = "Nom de l'Article")]
        public string NomArticle { get; set; }

        [StringLength(250)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Le prix unitaire est obligatoire.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Prix Unitaire (EUR)")]
        [Range(0.01, 99999.99, ErrorMessage = "Le prix doit être supérieur à 0.")]
        public decimal PrixUnitaire { get; set; }

        [Required(ErrorMessage = "La date de début est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Valable du")]
        public DateTime DateDebut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Valable jusqu'au")]
        public DateTime DateFin { get; set; }
    }
}
