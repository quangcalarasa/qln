using IOITQln.QuickPriceNOC.Interface;
using FcmSharp;
using FcmSharp.Requests;
using FcmSharp.Settings;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOITQln.Common.Services;
using IOITQln.Persistence;
using static IOITQln.Common.Enums.AppEnums;
using static IOITQln.Entities.Md167House;
using IOITQln.Migrations;
using IOITQln.Entities;

namespace IOITQln.QuickPriceNOC.Service
{
    public class CheckStatusMd167House : ICheckStatusMd167House
    {
        private static readonly ILog log = LogMaster.GetLogger("CheckStatusMd167House", "CheckStatusMd167House");
        private readonly ApiDbContext _context;
        public CheckStatusMd167House(ApiDbContext context)
        {
            _context=context;
        }
        #region Note for status of use
        // 1 = Đã cho thuê
        // 2 = Cho thuê 1 phần
        // 3 = Chưa cho thuê
        public enum StateOfUseType
        {
            DA_CHO_THUE = 1,
            CHO_THUE_1_PHAN = 2,
            CHUA_CHO_THUE = 3,
        }
        #endregion
        public StateOfUseType CheckStatus(int id,List<Md167House> md167Houses,List<Md167Contract> md167Contracts)
        {
            var input = md167Houses.Where(x=>x.Id==id).FirstOrDefault();
            if (input.TypeHouse == Type_House.Kios)
            {
                var contract = md167Contracts.Where(x => x.HouseId == input.Id && x.ContractStatus == ContractStatus167.CON_HIEU_LUC).FirstOrDefault();
                if (contract != null)
                {
                    return StateOfUseType.DA_CHO_THUE;
                }
                input.InfoValue.KiosStatus = Kios_Status.CHUA_CHO_THUE;
                _context.Update(input);
                _context.SaveChanges();
                return StateOfUseType.CHUA_CHO_THUE;
            }
            else if (input.TypeHouse == Type_House.Apartment)
            {
                var lstKios = md167Houses.Where(x => x.Md167HouseId == input.Id && x.TypeHouse == Type_House.Kios ).ToList();
                if (lstKios.Count() == 0)
                {
                    input.StatusOfUse = 3;
                    _context.Update(input);
                    _context.SaveChanges();
                    return StateOfUseType.CHUA_CHO_THUE;
                }
                int k = 0;
                foreach (var item in lstKios)
                {
                    var contract = md167Contracts.Where(x => x.HouseId == item.Id && x.ContractStatus == ContractStatus167.CON_HIEU_LUC).FirstOrDefault();
                    if (contract != null)
                    {
                        k++;
                    }
                }
                if (k == 0)
                {
                    input.StatusOfUse = 3;
                    _context.Update(input);
                    _context.SaveChanges();
                    return StateOfUseType.CHUA_CHO_THUE;
                }
                else if (k == lstKios.Count())
                {
                    input.StatusOfUse = 1;
                    _context.Update(input);
                    _context.SaveChanges();
                    return StateOfUseType.DA_CHO_THUE;
                }
                else
                {
                    input.StatusOfUse = 2;
                    _context.Update(input);
                    _context.SaveChanges();
                    return StateOfUseType.CHO_THUE_1_PHAN;
                }

            }
            else
            {
                var contract = md167Contracts.Where(x => x.HouseId == input.Id && x.ContractStatus == ContractStatus167.CON_HIEU_LUC).FirstOrDefault();
                if (contract != null)
                {
                    return StateOfUseType.DA_CHO_THUE;

                }
                input.StatusOfUse = 3;
                _context.Update(input);
                return StateOfUseType.CHUA_CHO_THUE;
            }
        }
    }
}
