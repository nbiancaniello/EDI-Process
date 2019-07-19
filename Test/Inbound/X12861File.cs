using System.Collections.Generic;
using Test.Inbound.Segments;

namespace Test.Inbound
{
    public class X12861File
    {
        public TransactionSetHeader TransactionSetHeader { get; set; }
        public BeginningSegmentForPlanningSchedule BeginningSegmentForPlanningSchedule { get; set; }
        public SpecialInstruction SpecialInstruction { get; set; }
        public List<ReferenceNumbers> ReferenceNumbers { get; set; }
        public List<DateTimePeriod> DateTimePeriods { get; set; }
        public List<Name> Names { get; set; }
        public List<ItemIdentification> ItemIdentifications { get; set; }
        public List<ShipDeliveryPattern> ShipDeliveryPatterns { get; set; }
        public List<ShippedReceivedInformation> ShippedReceivedInformation { get; set; }
        public List<TransactionTotals> TransactionTotals { get; set; }

        public X12861File(List<object> file)
        {
            foreach (object[] line in file)
            {
                switch (line[0].ToString())
                {
                    case "ISA":
                    case "GS":
                    case "SE":
                    case "GE":
                    case "IEA":
                        break;
                    case "ST":
                        TransactionSetHeader = new TransactionSetHeader(line);
                        break;
                    case "N1":
                        break;
                    default:
                        break;
                }
            }
            
        }
    }
}
