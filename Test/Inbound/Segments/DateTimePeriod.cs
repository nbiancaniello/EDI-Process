using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class DateTimePeriod
    {
        public string DateTimeQualifier { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string TimeCode { get; set; }

        public DateTimePeriod(object[] row)
        {
            DateTimeQualifier = row[1].ToString();
            Date = row[2].ToString();
            Time = row[3].ToString();
            TimeCode = row[4].ToString();
        }
    }
}
