using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class EtatDesOffresVol
    {
        [Key]
        public int Id { get; set; }

        public int EtatDesOffresId { get; set; }
        [ForeignKey("EtatDesOffresId")]
        public EtatDesOffres? EtatDesOffres { get; set; }

        public int VolId { get; set; }
        [ForeignKey("VolId")]
        public Vol? Vol { get; set; }
    }
}
