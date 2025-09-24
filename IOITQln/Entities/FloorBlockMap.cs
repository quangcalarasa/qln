using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class FloorBlockMap : AbstractEntity<int>
    {
        public int BlockId { get; set; }
        public int FloorId { get; set; }
    }
}
