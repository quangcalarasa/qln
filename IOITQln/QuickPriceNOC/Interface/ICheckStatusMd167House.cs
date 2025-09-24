using IOITQln.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOITQln.QuickPriceNOC.Service.CheckStatusMd167House;

namespace IOITQln.QuickPriceNOC.Interface
{
    public interface ICheckStatusMd167House
    {
        StateOfUseType CheckStatus(int id, List<Md167House> md167Houses, List<Md167Contract> md167Contracts);
    }

}
