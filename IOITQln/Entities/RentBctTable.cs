

// IOITQln.Entities.RentBctTable
using System;
using IOITQln.Common.Bases;

public class RentBctTable : AbstractEntity<Guid>
{
    public string? AreaName
    {
        get;
        set;
    }

    public int? Level
    {
        get;
        set;
    }

    public float? PrivateArea
    {
        get;
        set;
    }

    public DateTime? DateCoefficient
    {
        get;
        set;
    }

    public decimal? StandardPrice
    {
        get;
        set;
    }

    public decimal? TotalK
    {
        get;
        set;
    }

    public decimal? Ktlcb
    {
        get;
        set;
    }

    public decimal? Ktdbt
    {
        get;
        set;
    }

    public decimal? PriceRent1m2
    {
        get;
        set;
    }

    public decimal? PriceRent
    {
        get;
        set;
    }

    public string? Unit
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

    public int? DateDiff
    {
        get;
        set;
    }

    public string? Note
    {
        get;
        set;
    }

    public double? VAT
    {
        get;
        set;
    }

    public decimal? PolicyReduction
    {
        get;
        set;
    }

    public bool? check
    {
        get;
        set;
    }

    public Guid? RentFileId
    {
        get;
        set;
    }

    public decimal? PriceVAT
    {
        get;
        set;
    }

    public int? MonthDiff
    {
        get;
        set;
    }

    public decimal? TotalPrice
    {
        get;
        set;
    }

    public decimal? PriceAfterDiscount
    {
        get;
        set;
    }

    public decimal? DiscountCoff
    {
        get;
        set;
    }

    public int? Type
    {
        get;
        set;
    }

    public int Index
    {
        get;
        set;
    }

    public string? QD
    {
        get;
        set;
    }
}
