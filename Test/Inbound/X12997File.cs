using System.Collections.Generic;
using Test.Inbound.Segments;

namespace Test.Inbound
{
    public class X12997File
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
    }
}
