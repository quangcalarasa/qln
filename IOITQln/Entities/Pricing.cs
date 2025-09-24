

// IOITQln.Entities.Pricing
using System;
using System.Runtime.CompilerServices;
using IOITQln.Common.Bases;
using IOITQln.Common.Enums;

public class Pricing : AbstractEntity<int>
{
    public AppEnums.TypeReportApply TypeReportApply
    {
        get;
        set;
    }

    public AppEnums.TermApply? TermApply
    {
        get;
        set;
    }

    public int BlockId
    {
        get;
        set;
    }

    public int ApartmentId
    {
        get;
        set;
    }

    public DateTime? DateCreate
    {
        get;
        set;
    }

    public string TimeUse
    {

        get;

        set;
    }

    public int? VatId
    {
        get;
        set;
    }

    public float? Vat
    {
        get;
        set;
    }

    public decimal? ApartmentPrice
    {
        get;
        set;
    }

    public decimal? ApartmentPriceReduced
    {
        get;
        set;
    }

    public decimal? ApartmentPriceRemaining
    {
        get;
        set;
    }

    public decimal? ApartmentPriceNoVat
    {
        get;
        set;
    }

    public decimal? ApartmentPriceVat
    {
        get;
        set;
    }

    public string ApartmentPriceReducedNote
    {
        get;

        set;
    }

    public decimal? LandPrice
    {
        get;
        set;
    }

    public int? DeductionLandMoneyId
    {
        get;
        set;
    }

    public float? DeductionLandMoneyValue
    {
        get;
        set;
    }

    public float? ConversionArea
    {
        get;
        set;
    }

    public decimal? LandPriceAfterReduced
    {
        get;
        set;
    }

    public decimal? TotalPrice
    {
        get;
        set;
    }

    public int? AreaCorrectionCoefficientId
    {
        get;
        set;
    }

    public double? AreaCorrectionCoefficientValue
    {
        get;
        set;
    }

    public int? FlatCoefficientId_99
    {
        get;
        set;
    }

    public float? FlatCoefficient_99
    {
        get;
        set;
    }

    public int? FlatCoefficientId_34
    {
        get;
        set;
    }

    public float? FlatCoefficient_34
    {
        get;
        set;
    }

    public int? FlatCoefficientId_61
    {
        get;
        set;
    }

    public float? FlatCoefficient_61
    {
        get;
        set;
    }

    public float? SellLandArea
    {
        get;
        set;
    }

    public string ProcessProfileCeCode
    {

        get;
        set;
    }

    public string? DOENationalOwnership
    {
        get;
        set;
    }

    public string? HomePayOrder
    {
        get;
        set;
    }

    public string? DecisionSellHouse
    {
        get;
        set;
    }

    public string? ROfLTheLeaseContract
    {
        get;
        set;
    }

    public string? PayAndSellHouseContract
    {
        get;
        set;
    }

    public string? ROfLHousePayAndSaleContract
    {
        get;
        set;
    }

    public string? OLCDepartmentConstruction
    {
        get;
        set;
    }
}
