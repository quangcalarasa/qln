using IOITQln.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Common.Hub
{
    public interface IHubClient
    {
        Task BroadcastMessage(SignalRNotify signalRNotify);
    }
}
