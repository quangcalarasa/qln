using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Common.ViewModels.Common
{
    public class CascaderResponse
    {
        public int value { get; set; }
        public string label { get; set; }
        public bool isLeaf { get; set; }
        public List<CascaderResponse> children { get; set; }
    }
}
