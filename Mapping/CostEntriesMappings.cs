using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public static class CostEntriesMappings
    {
        public static CostEntryDetailsViewModel ToDetailsViewModel(this CostEntry source)
        {
            var dest = new CostEntryDetailsViewModel();
            source.MapTo(dest);
            return dest;
        }

        public static void MapTo(this CostEntry source, CostEntryDetailsViewModel dest)
        {
            dest.ID = source.ID;
            dest.EntryDate = source.EntryDate;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.Amount = source.Amount;
            dest.UserName = source.User?.Name;
            dest.CategoryName = source.Category?.Name;
            dest.ProjectName = source.Project?.Name;
            dest.CurrencySymbol = source.Currency?.Symbol;
        }

        public static void MapTo(this CostEntry source, CostEntryFormViewModel dest)
        {
            dest.EntryDate = source.EntryDate;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.CategoryID = source.CategoryID;
            dest.Amount = source.Amount;
            dest.CurrencyID = source.CurrencyID;
            dest.ProjectID = source.ProjectID;
        }

        public static void MapTo(this CostEntryFormViewModel source, CostEntry dest)
        {
            dest.EntryDate = source.EntryDate;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.CategoryID = source.CategoryID;
            dest.Amount = source.Amount;
            dest.CurrencyID = source.CurrencyID;
            dest.ProjectID = source.ProjectID;
        }

        // When editing a cost entry we want to keep the existing user. It should not be overwritten.
        // Since ProjectID might change don't copy project reference.
        public static void MapTo(this CostEntry source, CostEntry dest)
        {
            dest.ID = source.ID;
            dest.EntryDate = source.EntryDate;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.CategoryID = source.CategoryID;
            dest.Amount = source.Amount;
            dest.CurrencyID = source.CurrencyID;
            dest.ProjectID = source.ProjectID;
        }
    }
}
