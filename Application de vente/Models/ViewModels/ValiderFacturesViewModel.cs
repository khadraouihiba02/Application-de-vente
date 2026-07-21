namespace ApplicationDeVente.Models.ViewModels
{
    public class ValiderFacturesViewModel
    {
        public List<Facture> FacturesEnAttente { get; set; } = new List<Facture>();
        public List<Facture> HistoriqueFactures { get; set; } = new List<Facture>();
    }
}
