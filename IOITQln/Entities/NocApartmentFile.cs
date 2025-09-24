using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class NocApartmentFile : AbstractEntity<int>
    {
        public int NocApartmentId { get; set; }
        public string NameFile { get; set; } // tên file
        public string AttachedFiles { get; set; } // file đính kèm
        public string Note { get; set; }
    }
}
