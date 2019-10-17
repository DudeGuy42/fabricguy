# ACTORS, SERVICES, AND BUGS

.NET Core (using dotnet cli) versions of Actor projects do not work well. I avoid them at all cost. In other words, I only use the full .Net Framework versions of Fabric Applications here.

My goal is to set up a stateful service that just invokes "Ticks" on "GuyActors", where GuyActors are super simple sims that have Hunger and Tiredness. End-Users can opt to feed them, or put them to sleep, in which case their hunger and sleep counters just reset to zero. This is meant to flex my architecting muscles a bit and suss out benefits and an understanding of the Distributed Actor "paradigm".

I want as many "guys" doing things as fast as possible; then I want to see how the system scales and what the cost is for such a thing. This information will be used to determine the feasibility of some other experiments I'd like to perform.

## Progress Notes
- Cant use 'async streams' in my ASPNetCore GuyWeb because is a C# 8 feature, and because the compiler is selected based on the framework version being used, I can't force it. However, I can't use the newer framework versions because they don't compile without errors (right out of the box). I might have to debug the error of something that should "just work". Alternatively, I can do it another way where I do not need the async streams (I think ChannelReader?). Information from https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version. **Elected Solution:** Added a new GuyWebCore project. I am going to use the .Net Core 3 version that was released just last month. Lets see how it goes. (Updates) It causes a lot of problems. **Final Solution:** It just seems that the templates that Visual Studio creates projects with are out of data, and so from the template a bunch of amendments need to be made to have a cleaner experience. After cleaning up the problems things started to fall in to place.

- I switched to .Net Core 3 for GuyWebCore. There are two issues here now. Mapping the Index URL to http://{MachineName}:{Port}/, and deploying to {Port} in a consistent manner so that I can launch with Ctrl-F5. Arguably the second is less important...If you're not a developer. I am, and so it is maybe most important. I found this: https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-tutorial-dotnet-app-enable-https-endpoint which applies to httpS(!) instead of http, however I am hoping it is applicable. Note: Https may be more desirable but I do not want to mess with the issue of certificates right now. Adding the Kestrel listener to listen to the endpoint from the ServiceManifest seems to have taken me a little further, but the actual file contents are not posting (though the URLs aren't failing out now either.). I found this: https://andrewlock.net/comparing-startup-between-the-asp-net-core-3-templates/; within it there are links to changes in ASP.Net Core 3 vs ASP.Net Core 2.2. It seems to talk about the differences between UseMVC and the Endpoint routing (things I know nothing about).

Also; creating stateless webapps seems to be the only way to get the CSProject files to configure appropriately? The only working template? Or perhaps I am misunderstanding the intentention of stateful web services?

- I've restarted. I updated my Visual Studio and all tooling. I find that I still can't reliably stand up stateful ASP.Net Core services (it might be a configuration issue; but I am having a laggy connection right now and don't want to dig in to that at the moment; I may if this other approach is blocking). I've dropped the SQL back-end and I am intending to use stateful services with reliable collections instead. My ultimate question with this perspective is: Can my stateless web service still get to the reliable collections within my Fabric? Probably, right? Dunno.

- In this new project, my GuyActorCore has a build error right away. It seems my Actor service is still using .netcore2.0 when looking at the path of the targets file in VS Build window. Updating the package within the ActorCore project (again, a broken template it seems) seemed to have fixed that build issue. For now, the project launches via ctrl-f5 and everything seems nominal. Back to developer work.
- In preparation for reliable collections work; thing in terms of transactions - there appear to be a lot of potential pitfalls so read carefully: 
https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-work-with-reliable-collections
- In preparation for Actor work - review https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-actors-introduction

- I think I got the jist of how the SignalR Hubs work (and they do seem to work well, and 'as expected'). Streaming is still an open question. Ideally I have a stream that is constant, however all of the streams I see at the moment seem to have a finite number of elements being transferred. I'll have to do further research on this.


- Next is testing of service remoting. https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-communication-remoting; this is wired up but I think that there is a mismatch between adding listeners via the servicemanifest and via code in the CreateServiceReplicaListeners(). In the meantime, however, I have it to the point where a SignalR Hub can RPC on the stateful service core. 

## At the moment I have the following components:
### GuyStatefulServiceCore
The main service that pumps the GuyActors for events.

#### TODO: 
- Read numerical value N from a configuration script.
- Spawn N GuyActors
- Invoke Tick() on actors; use parallel strategies to invoke the Tick() on the actors.
- Occasionally invoke UpdateMainState() as this will pump data in to the SQL database so we can measure the impact it has on such things.

### GuyActorCore
Distributed agents that operate on GuyStates and "simulate" individual "Guy Sims"

#### TODO:
- Add Spawn() method to GuyActor interface.
- Wire up SQL database; namely when an actor is activated it should open a connection to the sql database
- Also when an actor is activated, retrieve any state that may exist in the sql database (run a query, if there is a response then restore the state)
- When UpdateMainState is invoked, save GuyState data to database.
- Ensure that Tick/Eat/Sleep methods are correct (for whatever reason I wrote these out already).

### GuyStatelessWebCore
The website for viewing information about the other components.

#### TODO: 
- Add visualizations for GuyStates that are retrieved from SQL. 
- In these visualizations be able to Sleep/Eat on each of the GuyStates
- When Sleep/Eat is invoked, a controller should invoke/pass a message to the appropriate Actor.
- **TODO RAMPUP:** Updates will need to be streamed (probably) with a SignalR Hub.
- **TODO:** Streaming SignalR Hub : https://docs.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-3.0
- **TODO:** Vanilla JavaScript Client

### **FUTURE:** AngularGuyClient
- Angular client that uses SignalR to communicate with the web server.

### **FUTURE:** ReactGuyClient
- Possibly in the future to demonstrate familiarity with React.