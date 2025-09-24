using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace IOITQln.Common.Hub
{
    public class BroadcastHub : Hub<IHubClient>
    {
        public Task SuscribeToUser(string userId)
        {
            return this.Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
