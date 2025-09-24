using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class WardManagement : AbstractEntity<long>
    {
        public int WardId { get; set; }
        public string WardName { get; set; }
        public int UserId { get; set; }
    }
}
