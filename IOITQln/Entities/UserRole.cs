using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class UserRole : AbstractEntity<int>
    {
        public long UserId { get; set; }
        public int RoleId { get; set; }
    }
}
