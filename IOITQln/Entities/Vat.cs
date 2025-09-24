using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class Vat : AbstractEntity<int>
    {
        public DateTime DoApply { get; set; }        //Ngày hiệu lực
        public double Value { get; set; }    //Giá trị
        public string Note { get; set; }    //Diễn giải
    }
}
