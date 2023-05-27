using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace UploadThing.Example.AspNetCoreMVC.Controllers
{
    [Route("api/utcallback")]
    [ApiController]
    public class CallbackController : Controller
    {
        private readonly IHubContext<AlertHub> hubContext;

        public CallbackController(IHubContext<AlertHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(string slug)
        {
            string jsonString;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                jsonString = await reader.ReadToEndAsync();
            }
            await hubContext.Clients.All.SendAsync(
                "ReceiveAlert",
                $"Slug: {slug}<br>Content:<br>{jsonString}"
            );
            return Ok();
        }
    }
}
