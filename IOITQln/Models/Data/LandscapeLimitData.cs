using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class LandscapeLimitData : LandscapeLimit
    {
        public List<LandscapeLimitItem> landscapeLimitItems { get; set; }
    }
}
