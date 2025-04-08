using System;
using System.Collections.Generic;

namespace FlightSearching_Library.Models
{
    public partial class Request
    {
        public int RequestId { get; set; }
        public string? DepartureLocation { get; set; }
        public string? ArrivalLocation { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? NumberOfAdults { get; set; }
        public int? NumberOfChildren { get; set; }
        public int? NumberOfInfants { get; set; }
        public string? ContactEmail { get; set; }

        public Request()
        {
        }

        public Request(int requestId, string? departureLocation, string? arrivalLocation, DateTime? departureDate, DateTime? returnDate, int? numberOfAdults, int? numberOfChildren, int? numberOfInfants, string? contactEmail)
        {
            RequestId = requestId;
            DepartureLocation = departureLocation;
            ArrivalLocation = arrivalLocation;
            DepartureDate = departureDate;
            ReturnDate = returnDate;
            NumberOfAdults = numberOfAdults;
            NumberOfChildren = numberOfChildren;
            NumberOfInfants = numberOfInfants;
            ContactEmail = contactEmail;
        }
    }
}
