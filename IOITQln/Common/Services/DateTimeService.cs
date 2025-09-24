using IOITQln.Common.Interfaces.Helpers;
using System;

namespace IOITQln.Common.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now => DateTime.Now;
    }
}
