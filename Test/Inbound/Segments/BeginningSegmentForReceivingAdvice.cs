using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class BeginningSegmentForReceivingAdvice
    {
        public string ReferenceNumber { get; set; }
        public string Date { get; set; }
        public string TransactioSetPurposeCode { get; set; }
        public string ReceivingAdviceTypeCode { get; set; }

        public BeginningSegmentForReceivingAdvice(object[] row)
        {
            ReferenceNumber = row[1].ToString();
            Date = row[2].ToString();
            TransactioSetPurposeCode = row[3].ToString();
            ReceivingAdviceTypeCode = row[4].ToString();
        }
    }
}
