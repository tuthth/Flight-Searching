using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FlightSearching_Library.Models;
using FlightSearching_Library.Repo;
using System.Text;

namespace FlightSearching_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlightsController : Controller
    {
        private readonly FlightTicketSearchContext _context;

        public FlightsController(FlightTicketSearchContext context)
        {
            _context = context;
        }

        [HttpPost("/GetFlights", Name = "GetFlight")]
        public List<Flight> GetFlights([FromBody]Request request) { 
            List<Flight> list = new List<Flight>();
            FlightSearchingRepo repo = new FlightSearchingRepo();
            bool checkLocal = repo.localOrInternational(request.DepartureLocation, request.ArrivalLocation);
            DateOnly departDate = DateOnly.FromDateTime((DateTime)request.DepartureDate);
            DateOnly returnDate = new DateOnly();
            try
            {
                returnDate = DateOnly.FromDateTime((DateTime)request.ReturnDate);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (checkLocal == true) {
               list = repo.getLocalFlights(request.DepartureLocation, request.ArrivalLocation, departDate, returnDate, (int)request.NumberOfAdults
                    , (int)request.NumberOfChildren, (int)request.NumberOfInfants, false);
            }
            else
            {
               list = repo.getInternationalFlights(request.DepartureLocation, request.ArrivalLocation, departDate, returnDate, (int)request.NumberOfAdults
                    , (int)request.NumberOfChildren, (int)request.NumberOfInfants, false);
            }
            return list;
        }
        [HttpGet("DownloadCSV/")]
        public FileResult DownloadFlightsCSV()
        {
            var flights = _context.Flights.ToList();
            var CSVContent = new StringBuilder();
            CSVContent.Append("FlightID, Departure Location, Arrival Location, Departure Date, Arrival Date, Airline, Price\n");
            foreach (var flight in flights)
            {
                decimal price = (decimal)flight.Price;
                int priceInInt = (int)price;
                CSVContent.AppendLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}",
                    flight.FlightId, flight.DepartureLocation.Trim(), flight.ArrivalLocation.Trim(), flight.DepartureDate, flight.ArrivalDate, flight.Airline.Trim(), priceInInt.ToString()));
            }
            var CSVBytes = Encoding.UTF8.GetBytes(CSVContent.ToString());
            return File(CSVBytes, "text/csv", "flights.csv");
        }
    }
}
