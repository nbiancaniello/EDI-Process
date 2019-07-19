using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Inbound.Segments
{
    public class ShipDeliveryPattern
    {
        public string ShipDeliveryCalendarPatternCode { get; set; }
        public string ShipDeliveryPatternTimeCode { get; set; }
        public ForecastSchedule ForecastSchedule { get; set; }
    }
}
