using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuyStatelessWebCore.Hubs
{
    public class GuyHub: Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("connected", $"new connection: {this.Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("disconnected", $"{this.Context.ConnectionId} disconnected.");
        }

        public void HubInfo()
        {
            Clients.Caller.SendAsync("receiveInfo", $"ConnectionId: {this.Context.ConnectionId}");
        }

        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            for(int i = 0; i < 10; i++)
            {
                Clients.All.SendAsync("broadcastMessage", name, $"{i}::{message}");
            }
        }
    }
}
