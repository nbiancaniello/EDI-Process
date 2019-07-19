namespace Test.Inbound.Segments
{
    public class BeginningSegmentForPlanningSchedule
    {
        public string TransactionSetPurposeCode { get; set; }
        public string ReferenceNumber { get; set; }
        public string ReleaseNumber { get; set; }
        public string ScheduleTypeQualifier { get; set; }
        public string ScheduleQuantityQualifier { get; set; }
        public string Date06 { get; set; }
        public string Date07 { get; set; }
        public string Date08 { get; set; }
        public string Date09 { get; set; }
        public string ContractNumber { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string PlanningScheduleTypeCode { get; set; }
        public string ActionCode { get; set; }

        public BeginningSegmentForPlanningSchedule(object[] row)
        {
            TransactionSetPurposeCode = row[1].ToString();
            ReferenceNumber = row[2].ToString();
            ReleaseNumber = row[3].ToString();
            ScheduleTypeQualifier = row[4].ToString();
            ScheduleQuantityQualifier = row[5].ToString();
            Date06 = row[6].ToString();
            Date07 = row[7].ToString();
            Date08 = row[8].ToString();
            Date09 = row[9].ToString();
        }
    }
}
