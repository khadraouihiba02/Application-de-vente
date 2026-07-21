using ApplicationDeVente.Models;

namespace ApplicationDeVente.Models.ViewModels
{
    public class ControleAgentFRSViewModel
    {
        public List<EtatDesVentes> EtatsVentes { get; set; } = new();
        public Dictionary<int, EtatDesVentesFRS> EtatsFRS { get; set; } = new();
    }
}
