using Microsoft.AspNetCore.SignalR;

namespace UploadThing.Example.AspNetCoreMVC
{
    public class AlertHub : Hub
    {
        public async Task ShowAlert()
        {
            await Clients.All.SendAsync("ReceiveAlert", "Ping Ping...");
        }
    }
}