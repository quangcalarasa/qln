using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Dto
{
    public class DebtsTableReq
    {
        public string Code { get; set; }
        public List<DebtsTable> data { get; set; }
    }
}
