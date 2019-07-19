using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class ResourceAuthorization
    {
        public string ResourceAuthorizationCode { get; set; }
        public string Date { get; set; }
        public string Quantity { get; set; }
        public string Quantity04 { get; set; }
        public string Date05 { get; set; }

        public ResourceAuthorization(object[] row)
        {
            ResourceAuthorizationCode = row[1].ToString();
            Date = row[2].ToString();
            //Quantity = row[3].ToString();
            //Quantity04 = row[4].ToString();
            //Date05 = row[5].ToString();
        }
    }
}
