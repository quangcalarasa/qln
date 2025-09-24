using IOITQln.Common.Bases;
using System;


namespace IOITQln.Entities
{
    public class ExtraPostingConfiguration : AbstractEntity<int>
    {
        public int CapacityImg { get; set; } //Dung lượng ảnh
        public int CapacityVideo { get; set; }  //Dung lượng video
        public int CapacityFile { get; set; }  //Dung lượng file
    }

}
