using System.Collections.Generic;

namespace ApplicationDeVente.Models.ViewModels
{
    public class AgentDashboardViewModel
    {
        public int VolsSaisisCeMois { get; set; } = 0;
        public int EtatsEnAttente { get; set; } = 0;
        public int EtatsValides { get; set; } = 0;
        public decimal CommissionEstimee { get; set; } = 0;
        public List<EtatDesVentes> DerniersEtats { get; set; } = new();
        public List<EtatDesOffres> DerniersEtatsOffres { get; set; } = new();
        public List<EtatDesVentesFRS> DerniersEtatsFRS { get; set; } = new();
        public List<EtatDesOffresFRS> DerniersEtatsOffresFRS { get; set; } = new();
    }
}
