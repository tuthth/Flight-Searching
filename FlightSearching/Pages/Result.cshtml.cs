using FlightSearching_Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FlightSearching.Pages
{
    public class ResultModel : PageModel
    {
        public List<Flight> flights { get; set; }
        public void OnGet()
        {
            string loadData = HttpContext.Session.GetString("loadData");
            flights = JsonConvert.DeserializeObject<List<Flight>>(loadData);
            HttpContext.Session.Remove("loadData");
        }
    }
}
