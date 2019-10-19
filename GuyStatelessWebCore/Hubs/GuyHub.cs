using GuyActorCore.Interfaces;
using GuyRemoteServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
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
        IGuyStatefulServiceCoreInterface _mainServiceProxy;
        Dictionary<string, string> _connectionGuyDictionary = new Dictionary<string, string>();

        public GuyHub()
        { 
            _mainServiceProxy = ServiceProxy.Create<IGuyStatefulServiceCoreInterface>(new Uri("fabric:/GuyFabric/GuyStatefulServiceCore"), new ServicePartitionKey(1), listenerName: "ServiceEndpointV2");
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await _mainServiceProxy.CreateGuy(Context.ConnectionId);
            await Clients.All.SendAsync("connected", $"new connection: {this.Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("disconnected", $"{this.Context.ConnectionId} disconnected.");
        }

        private IGuyActorCore Guy()
        {
            return ActorProxy.Create<IGuyActorCore>(new ActorId(_connectionGuyDictionary[this.Context.ConnectionId]), new Uri("fabric:/GuyFabric/GuyActorCore"));
        }

        public async Task<bool> LoginAs(string name)
        {
            return _connectionGuyDictionary.TryAdd(this.Context.ConnectionId, name);
        }

        public async Task LogoutAs(string name)
        {
            if(this._connectionGuyDictionary.ContainsKey(this.Context.ConnectionId))
            {
                _connectionGuyDictionary.Remove(this.Context.ConnectionId);
            }
        }

        public async Task HubInfo()
        {
            try
            {
                var guys = await _mainServiceProxy.GetGuys();
                await Clients.Caller.SendAsync("receiveInfo", $"ConnectionId: {this.Context.ConnectionId}\nGuysData: {guys}");
            }
            catch(Exception ex)
            {
                await Clients.Caller.SendAsync("receiveInfo", $"Server Exception: {ex}");

                Console.WriteLine($"{ex}");
            }
        }

        /// <summary>
        /// The client has requested that its actor Eat.
        /// </summary>
        /// <returns></returns>
        public async Task Eat()
        {
            try
            {
                await Guy().Eat();
            }
            catch(Exception ex)
            {
                // Dunno.
                // Do something.
            }
        }

        /// <summary>
        /// The client has requested that its actor sleep.
        /// </summary>
        /// <returns></returns>
        public async Task Sleep()
        {
            try
            {
                await Guy().Sleep();
            }
            catch(Exception ex)
            {
                // Dunno. 
                // Do something.

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
