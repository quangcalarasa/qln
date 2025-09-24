using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class TdcCustomerFile : AbstractEntity<int>
    {
        public int TdcCustomerId { get; set; }
        public string FileName { get; set; }
        public string Note { get; set; }
        public string File { get; set; }

    }
}
