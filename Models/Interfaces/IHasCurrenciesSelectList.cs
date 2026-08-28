using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;

namespace VontaGenta.Models
{
    public interface IHasCurrenciesSelectLists
    {
        public List<Currency> CurrencyList { get; set; }
        SelectList Currencies { get; set; }
    }
}