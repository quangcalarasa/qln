using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOITQln.Common.Bases
{
    public class AbstractEntity<TId> : BaseEntity<TId>
    {
        public virtual long? CreatedById { get; set; }
        public virtual long? UpdatedById { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string UpdatedBy { get; set; }
    }

    public class AbstractEntity : BaseEntity<int>
    {
        public virtual long? CreatedById { get; set; }
        public virtual long? UpdatedById { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string UpdatedBy { get; set; }
    }
}
