using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class ForecastSchedule
    {
        public string Quantity { get; set; }
        public string ForecastQualifier { get; set; }
        public string ForecastTimingQualifier { get; set; }
        public string Date { get; set; }
        public string Date05 { get; set; }

        public ForecastSchedule(object[] row)
        {
            Quantity = row[1].ToString();
            ForecastQualifier = row[2].ToString();
            ForecastTimingQualifier = row[3].ToString();
            Date = row[4].ToString();
            //Date05 = row[5].ToString();
        }
    }
}
