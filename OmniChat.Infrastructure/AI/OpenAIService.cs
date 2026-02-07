using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using OmniChat.Domain.Interfaces;

namespace OmniChat.Infrastructure.AI;

public class OpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "ChatGPT";

    public OpenAIService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["AI:OpenAI:ApiKey"];
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
    }

    // Implementação da Interface V2
    public async Task<string> GenerateResponseAsync(List<(string Role, string Content)> history)
    {
        var messages = new List<object>
        {
            new { role = "system", content = "Você é um assistente útil e profissional." }
        };

        // Converte o histórico de tuplas para o formato da OpenAI
        messages.AddRange(history.Select(h => new { role = h.Role.ToLower(), content = h.Content }));

        var payload = new
        {
            model = "gpt-4-turbo", // ou gpt-3.5-turbo dependendo da config
            messages = messages
        };

        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", payload);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        return result.choices[0].message.content;
    }

    public async Task<string> GetResponseAsync(string userMessage, string contextId)
    {
        // Cria um histórico temporário contendo apenas a mensagem atual
        var simpleHistory = new List<(string Role, string Content)>
        {
            ("User", userMessage)
        };

        // Reutiliza a lógica robusta do GenerateResponseAsync
        return await GenerateResponseAsync(simpleHistory);
    }
}