using System;
using System.Collections.Generic;

namespace FlightSearching_Library.Models
{
    public partial class Flight
    {
        public int FlightId { get; set; }
        public string? DepartureLocation { get; set; }
        public string? ArrivalLocation { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string? Airline { get; set; }
        public decimal? Price { get; set; }
        public string? DepartureAirportCode { get; set; }
        public string? ArrivalAirportCode { get; set; }
        public string? AirlineCode { get; set; }

        public virtual Airline? AirlineCodeNavigation { get; set; }
        public virtual Airport? ArrivalAirportCodeNavigation { get; set; }
        public virtual Airport? DepartureAirportCodeNavigation { get; set; }

        public Flight(int flightId, string? departureLocation, string? arrivalLocation, DateTime? departureDate, DateTime? arrivalDate, string? airline, decimal? price, string? departureAirportCode, string? arrivalAirportCode, string? airlineCode)
        {
            FlightId = flightId;
            DepartureLocation = departureLocation;
            ArrivalLocation = arrivalLocation;
            DepartureDate = departureDate;
            ArrivalDate = arrivalDate;
            Airline = airline;
            Price = price;
            DepartureAirportCode = departureAirportCode;
            ArrivalAirportCode = arrivalAirportCode;
            AirlineCode = airlineCode;
        }
    }
}
