

// IOITQln.Models.Dto.Md167HouseReqData
using System;
using System.Collections.Generic;
using IOITQln.Common.Enums;
using IOITQln.Entities;
using IOITQln.Models.Dto;

public class Md167HouseReqData
{
    public int Id
    {
        get;
        set;
    }

    public string Code
    {
        get;
        set;
    }

    public string HouseNumber
    {
        get;
        set;
    }

    public string DocumentNb
    {
        get;
        set;
    }

    public int ProvinceId
    {
        get;
        set;
    }

    public int DistrictId
    {
        get;
        set;
    }

    public int HouseTypeId
    {
        get;
        set;
    }

    public int WardId
    {
        get;
        set;
    }

    public int LaneId
    {
        get;
        set;
    }

    public string MapNumber
    {
        get;
        set;
    }

    public string ParcelNumber
    {
        get;
        set;
    }

    public int? LandTaxRateId
    {
        get;
        set;
    }

    public decimal LandTaxRate
    {
        get;
        set;
    }

    public string PlanningInfor
    {
        get;
        set;
    }

    public int LandId
    {
        get;
        set;
    }

    public int Md167TransferUnitId
    {
        get;
        set;
    }

    public DateTime ReceptionDate
    {
        get;
        set;
    }

    public AppEnums.DecreeEnum decree
    {
        get;
        set;
    }

    public int Location
    {
        get;
        set;
    }

    public string LocationCoefficient
    {
        get;
        set;
    }

    public decimal UnitPriceValue
    {
        get;
        set;
    }

    public decimal LandPrice
    {
        get;
        set;
    }

    public string UnitPrice
    {
        get;
        set;
    }

    public string SHNNCode
    {
        get;
        set;
    }

    public DateTime SHNNDate
    {
        get;
        set;
    }

    public string ContractCode
    {
        get;
        set;
    }

    public DateTime ContractDate
    {
        get;
        set;
    }

    public string LeaseCode
    {
        get;
        set;
    }

    public bool IsPayTax
    {
        get;
        set;
    }

    public DateTime LeaseDate
    {
        get;
        set;
    }

    public string TextureScale
    {
        get;
        set;
    }

    public string LeaseCertCode
    {
        get;
        set;
    }

    public DateTime LeaseCertDate
    {
        get;
        set;
    }

    public int Md167HouseId
    {
        get;
        set;
    }

    public int PurposeUsing
    {
        get;
        set;
    }

    public string DocumentCode
    {
        get;
        set;
    }

    public DateTime DocumentDate
    {
        get;
        set;
    }

    public int PlanContent
    {
        get;
        set;
    }

    public decimal OriginPrice
    {
        get;
        set;
    }

    public decimal ValueLand
    {
        get;
        set;
    }

    public Md167House.Type_House TypeHouse
    {
        get;
        set;
    }

    public int StatusOfUse
    {
        get;
        set;
    }

    public string Note
    {
        get;
        set;
    }

    public decimal? HouAreaLand
    {
        get;
        set;
    }

    public decimal? TaxNN
    {
        get;
        set;
    }

    public decimal? UseFloorPb
    {
        get;
        set;
    }

    public decimal? UseFloorPr
    {
        get;
        set;
    }

    public decimal? UseLandPb
    {
        get;
        set;
    }

    public decimal? UseLandPr
    {
        get;
        set;
    }

    public decimal? AreBuildPb
    {
        get;
        set;
    }

    public decimal? AreBuildPr
    {
        get;
        set;
    }

    public decimal? AreaLandInSafe
    {
        get;
        set;
    }

    public decimal? AreaLandInBankSafe
    {
        get;
        set;
    }

    public decimal? AreaHouseInSafe
    {
        get;
        set;
    }

    public decimal? AreaHouseInBankSafe
    {
        get;
        set;
    }

    public int? ApaFloorCount
    {
        get;
        set;
    }

    public decimal? ApaTax
    {
        get;
        set;
    }

    public bool ApaIsBasement
    {
        get;
        set;
    }

    public decimal? ApaValue
    {
        get;
        set;
    }

    public Md167House.Kios_Status? KiosStatus
    {
        get;
        set;
    }

    public decimal? AreaFloorBuild
    {
        get;
        set;
    }

    public decimal? AreaTunnel
    {
        get;
        set;
    }

    public int AreaValueId
    {
        get;
        set;
    }

    public int? LandPriceItemId
    {
        get;
        set;
    }

    public List<Md167HousePropose> md167HouseProposes
    {
        get;
        set;
    }

    public List<Md167HouseInfo> md167HouseInfos
    {
        get;
        set;
    }

    public List<Md167KiosReqData> md167Kios
    {
        get;
        set;
    }
}
