using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class TransactionSetTrailer
    {
        public string NumberOfIncludedSegments { get; set; }
        public string TransactionSetControlNumber { get; set; }
    }
}
