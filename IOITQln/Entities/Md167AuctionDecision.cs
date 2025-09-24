using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class Md167AuctionDecision : AbstractEntity<long>
    {
        public int Md167ContractId { get; set; }
        public string Decision { get; set; }
        public string AuctionUnit { get; set; }
        public decimal Price { get; set; }
        public DateTime DateEffect { get; set; }
    }
}
