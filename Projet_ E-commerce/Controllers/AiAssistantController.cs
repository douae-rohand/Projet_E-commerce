using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Projet__E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiAssistantController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly Projet__E_commerce.Data.ApplicationDbContext _db;

        public AiAssistantController(IHttpClientFactory httpClientFactory, IConfiguration configuration, Projet__E_commerce.Data.ApplicationDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _db = db;
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
                var baseUrl = _configuration["N8n:WebhookUrl"] ?? "http://localhost:5678/webhook/3dc65d23-466f-47a6-828e-e3f4f5c4e0fd";
                var sessionId = HttpContext.Session.Id;
                var webhookUrl = $"{baseUrl}?sessionId={sessionId}";

                // User Info
                var userId = HttpContext.Session.GetInt32("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");
                var userEmail = HttpContext.Session.GetString("UserEmail");
                string? userName = null;

                if (userId.HasValue)
                {
                    if (userRole == "CLIENT")
                    {
                        var profil = await _db.Clients.FirstOrDefaultAsync(c => c.id == userId.Value);
                        if (profil != null)
                        {
                            userName = $"{profil.prenom} {profil.nom}".Trim();
                        }
                    }
                    userName ??= userEmail?.Split('@')[0] ?? "Utilisateur";
                }

                // Force session initialization to get a stable ID
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("_SessionStarted")))
                {
                    HttpContext.Session.SetString("_SessionStarted", "true");
                }
                await HttpContext.Session.CommitAsync();

                var n8nRequest = new
                {
                    chatInput = request.Message,
                    query = request.Message,
                    sessionId = HttpContext.Session.Id,
                    user = new {
                        isLoggedIn = userId.HasValue,
                        name = userName,
                        role = userRole,
                        email = userEmail
                    }
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
                    
                    if (string.IsNullOrWhiteSpace(responseData))
                    {
                        return Ok(JsonSerializer.Serialize(new { output = "n8n returned an empty response. Check if your 'Respond to Webhook' node is reached and connected to the AI Agent." }));
                    }

                    try 
                    {
                        using var doc = JsonDocument.Parse(responseData);
                        return Ok(responseData);
                    }
                    catch
                    {
                        return Ok(JsonSerializer.Serialize(new { output = responseData }));
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"n8n Error: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                // In production, we need to know what's failing (Webhook URL, DB, etc.)
                return StatusCode(500, $"Assistant Controller Error: {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
