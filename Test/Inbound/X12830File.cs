using System.Collections.Generic;
using Test.Inbound.Segments;

namespace Test.Inbound
{
    public class X12830File
    {
        public TransactionSetHeader TransactionSetHeader { get; set; }
        public BeginningSegmentForPlanningSchedule BeginningSegmentForPlanningSchedule { get; set; }
        public SpecialInstruction SpecialInstruction { get; set; }
        public List<Name> Names { get; set; }
        //public List<DateTimePeriod> DateTimePeriods { get; set; }
        public List<ItemIdentification> ItemIdentification { get; set; }
        public List<ShipDeliveryPattern> ShipDeliveryPattern { get; set; }
        public List<ShippedReceivedInformation> ShippedReceivedInformation { get; set; }
        //public List<TransactionTotals> TransactionTotals { get; set; }

        public X12830File(List<object> file)
        {
            Names = new List<Name>();
            ItemIdentification itemIdentification = null;
            ItemIdentification = new List<ItemIdentification>();
            UnitDetail unitDetail;
            ReferenceNumbers referenceNumbers;
            AdministrativeCommunicationContact administrativeCommunicationContact;
            List<ForecastSchedule> forecastSchedules = new List<ForecastSchedule>();
            List<ResourceAuthorization> resourceAuthorizations = new List<ResourceAuthorization>();
            ShippedReceivedInformation shippedReceivedInformation = null;
            List<ShippedReceivedInformation> shippedReceivedInformations = new List<ShippedReceivedInformation>();
            string loop = null;

            foreach (object[] line in file)
            {
                switch (line[0].ToString())
                {
                    case "ST":
                        TransactionSetHeader = new TransactionSetHeader(line);
                        break;
                    case "BFR":
                        BeginningSegmentForPlanningSchedule = new BeginningSegmentForPlanningSchedule(line);
                        break;
                    case "N1":
                        Names.Add(new Name(line));
                        break;
                    case "LIN":
                        //ItemIdentifications.Add(new ItemIdentification(line));
                        if (itemIdentification != null)
                        {
                            if (itemIdentification.ProductServiceID != line[3].ToString())
                            {
                                itemIdentification.ShippedReceivedInformation.Add(shippedReceivedInformation);
                                ItemIdentification.Add(itemIdentification);
                                itemIdentification = new ItemIdentification(line);
                                itemIdentification.ShippedReceivedInformation = new List<ShippedReceivedInformation>();
                                itemIdentification.ForecastSchedule = new List<ForecastSchedule>();
                                itemIdentification.ResourceAuthorization = new List<ResourceAuthorization>();
                                loop = "LIN";
                            }
                        } else
                        {
                            itemIdentification = new ItemIdentification(line);
                            itemIdentification.ShippedReceivedInformation = new List<ShippedReceivedInformation>();
                            itemIdentification.ForecastSchedule = new List<ForecastSchedule>();
                            itemIdentification.ResourceAuthorization = new List<ResourceAuthorization>();
                            loop = "LIN";
                        }
                        break;
                    case "UIT":
                        unitDetail = new UnitDetail(line);
                        itemIdentification.UnitDetail = unitDetail;
                        break;
                    case "REF":
                        referenceNumbers = new ReferenceNumbers(line);
                        switch (loop)
                        {
                            case "LIN":
                                itemIdentification.ReferenceNumbers = referenceNumbers;
                                break;
                            case "SHP":
                                shippedReceivedInformation.ReferenceNumbers = referenceNumbers;
                                shippedReceivedInformations.Add(shippedReceivedInformation);
                                itemIdentification.ShippedReceivedInformation.Add(shippedReceivedInformation);
                                break;
                            default:
                                break;
                        }
                        break;
                    case "PER":
                        administrativeCommunicationContact = new AdministrativeCommunicationContact(line);
                        itemIdentification.AdministrativeCommunicationContact = administrativeCommunicationContact;
                        break;
                    case "FST":
                        forecastSchedules.Add(new ForecastSchedule(line));
                        break;
                    case "ATH":
                        resourceAuthorizations.Add(new ResourceAuthorization(line));
                        break;
                    case "SHP":
                        //shippedReceivedInformation.Add(new ShippedReceivedInformation(line));
                        shippedReceivedInformation = new ShippedReceivedInformation(line);
                        loop = "SHP";
                        break;
                    default:
                        break;
                }
            }

            shippedReceivedInformations.Add(shippedReceivedInformation);
            itemIdentification.ShippedReceivedInformation.Add(shippedReceivedInformation);
            itemIdentification.ForecastSchedule = forecastSchedules;
            itemIdentification.ResourceAuthorization = resourceAuthorizations;
            ItemIdentification.Add(itemIdentification);
        }
    }
}
