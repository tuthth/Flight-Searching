using System;
using System.Collections.Generic;

namespace FlightSearching_Library.Models
{
    public partial class Airline
    {
        public Airline()
        {
            Flights = new HashSet<Flight>();
        }

        public string AirlineCode { get; set; } = null!;
        public string? AirlineName { get; set; }

        public virtual ICollection<Flight> Flights { get; set; }

        public Airline(string airlineCode, string? airlineName)
        {
            AirlineCode = airlineCode;
            AirlineName = airlineName;
        }
    }
}
