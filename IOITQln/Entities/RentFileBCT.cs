

// IOITQln.Entities.RentFileBCT
using System;
using IOITQln.Common.Bases;

public class RentFileBCT : AbstractEntity<int>
{
    public Guid RentFileId
    {
        get;
        set;
    }

    public string CodeHd
    {
        get;
        set;
    }

    public string Code
    {
        get;
        set;
    }

    public string CustomerName
    {
        get;
        set;
    }

    public DateTime? DateStart
    {
        get;
        set;
    }

    public DateTime? DateEnd
    {
        get;
        set;
    }

    public decimal? Area
    {
        get;
        set;
    }

    public byte TypeBCT
    {
        get;
        set;
    }

    public int DiscountCoefficientId
    {
        get;
        set;
    }
}
