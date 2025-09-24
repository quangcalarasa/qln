

// IOITQln.Models.Dto.Md167KiosResData
using System.Runtime.CompilerServices;
using IOITQln.Entities;

public class Md167KiosResData
{
    public int? Id
    {
        get;
        set;
    }

    public string? Code
    {
        get;
        set;
    }

    public bool IsPayTax
    {
        get;
        set;
    }

    public string? HouseNumber
    {
        get;
        set;
    }

    public int? ProvinceId
    {
        get;
        set;
    }

    public int? DistrictId
    {
        get;
        set;
    }

    public int? WardId
    {
        get;
        set;
    }

    public int? LaneId
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

    public Md167House.Kios_Status? KiosStatus
    {
        get;
        set;
    }

    public decimal? TaxNN
    {
        get;
        set;
    }

    public string Note
    {

        get;
        set;
    }
}
