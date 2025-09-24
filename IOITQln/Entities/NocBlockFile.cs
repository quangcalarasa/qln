using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class NocBlockFile : AbstractEntity<int>
    {
        public int NocBlockId { get; set; }
        public string NameFile { get; set; } // tên file
        public string AttachedFiles { get; set; } // file đính kèm
        public string Note { get; set; } 
    }
}
