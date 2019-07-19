using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class ShippedReceivedInformation
    {
        public string QuantityQualifier { get; set; }
        public string Quantity { get; set; }
        public string DateTimeQualifier { get; set; }
        public string Date { get; set; }
        public string Date06 { get; set; }
        public ReferenceNumbers ReferenceNumbers { get; set; }

        public ShippedReceivedInformation(object[] row)
        {
            QuantityQualifier = row[1].ToString();
            Quantity = row[2].ToString();
            //DateTimeQualifier = row[3].ToString();
            //Date = row[4].ToString();
            //Date06 = row[5].ToString();
        }
    }
}
