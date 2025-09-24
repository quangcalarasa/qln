

// IOITQln.Entities.Md167HouseInfo
using System;
using IOITQln.Common.Bases;

public class Md167HouseInfo : AbstractEntity<int>
{
    public string Code
    {
        get;
        set;
    }

    public DateTime Date
    {
        get;
        set;
    }

    public string Content
    {
        get;
        set;
    }

    public int Md167HouseId
    {
        get;
        set;
    }

    public string AuthLetter
    {
        get;
        set;
    }

    public DateTime BrowseDate
    {
        get;
        set;
    }

    public string Unit
    {
        get;
        set;
    }
}
