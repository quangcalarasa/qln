using IOITQln.Entities;
using System;
using System.Collections.Generic;

namespace IOITQln.Models.Dto
{
    public class ImportDto
    {
        public DateTime  DoApply { get; set; }
        public double Value { get; set; }
    }

    public class rentingPrice
    {
        public int? LevelId { get; set; } // cấp nhà
        public double? Price { get; set; }
    }
}
