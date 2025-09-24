using System.Collections.Generic;

namespace IOITQln.Common.ViewModels.Common
{
    public class NzTreeResponse
    {
        public int key { get; set; }
        public string title { get; set; }
        public bool expanded { get; set; }
        public bool isLeaf { get; set; }
        public int? floorCode { get; set; }
        public bool? isMezzanine { get; set; }
        public List<NzTreeResponse> children { get; set; }
    }
}
