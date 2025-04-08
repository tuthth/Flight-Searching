using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FlightSearching_Library.Models;
using System.Text;

namespace FlightSearching_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequestsController : Controller
    {
        private readonly FlightTicketSearchContext _context;

        public RequestsController(FlightTicketSearchContext context)
        {
            _context = context;
        }
        [HttpGet("/GetRequestList", Name = "RequestList")]
        public IEnumerable<Request> GetAll() => _context.Requests.ToList();
        [HttpGet("DownloadCSV/")]
        public FileResult DownloadAirportsCSV()
        {
            var list = _context.Requests.ToList();
            var CSVContent = new StringBuilder();
            CSVContent.AppendLine("ID, Departure location, Arrival location, Departure date, Arrival date, Number of adults, Number of childs, Number of infants, Contact email\n");
            foreach (var request in list)
            {
                string email = request.ContactEmail.Trim();
                if (email.Length == 0) email = "No data";
                CSVContent.AppendLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}",
                    request.RequestId.ToString(), request.DepartureLocation.Trim(), request.ArrivalLocation.Trim(), request.DepartureDate, request.ReturnDate, 
                    request.NumberOfAdults.ToString(), request.NumberOfChildren.ToString(), request.NumberOfInfants.ToString(), email));
            }
            var CSVBytes = Encoding.UTF8.GetBytes(CSVContent.ToString());
            return File(CSVBytes, "text/csv", "requests.csv");
        }
    }
}
