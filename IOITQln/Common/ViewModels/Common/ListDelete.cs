using System;
using System.Collections.Generic;
using System.Text;

namespace IOITQln.Common.ViewModels.Common
{
    public class ItemDelete<TId>
    {
        public virtual TId Id { get; set; }
        public bool? DeleteSuccess { get; set; }
        public string DeleteNote { get; set; }
    }
}
