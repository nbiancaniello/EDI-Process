using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class ReferenceNumbers
    {
        public string ReferenceNumberQualifier { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }

        public ReferenceNumbers(object[] row)
        {
            ReferenceNumberQualifier = row[1].ToString();
            ReferenceNumber = row[2].ToString();
            //Description = row[3].ToString();
        }
    }
}
