using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Projet__E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiAssistantController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AiAssistantController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
                return BadRequest("Message cannot be empty");

            try
            {
                var client = _httpClientFactory.CreateClient();
                
                // Get webhook URL from configuration
                var baseUrl = _configuration["N8n:WebhookUrl"] ?? "https://shopitri.app.n8n.cloud/webhook/3dc65d23-466f-47a6-828e-e3f4f5c4e0fd";
                var sessionId = HttpContext.Session.Id;
                var webhookUrl = $"{baseUrl}?sessionId={sessionId}";

                // Force session initialization to get a stable ID
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("_SessionStarted")))
                {
                    HttpContext.Session.SetString("_SessionStarted", "true");
                }
                await HttpContext.Session.CommitAsync();

                var n8nRequest = new
                {
                    chatInput = request.Message,
                    query = request.Message, // Some n8n nodes expect 'query' by default
                    sessionId = HttpContext.Session.Id
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(n8nRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(webhookUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    
                    try 
                    {
                        // Try to parse as JSON to ensure it's valid for the frontend fetch()
                        using var doc = JsonDocument.Parse(responseData);
                        return Ok(responseData);
                    }
                    catch
                    {
                        // If not JSON (e.g. plain text from n8n), wrap it in a JSON object
                        return Ok(JsonSerializer.Serialize(new { output = responseData }));
                    }
                }

                return StatusCode((int)response.StatusCode, "Error communicating with AI Assistant");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
