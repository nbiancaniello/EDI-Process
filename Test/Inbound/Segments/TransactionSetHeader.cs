using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class TransactionSetHeader
    {
        public string TransactionSetIdentifierCode { get; set; }
        public string TransactionSetControlNumber { get; set; }

        public TransactionSetHeader(object[] row)
        {
            TransactionSetIdentifierCode = row[1].ToString();
            TransactionSetControlNumber = row[2].ToString();
        }
    }
}
