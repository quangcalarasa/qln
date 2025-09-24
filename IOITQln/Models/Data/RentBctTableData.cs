using Castle.Core;
using IOITQln.Entities;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class RentBctTableData : RentBctTable
    {
       public List<ChilDfTable> chilDfs { get; set; }
    }
}
