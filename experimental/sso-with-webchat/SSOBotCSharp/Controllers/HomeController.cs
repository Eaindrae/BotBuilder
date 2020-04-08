using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using AuthTest.Models;

namespace AuthTest.Controllers
{
    public class HomeController : Controller
    {
        public IConfiguration Configuration { get; set; }

        public HomeController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var secret = Configuration.GetSection("ClientDirectLineSecret")?.Value;
            var endpoint = Configuration.GetSection("ClientDirectLineEndpoint")?.Value;
            var clientId = Configuration.GetSection("ClientId")?.Value;
            var tenantId = Configuration.GetSection("TenantId")?.Value;

            var userId = "dl_" + Guid.NewGuid().ToString();

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/tokens/generate");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secret);
            request.Headers.Add("Origin", "https://originbot.azurewebsites.net");
            // TrustedOrigins = new string[] { "http://localhost:4275", "https://githubauthbotprod.azurewebsites.net/" }
            request.Content = new StringContent(JsonConvert.SerializeObject(new { User = new { Id = userId } }),
                                    Encoding.UTF8,
                                    "application/json");

            var response = await client.SendAsync(request);
            string token = String.Empty;

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<DirectLineToken>(body).token;
            }

            var config = new ChatConfig()
            {
                Token = token,
                Domain = endpoint,
                UserId = userId,
                ClientId = clientId,
                TenantId = tenantId
            };
            return View(config);
        }

        public Task<IActionResult> New()
        {
            return Index();
        }

        public async Task<IActionResult> NoTrusted()
        {
            var secret = Configuration.GetSection("ClientDirectLineSecret")?.Value;
            var endpoint = Configuration.GetSection("ClientDirectLineEndpoint")?.Value;

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/tokens/generate");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secret);

            var response = await client.SendAsync(request);
            string token = String.Empty;

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<DirectLineToken>(body).token;
            }

            var config = new ChatConfig()
            {
                Token = token,
                Domain = endpoint,
                UserId = Guid.NewGuid().ToString()
            };
            return View(config);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DirectLineToken
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
    }
}
