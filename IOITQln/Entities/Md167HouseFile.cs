

// IOITQln.Entities.Md167HouseFile
using System;
using System.Runtime.CompilerServices;
using IOITQln.Common.Bases;

public class Md167HouseFile : AbstractEntity<int>
{
    public int Md167HouseId
    {
        get;
        set;
    }

    public DateTime? BrowseDate
    {
        get;
        set;
    }

    public DateTime? ReceiptDate
    {
        get;
        set;
    }

    public string? Unit
    {
       
        get;
   
        set;
    }

    public string? DocNumber
    {
       
        get;

        set;
    }

    public string NameFile
    {
        get;
        set;
    }

    public string AttachedFiles
    {
        get;
        set;
    }

    public string Notefile
    {
        get;
        set;
    }
}
