using System.ComponentModel.DataAnnotations;

namespace ApplicationDeVente.Models.ViewModels
{
    public class CommissionViewModel
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string NomMois { get; set; } = string.Empty;

        public List<ItemCommission> Commissions { get; set; } = new List<ItemCommission>();
        public decimal TotalCommission { get; set; }
    }

    public class ItemCommission
    {
        public int PNCId { get; set; }
        public string Matricule { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public decimal TotalEncaisse { get; set; }
        public decimal CommissionCalculee { get; set; }
    }
}
