using FlightSearching_Library.Interface;
using FlightSearching_Library.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlightSearching_Library.Repo
{
    public class FlightSearchingRepo : IFlightSerachingRepo
    {
        public static IWebDriver? driver;
        public static FlightTicketSearchContext db = new FlightTicketSearchContext();
        public FlightSearchingRepo() { }
        private static FlightSearchingRepo instance = null;
        public static FlightSearchingRepo Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FlightSearchingRepo();
                }
                return instance;
            }
        }

        public void addNewAirline(string keyword)
        {
            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--window-size=1920,1080");
                options.AddArguments("--headless=new");
                options.AddArguments("--incognito");
                IWebDriver IATACodeFinder = new ChromeDriver(options);
                string url = String.Format("https://www.iata.org/en/publications/directories/code-search/?airline.search={0}", keyword);
                IATACodeFinder.Navigate().GoToUrl(url);
                Thread.Sleep(500);
                IWebElement airlineName = IATACodeFinder.FindElement(By.XPath("(//td[@data-heading='Company name'])[1]"));
                IWebElement airlineCodeWith2Char = IATACodeFinder.FindElement(By.XPath("(//td)[6]"));
                Airline airline = new Airline(airlineCodeWith2Char.GetAttribute("textContent"), airlineName.GetAttribute("textContent"));
                db.Airlines.Add(airline);
                db.SaveChanges();
                IATACodeFinder.Quit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void addNewAirport(string keyword)
        {
            string url = String.Format("https://www.nationsonline.org/oneworld/IATA_Codes/IATA_Code_{0}.htm", keyword[0]);

            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--window-size=1920,1080");
            options.AddArguments("--headless=new");
            options.AddArguments("--incognito");

            IWebDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(url);
            for (int i = 1; i <= 1000; i++)
            {
                IWebElement iataCode = driver.FindElement(By.XPath(String.Format("(//tr)[{0}]", i)));
                string result = iataCode.GetAttribute("textContent");

                if (result.Contains(keyword))
                {
                    string[] strings = result.Split("\n");
                    //Console.WriteLine(strings[1]);
                    //Console.WriteLine(strings[2]);
                    //Console.WriteLine(strings[3]);
                    //Console.WriteLine(strings[4]);
                    Airport airport = new Airport(strings[1].Trim(), strings[3], strings[2], strings[4]);
                    ///(IATA code, airport name, city, country)
                    try
                    {
                        db.Airports.Add(airport);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    //Console.WriteLine(airport.AirportName);
                    //Console.WriteLine("Add new airport to db done.");
                    break;
                }
            }
            driver.Quit();
        }

        public bool checkAirlineExist(string airline)
        {

            bool check = db.Airlines.Any(a => a.AirlineCode.Equals(airline.ToUpper()));
            return check;
        }

        public bool checkAirportExist(string airport)
        {
            bool check = db.Airports.Any(a => a.AirportCode.Equals(airport.ToUpper()));
            return check;
        }

        public List<Flight> getInternationalFlights(string departureLocation, string arrivalLocation, DateOnly departureDate, DateOnly arrivalDate,
            int numberOfAdults, int numberOfChilds, int numberOfInfants, bool businessLevel)
        {
            List<Flight> result = new List<Flight>();
            bool check = true;
            string url = "";
            if (arrivalDate < departureDate)
            {
                url = String.Format("https://www.abay.vn/Ve-May-Bay-Quoc-Te-{0}-{1}-{2}Thang{3}-{4}-{5}-{6}", departureLocation, arrivalLocation
                , departureDate.Day, departureDate.Month, numberOfAdults, numberOfChilds, numberOfInfants);
                check = true;
            }
            else if (arrivalDate > departureDate)
            {

                url = String.Format("https://www.abay.vn/_WEB/ResultInt/ResultInt.aspx?input={0}-{1}-{2}-{3}-{4}-{5}-{6}&all=1", departureLocation, arrivalLocation
                , numberOfAdults, numberOfChilds, numberOfInfants, departureDate.ToString("ddMMMyyyy"), arrivalDate.ToString("ddMMMyyyy"));
                check = false;
            }



            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--window-size=1920,1080");
            options.AddArguments("--headless=new");
            options.AddArguments("--incognito");

            driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(url);
            //driver.Manage().Window.Maximize();

            IWebElement clickToSearch = driver.FindElement(By.XPath("(//input[@id='cphMain_ctl00_usrSearchFormD2_btnSearch'])[1]"));
            clickToSearch.Click();
            WebDriverWait webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            webDriverWait.Until(e => e.FindElement(By.XPath("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[1]")));
            if (businessLevel == true)
            {
                IWebElement showBusinessPrice = driver.FindElement(By.XPath("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[2]/div[1]/section[3]/ul[1]/li[3]/label[1]"));
                showBusinessPrice.Click();
                
            }


            if (check == true)
            {
                result = getInternationalFlights_OneWay(departureLocation, arrivalLocation, departureDate);
            }
            else
            {
                
                result = getInternationalFlights_RoundTrip(departureLocation, arrivalLocation, departureDate, arrivalDate);
            }
            driver.Quit();
            return result;
        }

        public List<Flight> getInternationalFlights_OneWay(string departureLocation, string arrivalLocation, DateOnly departureDate)
        {
            List<Flight> result = new List<Flight>();

            for (int i = 2; i <= 400; i++)
            {
                int k = db.Flights.Count();
                try
                {
                    IWebElement departAt = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[1]/ul[1]/li[1]/div[1]/span[1]/code[1]", i)));
                    IWebElement arriveAt = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[1]/ul[1]/li[1]/div[1]/span[1]/code[3]", i)));
                    IWebElement flightStart = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[1]/ul[1]/li[1]/div[2]/strong[1]", i)));
                    IWebElement flightEnd = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[1]/ul[1]/li[1]/div[2]/strong[2]", i)));
                    IWebElement numberOfTransit = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[1]/ul[1]/li[1]/div[2]/div[1]/span[1]", i)));
                    IWebElement flightTime = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[1]/ul[1]/li[1]/span[3]", i)));
                    IWebElement showMoreDetails = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[1]/div[1]/button[1]", i)));
                    showMoreDetails.Click();
                    IWebElement airline = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[1]/ul[1]/li[1]/span[1]/img[1]", i)));
                    IWebElement? finalPrice = null;
                    try
                    {
                        finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[2]/ul[1]/li[3]/span[2]/strong[1]", i)));
                    }
                    catch
                    {
                        try
                        {
                            finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[2]/ul[1]/li[4]/span[2]/strong[1]", i)));
                        }
                        catch
                        {
                            try
                            {
                                finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[3]/div[{0}]/div[2]/ul[1]/li[5]/span[2]/strong[1]", i * 2 + 1)));
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }

                    Thread.Sleep(500);
                    Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", departureLocation, arrivalLocation, flightStart.GetAttribute("textContent"),
                        flightEnd.GetAttribute("textContent"), numberOfTransit.GetAttribute("textContent"),
                        flightTime.GetAttribute("textContent"), airline.GetAttribute("alt"), finalPrice.GetAttribute("textContent")));
                    bool checkAirlineIsExist = checkAirlineExist(airline.GetAttribute("alt"));
                    bool checkAirport1Exist = checkAirportExist(departAt.GetAttribute("textContent"));
                    bool checkAirport2Exist = checkAirportExist(arriveAt.GetAttribute("textContent"));
                    if (departAt.GetAttribute("textContent").Equals(departureLocation) && arriveAt.GetAttribute("textContent").Equals(arrivalLocation))
                    {
                        string airportDepartName = db.Airports.FirstOrDefault(a => a.AirportCode.Equals(departureLocation)).AirportName;
                        string airportArriveName = db.Airports.FirstOrDefault(a => a.AirportCode.Equals(arrivalLocation)).AirportName;
                        string airlineCode = airline.GetAttribute("alt").Substring(0, 2).ToUpper();
                        var airlineData = db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airlineCode));
                        if(airlineData == null)
                        {
                            addNewAirline(airlineCode);
                            airlineData = db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airlineCode));
                        }
                        string airlineName = airlineData.AirlineName;
                        DateTime start = departureDate.ToDateTime(TimeOnly.MinValue) + TimeSpan.Parse(flightStart.GetAttribute("textContent"));
                        string[] splits = flightTime.GetAttribute("textContent").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        DateTime end = start + new TimeSpan(Int32.Parse(splits[0]), Int32.Parse(splits[2]), 0);
                        string[] priceSplits = finalPrice.GetAttribute("textContent").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        decimal price = Decimal.Parse(priceSplits[0].Replace(",", ""));
                        try
                        {
                            k++;
                            Flight flight = new Flight(k, airportDepartName, airportArriveName, start, end, airlineName
                            , price, departureLocation, arrivalLocation, airlineCode);
                            db.Flights.Add(flight);
                            result.Add(flight);
                            db.SaveChanges();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Out of branch at i = {0}", i);
                    break;

                }

            }
            return result;
        }

        public List<Flight> getInternationalFlights_RoundTrip(string departureLocation, string arrivalLocation, DateOnly departureDate, DateOnly arrivalDate)
        {
            List<Flight> result = new List<Flight>();
            for (int i = 2; i < 400; i++)
            {
                int size = db.Flights.Count();
                try
                {
                    IWebElement departLocation1 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[1]/div[1]/span[1]/code[1]", i)));
                    bool checkDepartLocation1Exists = checkAirportExist(departLocation1.GetAttribute("textContent"));
                    IWebElement departLocation2 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[2]/div[1]/span[1]/code[1]", i)));
                    bool checkDepartLocation2Exists = checkAirportExist(departLocation2.GetAttribute("textContent"));
                    IWebElement arriveLocation1 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[1]/div[1]/span[1]/code[3]", i)));
                    bool checkArriveLocation1Exists = checkAirportExist(arriveLocation1.GetAttribute("textContent"));
                    IWebElement arriveLocation2 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[2]/div[1]/span[1]/code[3]", i)));
                    bool checkArriveLocation2Exists = checkAirportExist(arriveLocation2.GetAttribute("textContent"));
                    IWebElement flightStart1 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[1]/div[2]/strong[1]", i)));
                    IWebElement flightStart2 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[2]/div[2]/strong[1]", i)));
                    IWebElement flightEnd1 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[1]/div[2]/strong[2]", i)));
                    IWebElement flightEnd2 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[2]/div[2]/strong[2]", i)));
                    IWebElement numberOfTransit1 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[1]/div[2]/div[1]/span[1]", i)));
                    IWebElement numberOfTransit2 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[2]/div[2]/div[1]/span[1]", i)));
                    IWebElement flightTime1 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[1]/span[3]/small[1]", i)));
                    IWebElement flightTime2 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[2]/span[3]/small[1]", i)));
                    IWebElement showDetails = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/div[1]/button[1]", i)));
                    showDetails.Click();
                    IWebElement airline1 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[1]/span[1]/img[1]", i)));
                    bool checkAirline1Exist = checkAirlineExist(airline1.GetAttribute("alt"));
                    IWebElement airline2 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[1]/ul[1]/li[2]/span[1]/img[1]", i)));
                    bool checkAirline2Exist = checkAirlineExist(airline2.GetAttribute("alt"));
                    IWebElement? finalPrice = null;
                    try
                    {
                        finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[2]/ul[1]/li[5]/span[2]/strong[1]", i)));
                    }
                    catch
                    {
                        try
                        {
                            finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[2]/ul[1]/li[4]/span[2]/strong[1]", i)));
                        }
                        catch
                        {
                            try
                            {
                                finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/section[2]/div[{0}]/div[2]/ul[1]/li[3]/span[2]/strong[1]", i)));
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }

                    Thread.Sleep(500);

                    //add new flight
                    DateTime departFlightTime1 = departureDate.ToDateTime(TimeOnly.MinValue) + TimeSpan.Parse(flightStart1.GetAttribute("textContent"));
                    string[] splits1 = flightTime1.GetAttribute("textContent").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    DateTime arriveFlightTime1 = departFlightTime1 + new TimeSpan(Int32.Parse(splits1[0]), Int32.Parse(splits1[2]), 0);
                    DateTime departFlightTime2 = arrivalDate.ToDateTime(TimeOnly.MinValue) + TimeSpan.Parse(flightStart2.GetAttribute("textContent"));
                    string[] splits2 = flightTime2.GetAttribute("textContent").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    DateTime arriveFlightTime2 = departFlightTime2 + new TimeSpan(Int32.Parse(splits2[0]), Int32.Parse(splits2[2]), 0);
                    string[] priceSplits = finalPrice.GetAttribute("textContent").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    decimal price = Decimal.Parse(priceSplits[0]);

                    ////test on console
                    //Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", departLocation1.GetAttribute("textContent"), arriveLocation1.GetAttribute("textContent"), flightStart1.GetAttribute("textContent"),
                    //    flightEnd1.GetAttribute("textContent"), numberOfTransit1.GetAttribute("textContent"),
                    //    flightTime1.GetAttribute("textContent"), airline1.GetAttribute("alt"), finalPrice.GetAttribute("textContent")));
                    //Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", departLocation2.GetAttribute("textContent"), arriveLocation2.GetAttribute("textContent"), flightStart2.GetAttribute("textContent"),
                    //   flightEnd2.GetAttribute("textContent"), numberOfTransit2.GetAttribute("textContent"),
                    //   flightTime2.GetAttribute("textContent"), airline2.GetAttribute("alt"), finalPrice.GetAttribute("textContent")));

                    var airlineData1 = db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airline1.GetAttribute("alt")));
                    if (airlineData1 == null)
                    {
                        addNewAirline(airline1.GetAttribute("alt"));
                        airlineData1 = db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airline1.GetAttribute("alt")));
                    }
                    var airlineData2 = db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airline2.GetAttribute("alt")));
                    if (airlineData2 == null)
                    {
                        addNewAirline(airline2.GetAttribute("alt"));
                        airlineData2 = db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airline2.GetAttribute("alt")));
                    }
                    //add flight 1
                    int flightId1 = size + 1;
                    int flightId2 = size + 2;
                    Flight flight1 = new Flight(flightId1, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(departLocation1.GetAttribute("textContent"))).AirportName, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(arriveLocation1.GetAttribute("textContent"))).AirportName,
                        departFlightTime1, arriveFlightTime1, db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airline1.GetAttribute("alt"))).AirlineName, 0,
                        departLocation1.GetAttribute("textContent"), arriveLocation1.GetAttribute("textContent"), airline1.GetAttribute("alt"));
                    Flight flight2 = new Flight(flightId2, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(departLocation2.GetAttribute("textContent"))).AirportName, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(arriveLocation2.GetAttribute("textContent"))).AirportName,
                    departFlightTime2, arriveFlightTime2, db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airline2.GetAttribute("alt"))).AirlineName, price,
                    departLocation2.GetAttribute("textContent"), arriveLocation2.GetAttribute("textContent"), airline2.GetAttribute("alt"));
                    db.Flights.Add(flight1);
                    db.Flights.Add(flight2);
                    result.Add(flight1);
                    result.Add(flight2);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Out of branch at i = {0}", i);
                    Console.WriteLine(ex.ToString());
                    break;
                }

            }
            return result;
        }

        public List<Flight> getLocalFlights(string departureLocation, string arrivalLocation, DateOnly departureDate, DateOnly arrivalDate,
            int numberOfAdults, int numberOfChilds, int numberOfInfants, bool businessLevel)
        {
            List<Flight> result = new List<Flight>();
            string url = "";
            bool check = true; ///check if user choose one-way, or round-trip
            if (arrivalDate < departureDate)
            {
                url = String.Format("https://www.abay.vn/Ve-May-Bay-Gia-Re-{0}-{1}-{2}Thang{3}-{4}-{5}-{6}", departureLocation, arrivalLocation
                , departureDate.Day, departureDate.Month, numberOfAdults, numberOfChilds, numberOfInfants);
                check = true;
            }
            else if (arrivalDate > departureDate)
            {

                url = String.Format("https://www.abay.vn/_WEB/ResultInt/ResultInt.aspx?input={0}-{1}-{2}-{3}-{4}-{5}-{6}&all=1", departureLocation, arrivalLocation
                , numberOfAdults, numberOfChilds, numberOfInfants, departureDate.ToString("ddMMMyyyy"), arrivalDate.ToString("ddMMMyyyy"));
                check = false;
            }



            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--window-size=1920,1080");
            options.AddArguments("--headless=new");
            options.AddArguments("--incognito");

            driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(url);
            //driver.Manage().Window.Maximize();

            IWebElement clickToSearch = driver.FindElement(By.XPath("(//input[@id='cphMain_ctl00_usrSearchFormD2_btnSearch'])[1]"));
            clickToSearch.Click();
            Thread.Sleep(3000);
            if (businessLevel == true)
            {
                IWebElement showBusinessPrice = driver.FindElement(By.XPath("//body[1]/form[1]/div[2]/div[6]/div[3]/div[2]/div[1]/div[2]/div[1]/table[1]/tbody[1]/tr[3]/td[1]"));
                showBusinessPrice.Click();
                Thread.Sleep(3000);
            }



            if (check == true)
            {
                result = getLocalFlights_OneWay(departureLocation, arrivalLocation, departureDate);
            }
            else
            {
                result = getLocalFlights_RoundTrip(departureLocation, arrivalLocation, departureDate, arrivalDate);
            }
            driver.Quit();
            return result;
        }

        public List<Flight> getLocalFlights_OneWay(string departureLocation, string arrivalLocation, DateOnly departureDate)
        {
            List<Flight> result = new List<Flight>();
            for (int i = 2; i < 400; i++)
            {
                int k = db.Flights.Count();
                try
                {
                    IWebElement airline = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[1]/img[1]", i * 2)));
                    IWebElement showDetails = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[6]/a[1]/b[1]", i * 2)));
                    Thread.Sleep(500);
                    showDetails.Click();
                    Thread.Sleep(500);
                    IWebElement flightStart = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[1]/tbody[1]/tr[1]/td[1]/p[2]", i * 2 + 1)));
                    IWebElement flightEnd = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[1]/tbody[1]/tr[1]/td[3]/p[2]", i * 2 + 1)));
                    IWebElement departAt = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[1]/tbody[1]/tr[1]/td[1]/p[1]/b[1]", i * 2 + 1)));
                    IWebElement arriveAt = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[1]/tbody[1]/tr[1]/td[3]/p[1]/b[1]", i * 2 + 1)));
                    IWebElement? finalPrice = null;

                    try
                    {
                        finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[5]/td[2]", i * 2 + 1)));
                    }
                    catch
                    {
                        try
                        {
                            finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[4]/td[2]", i * 2 + 1)));
                        }
                        catch
                        {
                            try
                            {
                                finalPrice = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[2]/td[5]", i * 2 + 1)));
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }

                    Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}", departureLocation, arrivalLocation, flightStart.GetAttribute("textContent"),
                        flightEnd.GetAttribute("textContent"), airline.GetAttribute("alt"), finalPrice.GetAttribute("textContent")));

                    //use regex to split string
                    string pattern = @"\((\w+)\)";
                    Match match1 = Regex.Match(departAt.GetAttribute("textContent"), pattern);
                    Match match2 = Regex.Match(arriveAt.GetAttribute("textContent"), pattern);
                    string depart = match1.Groups[1].Value;
                    string arrive = match2.Groups[1].Value;

                    //set DateTime format
                    string format = "HH:mm, dd/MM/yyyy";
                    DateTime start = DateTime.ParseExact(flightStart.GetAttribute("textContent"), format, null);
                    DateTime end = DateTime.ParseExact(flightEnd.GetAttribute("textContent"), format, null);

                    //airline code
                    string airlineCode = airline.GetAttribute("alt").Substring(0, 2);

                    bool checkAirlineIsExist = checkAirlineExist(airlineCode);
                    bool checkAirport1Exist = checkAirportExist(depart);
                    bool checkAirport2Exist = checkAirportExist(arrive);


                    //set price
                    string priceInString = finalPrice.GetAttribute("textContent");
                    string[] parts = priceInString.Split(" ");
                    decimal price = Decimal.Parse(parts[0]);

                    //set new Flight
                    k++;
                    Flight flight = new Flight(k, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(depart)).AirportName, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(arrive)).AirportName, start, end,
                        db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airlineCode)).AirlineName, price, depart, arrive, airlineCode);
                    db.Flights.Add(flight);
                    result.Add(flight);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return result;

                }

            }
            return result;
        }

        public List<Flight> getLocalFlights_RoundTrip(string departureLocation, string arrivalLocation, DateOnly departureDate, DateOnly arrivalDate)
        {
            List<Flight> result = new List<Flight>();
            for (int i = 2; i < 400; i++)
            {
                int k = db.Flights.Count();
                try
                {
                    IWebElement? showDetails1 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[{0}]/td[6]/a[1]", i * 2)));
                    showDetails1.Click();
                    Thread.Sleep(500);
                    IWebElement? airline1 = driver.FindElement(By.XPath(String.Format("/html[1]/body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[{0}]/td[1]/img[1]", i * 2)));
                    IWebElement? flightStart1 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[1]/tbody[1]/tr[1]/td[1]/p[2]", i * 2 + 1)));
                    IWebElement? flightEnd1 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[1]/tbody[1]/tr[1]/td[3]/p[2]", i * 2 + 1)));
                    IWebElement? price1 = null;

                    try
                    {
                        price1 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[5]/td[2]", i * 2 + 1)));
                    }
                    catch
                    {
                        try
                        {
                            price1 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[4]/td[2]", i * 2 + 1)));
                        }
                        catch
                        {
                            try
                            {
                                price1 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[2]/td[5]", i * 2 + 1)));
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }

                    IWebElement? showDetails2 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[{0}]/td[6]/a[1]", i * 2)));
                    showDetails2.Click();
                    Thread.Sleep(500);
                    IWebElement? airline2 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[{0}]/td[1]/img[1]", i * 2)));
                    IWebElement? flightStart2 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[1]/tbody[1]/tr[1]/td[1]/p[2]", i * 2 + 1)));
                    IWebElement? flightEnd2 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[1]/tbody[1]/tr[1]/td[3]/p[2]", i * 2 + 1)));
                    IWebElement? price2 = null;

                    try
                    {
                        price2 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[5]/td[2]", i * 2 + 1)));
                    }
                    catch
                    {
                        try
                        {
                            price2 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[4]/td[2]", i * 2 + 1)));
                        }
                        catch
                        {
                            try
                            {
                                price2 = driver.FindElement(By.XPath(String.Format("//body[1]/form[1]/div[2]/div[6]/div[3]/div[1]/div[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[{0}]/td[1]/div[1]/table[2]/tbody[1]/tr[2]/td[5]", i * 2 + 1)));
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }
                    if (showDetails1 == null && showDetails2 == null) break;
                    //test on console
                    Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}", departureLocation, arrivalLocation, flightStart1.GetAttribute("textContent"),
                        flightEnd1.GetAttribute("textContent"), airline1.GetAttribute("alt"), price1.GetAttribute("textContent")));
                    Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}", arrivalLocation, departureLocation, flightStart2.GetAttribute("textContent"),
                       flightEnd2.GetAttribute("textContent"), airline2.GetAttribute("alt"), price2.GetAttribute("textContent")));

                    //set DateTime format
                    string format = "HH:mm, dd/MM/yyyy";
                    DateTime start1 = DateTime.ParseExact(flightStart1.GetAttribute("textContent"), format, null);
                    DateTime end1 = DateTime.ParseExact(flightEnd1.GetAttribute("textContent"), format, null);
                    DateTime start2 = DateTime.ParseExact(flightStart2.GetAttribute("textContent"), format, null);
                    DateTime end2 = DateTime.ParseExact(flightEnd2.GetAttribute("textContent"), format, null);

                    //airline code
                    string airlineCode1 = airline1.GetAttribute("alt").Substring(0, 2);
                    string airlineCode2 = airline2.GetAttribute("alt").Substring(0, 2);

                    bool checkAirline1Exist = checkAirlineExist(airlineCode1);
                    bool checkAirline2Exist = checkAirlineExist(airlineCode1);
                    bool checkAirport1Exist = checkAirportExist(departureLocation);
                    bool checkAirport2Exist = checkAirportExist(arrivalLocation);


                    //set price
                    string price1InString = price1.GetAttribute("textContent");
                    decimal finalPrice1 = Decimal.Parse(price1InString.Trim());

                    string price2InString = price2.GetAttribute("textContent");
                    decimal finalPrice2 = Decimal.Parse(price2InString.Trim());

                    //add flight
                    Flight flight1 = new Flight(k + 1, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(departureLocation)).AirportName, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(arrivalLocation)).AirportName, start1, end1,
                        db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airlineCode1)).AirlineName, finalPrice1, departureLocation, arrivalLocation, airlineCode1);
                    Flight flight2 = new Flight(k + 2, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(arrivalLocation)).AirportName, db.Airports.FirstOrDefault(a => a.AirportCode.Equals(departureLocation)).AirportName, start2, end2,
                       db.Airlines.FirstOrDefault(a => a.AirlineCode.Equals(airlineCode2)).AirlineName, finalPrice2, arrivalLocation, departureLocation, airlineCode2);

                    db.Flights.Add(flight1);
                    db.Flights.Add(flight2);
                    db.SaveChanges();
                    result.Add(flight1);
                    result.Add(flight2);
                }
                catch (Exception e)
                {
                    return result;
                }

            }
            return result;
        }

        public bool localOrInternational(string loca1, string loca2)
        {
            bool check = false;
            try
            {
                Airport a1 = db.Airports.Where(a => a.AirportCode.Equals(loca1)).FirstOrDefault();
                Airport a2 = db.Airports.Where(a => a.AirportCode.Equals(loca2)).FirstOrDefault();
                if (a1.Country.Replace("\r\n", string.Empty).Trim().Equals(a2.Country.Replace("\r\n", string.Empty).Trim()) && a1.Country.Replace("\r\n", string.Empty).Trim().Equals("Viet Nam"))
                {
                    check = true;
                }
                else
                {
                    check = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when checking");
                Console.WriteLine(ex);
            }
            return check;
        }

        public void removeAll()
        {
            var removeAll = db.Flights.ToList();
            db.Flights.RemoveRange(removeAll);
            db.SaveChanges();
        }
    }
}
