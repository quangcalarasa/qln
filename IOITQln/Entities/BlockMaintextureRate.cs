using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class BlockMaintextureRate : AbstractEntity<long>    //Bảng tỷ lệ chất lượng còn lại của căn nhà
    {
        public int TargetId { get; set; }
        public int LevelBlockId { get; set; }
        public int RatioMainTextureId { get; set; }
        public TypeMainTexTure TypeMainTexTure { get; set; }
        public int CurrentStateMainTextureId { get; set; }
        public float RemainingRate { get; set; }
        public float MainRate { get; set; }
        public float? TotalValue { get; set; }
        public float? TotalValue1 { get; set; }
        public float? TotalValue2 { get; set; }
        public TypeBlockMaintextureRate Type { get; set; }
    }
}
