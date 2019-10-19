using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuyRemoteServices;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GuyActorCore.Interfaces;
using Microsoft.ServiceFabric.Actors;

namespace GuyStatefulServiceCore
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class GuyStatefulServiceCore : StatefulService, IGuyStatefulServiceCoreInterface
    {
        const string GUY_DICTIONARY = "guy_dictionary";
        const string GUY_ACTOR_URI = "fabric:/GuyFabric/GuyActorCore";


        public GuyStatefulServiceCore(StatefulServiceContext context)
            : base(context)
        { 
        }

        public async Task SpawnGuy(string name)
        {
            // TODO: Will this work??
            var guyDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, IGuyActorCore>>(GUY_DICTIONARY);
            
            using (var tx = this.StateManager.CreateTransaction())
            {
                // creates an actor proxy with the same name as the guy.
                await guyDictionary.AddAsync(tx, name, ActorProxy.Create<IGuyActorCore>(new ActorId(name), new Uri(GUY_ACTOR_URI)));
                
                await tx.CommitAsync();
            }
        }

        public async Task<string> RetrieveGuys()
        {
            var guyDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, IGuyActorCore>>(GUY_DICTIONARY);
            using (var tx = this.StateManager.CreateTransaction())
            {
                var collection = await guyDictionary.CreateEnumerableAsync(tx, EnumerationMode.Unordered);
                
                await foreach(var item in collection)
                {
                    
                }
            }

                await Task.Delay(1);
                // TODO - return a json serialized collection of guy states
            return "TODO: Return Guys collection from GuyStatefulServiceCore.";
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new []
            {
                new ServiceReplicaListener(context => new FabricTransportServiceRemotingListener(context, this), name: "ServiceEndpointV2")
            };
            //return new[] { new ServiceInstanceListener(context => new FabricTransportServiceRemotingListener(context, this)) };
            //return this.CreateServiceRemotingReplicaListeners();
            //return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            // todo: Make the GUY_DICTIONARY a <string, string> dictionary.
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>(GUY_DICTIONARY);
            
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
