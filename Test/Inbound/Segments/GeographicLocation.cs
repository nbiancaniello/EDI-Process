using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class GeographicLocation
    {
        public string CityName { get; set; }
        public string StateOrProvinceCode { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string LocationQualifier { get; set; }
        public string LocationIdentifier { get; set; }
    }
}
