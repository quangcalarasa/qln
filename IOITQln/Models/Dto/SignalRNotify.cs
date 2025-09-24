using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Dto
{
    public class SignalRNotify
    {
        public string Title { get; set; }
        public string Contents { get; set; }
        public TypeSignalRNotify Type { get; set; }

        public SignalRNotify(string title, string contents, TypeSignalRNotify type)
        {
            Title = title;
            Contents = contents;
            Type = type;
        }
    }
}
