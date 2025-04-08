using FlightSearching_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSearching_Library.Interface
{
    public interface IFlightSerachingRepo
    {
        public void removeAll();
        public void addNewAirport(string keyword);
        public void addNewAirline(string keyword);
        public bool localOrInternational(string t1, string t2);
        public bool checkAirportExist(string keyword);
        public bool checkAirlineExist(string keyword);
        public List<Flight> getLocalFlights(string departureLocation, string arrivalLocation, DateOnly departureDate, DateOnly arrivalDate,
            int numberOfAdults, int numberOfChilds, int numberOfInfants, bool businessLevel);
        public List<Flight> getLocalFlights_OneWay(string departureLocation, string arrivalLocation, DateOnly departureDate);
        public List<Flight> getLocalFlights_RoundTrip(string departureLocation, string arrivalLocation, DateOnly departureDate, DateOnly arrivalDate);
        public List<Flight> getInternationalFlights(string departureLocation, string arrivalLocation, DateOnly departureDate, DateOnly arrivalDate,
            int numberOfAdults, int numberOfChilds, int numberOfInfants, bool businessLevel);
        public List<Flight> getInternationalFlights_OneWay(string departureLocation, string arrivalLocation, DateOnly departureDate);
        public List<Flight> getInternationalFlights_RoundTrip(string departureLocation, string arrivalLocation, DateOnly departureDate, DateOnly arrivalDate);

    }
}
