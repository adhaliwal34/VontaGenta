using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public static class RevenuesMappings
    {
        public static void MapTo(this Revenue source, RevenueDetailsViewModel dest)
        {
            dest.ID = source.ID;
            dest.Description = source.Description;
            dest.Amount = source.Amount;
            dest.EntryDate = source.EntryDate;
            dest.ProjectName = source.Project?.Name;
            dest.CurrencySymbol = source.Currency?.Symbol;
        }

        public static void MapTo(this Revenue source, RevenueFormViewModel dest)
        {
            dest.Description = source.Description;
            dest.Amount = source.Amount;
            dest.CurrencyID = source.CurrencyID;
            dest.EntryDate = source.EntryDate;
            dest.ProjectID = source.ProjectID;
        }

        public static void MapTo(this RevenueFormViewModel source, Revenue dest)
        {
            dest.Description = source.Description;
            dest.Amount = source.Amount;
            dest.CurrencyID = source.CurrencyID;
            dest.EntryDate = source.EntryDate;
            dest.ProjectID = source.ProjectID;
        }
    }
}
