using FlightSearching_Library.Interface;
using FlightSearching_Library.Models;
using FlightSearching_Library.Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FlightSearching_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AirportsController : Controller
    {
        private readonly FlightTicketSearchContext _context;
        private readonly ILogger<AirlinesController> _logger;
        private readonly IFlightSerachingRepo repo = new FlightSearchingRepo();

        public AirportsController(FlightTicketSearchContext context)
        {
            _context = context;
        }

        [HttpGet("/GetAirport", Name = "GetAirport")]
        public IEnumerable<Airport> GetAirports(string? keyword)
        {
            if (keyword.Length > 0)
            {
                if (keyword.Length == 3) return _context.Airports.Where(a => a.AirportCode.Contains(keyword));
                else return _context.Airports.Where(a => a.AirportName.Contains(keyword) || a.AirportCode.Contains(keyword));
            }
            else
            {
                return _context.Airports.ToList();
            }
        }
        [HttpGet("/GetAirportFromCountry", Name = "GetAirportByCountry")]
        public IEnumerable<Airport> GetAirportsFromACountry(string country)
        {
            return _context.Airports.Where(a => a.Country.Contains(country)).ToList();
        }
        [HttpGet("/LoadAirportForMainPage", Name = "LoadAirport")]
        public List<string> GetAirports()
        {
            List<string> loadData = new List<string>();
            IEnumerable<Airport> airportData = _context.Airports.OrderByDescending(a => a.Country.Contains("Viet Nam")).ThenBy(a => a.Country);
            foreach (Airport airport in airportData)
            {
                string data = String.Format("{0} ({1}) ({2})", airport.AirportName.Trim(), airport.AirportCode.Trim(), airport.Country.Trim());
                loadData.Add(data);
            }
            return loadData;
        }
        [HttpPut("/AddAirport", Name = "AddAirport")]
        public string AddAirport(string keyword)
        {
            string notice = "";
            bool checkExist = repo.checkAirportExist(keyword);
            if (checkExist == false)
            {
                repo.addNewAirport(keyword);
                notice = "Add airport sucess";
            }
            else
            {
                notice = "This airport has been added before";
            }
            return notice;
        }
        [HttpGet("DownloadCSV/")]
        public FileResult DownloadAirportsCSV()
        {
            var list = _context.Airports.ToList();
            var CSVContent = new StringBuilder();
            CSVContent.AppendLine("Airport code, Airport name, City, Country");
            foreach (var airport in list)
            {
                CSVContent.AppendLine(String.Format("{0}, {1}, {2}, {3}", airport.AirportCode.Trim(), airport.AirportName.Trim(), airport.City.Trim(), airport.Country.Trim()));
            }
            var CSVBytes = Encoding.UTF8.GetBytes(CSVContent.ToString());
            return File(CSVBytes, "text/csv", "airports.csv");
        }
    }
}
