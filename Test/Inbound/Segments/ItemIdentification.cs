using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class ItemIdentification
    {
        public string AssignedIdentification { get; set; }
        public string ProductServiceIDQualifier { get; set; }
        public string ProductServiceID { get; set; }
        public string ProductServiceIDQualifier04 { get; set; }
        public string ProductServiceID05 { get; set; }
        //public string ProductServiceIDQualifier06 { get; set; }
        //public string ProductServiceID07 { get; set; }
        //public string ProductServiceIDQualifier08 { get; set; }
        //public string ProductServiceID09 { get; set; }
        //public string ProductServiceIDQualifier10 { get; set; }
        public UnitDetail UnitDetail { get; set; }
        public ReferenceNumbers ReferenceNumbers { get; set; }
        public AdministrativeCommunicationContact AdministrativeCommunicationContact { get; set; }
        public List<ForecastSchedule> ForecastSchedule { get; set; }
        public List<ResourceAuthorization> ResourceAuthorization { get; set; }
        public List<ShippedReceivedInformation> ShippedReceivedInformation { get; set; }

        public ItemIdentification(object[] row)
        {
            AssignedIdentification = row[1].ToString();
            ProductServiceIDQualifier = row[2].ToString();
            ProductServiceID = row[3].ToString();
            ProductServiceIDQualifier04 = row[4].ToString();
            ProductServiceID05 = row[5].ToString();
        }
    }
}
