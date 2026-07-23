using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AI;

using Microsoft.Extensions.Configuration;

namespace AgriculturePlatform.Infrastructure.Services
{
    public class AnthropicService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey; 
        private readonly string _model;

        public AnthropicService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["AnthropicApi:ApiKey"] ?? throw new ArgumentNullException("AnthropicApi:ApiKey configuration is missing");
            _model = configuration["AnthropicApi:Model"] ?? "claude-sonnet-4-6";
            var baseUrl = configuration["AnthropicApi:BaseUrl"] ?? "https://proxy.llm-gateway.ready.presidio.com/";

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetChatCompletionAsync(string message, IEnumerable<ChatMessage> history, string systemPrompt)
        {
            var messages = new List<object>();

            foreach (var msg in history)
            {
                messages.Add(new { role = "user", content = msg.Query });
                messages.Add(new { role = "assistant", content = msg.Response });
            }

            messages.Add(new { role = "user", content = message });

            var requestBody = new
            {
                model = _model,
                system = systemPrompt,
                max_tokens = 4096,
                messages = messages,
                temperature = 0.7
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/messages", jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Anthropic API Error: {response.StatusCode} - {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonDocument.Parse(responseJson);

            var answer = responseObject.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return answer ?? "Sorry, I could not generate a response.";
        }

        public async Task<string> AnalyzePlantImageAsync(string base64Image, string cropContext)
        {
            var systemPrompt = @"You are an expert plant pathologist and agronomist. 
Analyze the provided image of a plant/crop and identify any diseases, pests, or deficiencies.
You MUST output your response in valid JSON format matching this schema:
{
  ""diseaseName"": ""string (e.g., 'Powdery Mildew', 'Healthy', 'Unknown')"",
  ""category"": ""string (e.g., 'Fungal', 'Bacterial', 'Viral', 'Pest', 'Deficiency')"",
  ""severity"": ""string ('Low', 'Medium', 'High', 'None')"",
  ""confidenceScore"": number (0-100),
  ""symptoms"": [""string""],
  ""treatment"": [""string""],
  ""prevention"": [""string""],
  ""organicRemedies"": [""string""],
  ""additionalInfo"": ""string (any other context or notes)""
}";

            var userContent = new List<object>
            {
                new 
                { 
                    type = "image", 
                    source = new 
                    { 
                        type = "base64", 
                        media_type = "image/jpeg", 
                        data = base64Image 
                    } 
                },
                new 
                { 
                    type = "text", 
                    text = $"Please analyze this plant image. Context: {cropContext}. Ensure the output is valid JSON." 
                }
            };

            var requestBody = new
            {
                model = _model,
                system = systemPrompt,
                max_tokens = 4096,
                messages = new[]
                {
                    new { role = "user", content = userContent }
                },
                temperature = 0.2
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("v1/messages", jsonContent);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Anthropic Vision API Error: {response.StatusCode} - {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonDocument.Parse(responseJson);

            var answer = responseObject.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (answer != null)
            {
                answer = answer.Trim();
                if (answer.StartsWith("```"))
                {
                    var firstNewline = answer.IndexOf('\n');
                    var lastBackticks = answer.LastIndexOf("```");
                    if (firstNewline != -1 && lastBackticks != -1 && lastBackticks > firstNewline)
                    {
                        answer = answer.Substring(firstNewline + 1, lastBackticks - firstNewline - 1).Trim();
                    }
                }
            }

            return answer ?? "{}";
        }

        public async Task<string> AnalyzePlantImageAsTextAsync(string base64Image, string cropContext)
        {
            var systemPrompt = "You are an expert plant pathologist and agronomist. Analyze this plant image and describe any diseases, pests, or deficiencies in plain text.";

            var userContent = new List<object>
            {
                new 
                { 
                    type = "image", 
                    source = new 
                    { 
                        type = "base64", 
                        media_type = "image/jpeg", 
                        data = base64Image 
                    } 
                },
                new 
                { 
                    type = "text", 
                    text = $"Context: {cropContext}" 
                }
            };

            var requestBody = new
            {
                model = _model,
                system = systemPrompt,
                max_tokens = 2000,
                messages = new[]
                {
                    new { role = "user", content = userContent }
                },
                temperature = 0.5
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("v1/messages", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Anthropic Vision API Error: {response.StatusCode} - {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonDocument.Parse(responseJson);

            return responseObject.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "Could not analyze image.";
        }
    }
}
