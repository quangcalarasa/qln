using System;
using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class Holiday : AbstractEntity<int>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
