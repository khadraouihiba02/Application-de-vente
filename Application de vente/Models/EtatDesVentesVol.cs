using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationDeVente.Models
{
    public class EtatDesVentesVol
    {
        [Key]
        public int Id { get; set; }

        public int EtatDesVentesId { get; set; }
        [ForeignKey("EtatDesVentesId")]
        public EtatDesVentes? EtatDesVentes { get; set; }

        public int VolId { get; set; }
        [ForeignKey("VolId")]
        public Vol? Vol { get; set; }
    }
}
