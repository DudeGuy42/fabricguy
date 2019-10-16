using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GuyActorCore.Interfaces;
using System.Text.Json;

namespace GuyActorCore
{
    public struct GuyState
    {
        public int Hunger { get; set; }
        public int Sleep { get; set; }
    }
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class GuyActorCore : Actor, IGuyActorCore
    {
        const string GUY_STATE_NAME = "guy_state";

        /// <summary>
        /// Initializes a new instance of GuyActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public GuyActorCore(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task UpdateMainState()
        {
            var guyState = await GetInternalState();

            // TODO: Push data to DB; this is the "main state"
        }

        public async Task Tick()
        {
            var guyState = await GetInternalState();

            guyState.Hunger++;
            guyState.Sleep++;

            await SaveInternalState(guyState);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization
            await this.StateManager.TryAddStateAsync(GUY_STATE_NAME, JsonSerializer.Serialize(new GuyState { Hunger = 0, Sleep = 0 }));
        }

        private async Task<GuyState> GetInternalState()
        {
            return JsonSerializer.Deserialize<GuyState>(await this.StateManager.GetStateAsync<string>(GUY_STATE_NAME));
        }

        private async Task SaveInternalState(GuyState guyState)
        {
            await this.StateManager.AddOrUpdateStateAsync(GUY_STATE_NAME, JsonSerializer.Serialize(guyState),
                (str, state) => {
                    return JsonSerializer.Serialize(guyState);
                });
        }

        public async Task Eat()
        {
            var state = await GetInternalState();
            state.Hunger = 0;

            await SaveInternalState(state);
        }

        public async Task Sleep()
        {
            var state = await GetInternalState();
            state.Sleep = 0;

            await SaveInternalState(state);
        }
    }
}
