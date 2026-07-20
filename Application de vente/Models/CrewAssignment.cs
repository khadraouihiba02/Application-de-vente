using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class CrewAssignment
    {
        [Key]
        public int Id { get; set; }

        public int VolId { get; set; }
        [ForeignKey("VolId")]
        public Vol? Vol { get; set; }

        public int PNCId { get; set; }
        [ForeignKey("PNCId")]
        public PNC? PNC { get; set; }

        [StringLength(50)]
        public string Rank { get; set; } = string.Empty;
    }
}
