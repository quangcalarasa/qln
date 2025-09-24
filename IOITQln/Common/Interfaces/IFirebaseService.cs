using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Common.Interfaces
{
    public interface IFirebaseService
    {
        Task sendNotification(string tokenKey, long? userId, int badge, int badgeTotal, string name, string contents, bool notificationSound, string returnUrl, string domain);
    }
}
