using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class AdministrativeCommunicationContact
    {
        public string ContactFunctionCode { get; set; }
        public string Name { get; set; }
        public string CommunicationNumberQualifier { get; set; }
        public string CommunicationNumber { get; set; }
        public string CommunicationNumberQualifier05 { get; set; }
        public string CommunicationNumber06 { get; set; }

        public AdministrativeCommunicationContact(object[] row)
        {
            ContactFunctionCode = row[1].ToString();
            Name = row[2].ToString();
        }
    }
}
