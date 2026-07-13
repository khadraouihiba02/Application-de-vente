using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Code Article")]
        public string CodeArticle { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nom de l'Article")]
        public string NomArticle { get; set; }

        public string Description { get; set; }

        // Navigation property
        public ICollection<PrixArticleContrat> PrixContrats { get; set; }
    }
}
