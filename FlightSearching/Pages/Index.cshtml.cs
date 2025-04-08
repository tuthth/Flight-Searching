using FlightSearching_API.Controllers;
using FlightSearching_Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace FlightSearching.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IEnumerable<string> Locations { get; set; }
        public FlightTicketSearchContext db { get; set; } = new FlightTicketSearchContext();


        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            using(HttpClient httpClient = new HttpClient())
            {
                string url = "https://localhost:44396/LoadAirportForMainPage";

                HttpResponseMessage responseMessage = await httpClient.GetAsync(url);
                if(responseMessage.IsSuccessStatusCode)
                {
                    string loadData = await responseMessage.Content.ReadAsStringAsync();
                    Locations = JsonConvert.DeserializeObject<List<string>>(loadData);
                    
                }
                else
                {
                    ///Chua biet lam gi hihi
                }
            }
        }
        public async Task<IActionResult> OnPost()
        {
            int requestId = db.Requests.Count();
            string departureLocation = Request.Form["from"];
            string arrivalLocation = Request.Form["to"];
            DateTime? departureDate = DateTime.Parse(Request.Form["depart"]);
            DateTime? returnDate = null;
            try
            {
                returnDate = DateTime.Parse(Request.Form["return"]);
            }
            catch
            {
                returnDate = new DateTime();
            }
            int? numberOfAdults = Int32.Parse(Request.Form["adults"]);
            int? numberOfChildren = Int32.Parse(Request.Form["childs"]);
            int? numberOfInfants = Int32.Parse(Request.Form["infants"]);
            string? contactEmail = Request.Form["email"];

            Request request = new Request(requestId, GetAirportCodeFromForm(departureLocation), GetAirportCodeFromForm(arrivalLocation), departureDate, returnDate, numberOfAdults, numberOfChildren, numberOfInfants, contactEmail);
            db.Requests.Add(request);
            db.SaveChanges();
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(300);
                string url = "https://localhost:44396/GetFlights";

                var requestData = new
                {
                    RequestId = requestId,
                    DepartureLocation = GetAirportCodeFromForm(departureLocation),
                    ArrivalLocation = GetAirportCodeFromForm(arrivalLocation),
                    DepartureDate = departureDate,
                    ReturnDate = returnDate,
                    NumberOfAdults = numberOfAdults,
                    NumberOfChildren = numberOfChildren,
                    NumberOfInfants = numberOfInfants,
                    ContactEmail = contactEmail
                };

                var jsonRequestData = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");

                HttpResponseMessage responseMessage = await httpClient.PostAsync(url, content);
                if (responseMessage.IsSuccessStatusCode)
                {
                    string loadData = await responseMessage.Content.ReadAsStringAsync();
                    HttpContext.Session.SetString("loadData", loadData);
                    return RedirectToPage("/Result");
                }
                else
                {
                    Console.WriteLine(responseMessage);
                    return RedirectToPage("/Error");
                }
            }
        }
        public string GetAirportCodeFromForm(string airport)
        {
            Regex regex = new Regex(@"\(([^)]*)\)");
            Match match = regex.Match(airport);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
        
    }
}