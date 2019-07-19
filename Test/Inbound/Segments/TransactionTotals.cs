using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class TransactionTotals
    {
        public string NumberOfLineItems { get; set; }
        public string HashTotal { get; set; }
        public TransactionSetTrailer TransactionSetTrailer { get; set; }
    }
}
