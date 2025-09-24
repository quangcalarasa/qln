using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class LevelBlockMap : AbstractEntity<int>
    {
        public int BlockId { get; set; }
        public int LevelId { get; set; }
    }
}
