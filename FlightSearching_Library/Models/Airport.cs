using System;
using System.Collections.Generic;

namespace FlightSearching_Library.Models
{
    public partial class Airport
    {
        public Airport()
        {
            FlightArrivalAirportCodeNavigations = new HashSet<Flight>();
            FlightDepartureAirportCodeNavigations = new HashSet<Flight>();
        }

        public string AirportCode { get; set; } = null!;
        public string? AirportName { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public virtual ICollection<Flight> FlightArrivalAirportCodeNavigations { get; set; }
        public virtual ICollection<Flight> FlightDepartureAirportCodeNavigations { get; set; }

        public Airport(string airportCode, string? airportName, string? city, string? country)
        {
            AirportCode = airportCode;
            AirportName = airportName;
            City = city;
            Country = country;
        }
    }
}
