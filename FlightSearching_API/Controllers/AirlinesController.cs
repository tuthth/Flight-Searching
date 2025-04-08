using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FlightSearching_Library.Models;
using FlightSearching_Library.Interface;
using FlightSearching_Library.Repo;
using System.Diagnostics;
using System.Text;

namespace FlightSearching_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AirlinesController : Controller
    {
        private readonly FlightTicketSearchContext _context;
        private readonly ILogger<AirlinesController> _logger;
        private readonly IFlightSerachingRepo repo = new FlightSearchingRepo();

        public AirlinesController(FlightTicketSearchContext context)
        {
            _context = context;
        }
        [HttpGet("/GetAirlines", Name = "GetAllAirlines")]
        public IEnumerable<Airline> GetAirlines()
        {

            var airlineList = _context.Airlines.ToList();

            return airlineList.ToArray();
        }
        [HttpPut("/AddAirlines")]
        public string AddAirline(string keyword)
        {
            string notice = "";
            bool checkExist = repo.checkAirlineExist(keyword);
            if (checkExist == false)
            {
                repo.addNewAirline(keyword);
                notice = "Add airline sucess";
            }
            else
            {
                notice = "This airline has been added before";
            }
            return notice;
        }
        [HttpGet("DownloadCSV/")]
        public FileResult DownloadAirlinesCSV()
        {
            var list = _context.Airlines.ToList();
            var CSVContent = new StringBuilder();
            CSVContent.AppendLine("Airline code, Airline name");
            foreach(var airline in list)
            {
                CSVContent.AppendLine(String.Format("{0}, {1}", airline.AirlineCode.Trim(), airline.AirlineName.Trim()));
            }
            var CSVBytes = Encoding.UTF8.GetBytes(CSVContent.ToString());
            return File(CSVBytes, "text/csv", "airlines.csv");
        }
    }
}
