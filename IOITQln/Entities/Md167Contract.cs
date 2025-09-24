using IOITQln.Common.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static IOITQln.Common.Enums.AppEnums;

using IOITQln.Common.Enums;

public class Md167Contract : AbstractEntity<int>
{
    public string Code
    {
        get;
        set;
    }

    public string ProfileCode
    {
        get;
        set;
    }

    public DateTime DateSign
    {
        get;
        set;
    }

    public string? CodeDoc
    {
        
        get;
        
        set;
    }

    public DateTime? DateSignDoc
    {
        get;
        set;
    }

    public int DelegateId
    {
        get;
        set;
    }

    public int HouseId
    {
        get;
        set;
    }

    public AppEnums.TypePriceContract167 TypePrice
    {
        get;
        set;
    }

    public AppEnums.RentalPeriodContract167 RentalPeriod
    {
        get;
        set;
    }

    public string NoteRentalPeriod
    {
        get;
        set;
    }

    public AppEnums.RentalPurposeContract167 RentalPurpose
    {
        get;
        set;
    }

    public string NoteRentalPurpose
    {
        get;
        set;
    }

    public AppEnums.PaymentPeriodContract167 PaymentPeriod
    {
        get;
        set;
    }

    public DateTime DateGroundHandover
    {
        get;
        set;
    }

    public AppEnums.ContractStatus167 ContractStatus
    {
        get;
        set;
    }
     //Qfix
    public DateTime? EndDate
    {
        get;
        set;
    }

    public string ContractExtension
    {
        get;
        set;
    }

    public string Liquidation
    {
        get;
        set;
    }

    public float? AllocationCoefficient
    {
        get;
        set;
    }

    public int? ParentId
    {
        get;
        set;
    }

    public AppEnums.Contract167Type Type
    {
        get;
        set;
    }

    public bool? PaidDeposit
    {
        get;
        set;
    }

    public bool? RefundPaidDeposit
    {
        get;
        set;
    }

    [NotMapped]
    public string[] contractExtensions
    {
        get
        {
            if (ContractExtension != null && ContractExtension != "")
            {
                return ContractExtension.Split(";").ToArray();
            }
            return new string[0];
        }
        set
        {
            if (value != null)
            {
                ContractExtension = string.Join(";", value);
            }
        }
    }

    [NotMapped]
    public string[] liquidations
    {
        get
        {
            if (Liquidation != null && Liquidation != "")
            {
                return Liquidation.Split(";").ToArray();
            }
            return new string[0];
        }
        set
        {
            if (value != null)
            {
                Liquidation = string.Join(";", value);
            }
        }
    }
}

