namespace Test.Inbound.Segments
{
    public class Name
    {
        public string EntityIdentifierCode { get; set; }
        public string Name02 { get; set; }
        public string IdentificationCodeQualifier { get; set; }
        public string IdentificationCode { get; set; }
        //public AdditionalNameInformation AdditionalNameInformation { get; set; }
        //public AddressInformation AddressInformation { get; set; }
        //public GeographicLocation GeographicLocation { get; set; }

        public Name(object[] row)
        {
            EntityIdentifierCode = row[1].ToString();
            Name02 = row[2].ToString();
            IdentificationCodeQualifier = row[3].ToString();
            IdentificationCode = row[4].ToString();
        }
    }
}
