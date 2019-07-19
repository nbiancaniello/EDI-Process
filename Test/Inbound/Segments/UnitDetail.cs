using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class UnitDetail
    {
        public string UnitBasisForMeasurementCode { get; set; }

        public UnitDetail(object[] row)
        {
            UnitBasisForMeasurementCode = row[1].ToString();
        }
    }
}
