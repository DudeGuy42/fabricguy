using GuyRemoteServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GuyStatelessWebCore.Hubs
{
    public class GuyHub: Hub
    {
        IGuyStatefulServiceCoreInterface _guyService;
        public GuyHub()
        { 
            _guyService = ServiceProxy.Create<IGuyStatefulServiceCoreInterface>(new Uri("fabric:/GuyFabric/GuyStatefulServiceCore"), new ServicePartitionKey(1), listenerName: "ServiceEndpointV2");
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await _guyService.CreateGuy(Context.ConnectionId);
            await Clients.All.SendAsync("connected", $"new connection: {this.Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("disconnected", $"{this.Context.ConnectionId} disconnected.");
        }

        public async Task HubInfo()
        {
            try
            {
                var guys = await _guyService.GetGuys();
                await Clients.Caller.SendAsync("receiveInfo", $"ConnectionId: {this.Context.ConnectionId}\nGuysData: {guys}");
            }
            catch(Exception ex)
            {
                await Clients.Caller.SendAsync("receiveInfo", $"Server Exception: {ex}");

                Console.WriteLine($"{ex}");
            }
        }

        public async IAsyncEnumerable<int> Counter()
        {
            await HubInfo();
            int i = 0;
            while(true)
            { 
                // Check the cancellation token regularly so that the server will stop
                // producing items if the client disconnects.
                //cancellationToken.ThrowIfCancellationRequested();

                yield return i++;

                // Use the cancellationToken in other APIs that accept cancellation
                // tokens so the cancellation can flow down to them.
                await Task.Delay(1000);
            }
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
