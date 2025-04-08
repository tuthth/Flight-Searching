using FlightSearching_Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FlightSearching_Web.Pages
{
    public class DashboardModel : PageModel
    {
        public FlightTicketSearchContext context { get; set; }
        public string RequestSize { get; set; }
        public string FlightSize { get; set; }
        public string AirlineSize { get; set; }
        public string AirportSize { get; set; }
        public List<Request> RequestList { get; set; }
        public void OnGet()
        {
            context = new FlightTicketSearchContext();
            RequestSize = context.Requests.Count().ToString();
            FlightSize = context.Flights.Count().ToString();
            AirlineSize = context.Airlines.Count().ToString();
            AirportSize = context.Airports.Count().ToString();

            RequestList = context.Requests.ToList();
        }
        [HttpGet]
        public async Task<IActionResult> GenerateCSVFlights()
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(300);
                string url = "https://localhost:44396/Flights/DownloadCSV";
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                if (responseMessage.IsSuccessStatusCode)
                {
                    using (Stream contentStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        byte[] csvBytes = new byte[contentStream.Length];
                        contentStream.Read(csvBytes, 0, (int)contentStream.Length);
                        string fileName = "flights.csv";
                        string mediaType = "text/csv";
                        return File(csvBytes, mediaType, fileName);
                    }
                }
                else return RedirectToPage("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GenerateCSVAirlines()
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(300);
                string url = "https://localhost:44396/Airlines/DownloadCSV";
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                if (responseMessage.IsSuccessStatusCode)
                {
                    using (Stream contentStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        byte[] csvBytes = new byte[contentStream.Length];
                        contentStream.Read(csvBytes, 0, (int)contentStream.Length);
                        string fileName = "airlines.csv";
                        string mediaType = "text/csv";
                        return File(csvBytes, mediaType, fileName);
                    }
                }
                else return RedirectToPage("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GenerateCSVAirports()
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(300);
                string url = "https://localhost:44396/Airports/DownloadCSV";
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                if (responseMessage.IsSuccessStatusCode)
                {
                    using (Stream contentStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        byte[] csvBytes = new byte[contentStream.Length];
                        contentStream.Read(csvBytes, 0, (int)contentStream.Length);
                        string fileName = "airports.csv";
                        string mediaType = "text/csv";
                        return File(csvBytes, mediaType, fileName);
                    }
                }
                else return RedirectToPage("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GenerateCSVRequests()
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(300);
                string url = "https://localhost:44396/Requests/DownloadCSV";
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                if (responseMessage.IsSuccessStatusCode)
                {
                    using (Stream contentStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        byte[] csvBytes = new byte[contentStream.Length];
                        contentStream.Read(csvBytes, 0, (int)contentStream.Length);
                        string fileName = "requests.csv";
                        string mediaType = "text/csv";
                        return File(csvBytes, mediaType, fileName);
                    }
                }
                else return RedirectToPage("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> OnPost(string downloadType)
        {
            switch (downloadType)
            {
                case "GenerateCSVRequests":
                    return await GenerateCSVRequests();
                case "GenerateCSVFlights":
                    return await GenerateCSVFlights();
                case "GenerateCSVAirports":
                    return await GenerateCSVAirports();
                case "GenerateCSVAirlines":
                    return await GenerateCSVAirlines();
                default:
                    return RedirectToPage("Error");
            }
        }
    }
}
