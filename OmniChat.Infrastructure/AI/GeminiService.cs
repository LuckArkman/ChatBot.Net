using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using OmniChat.Domain.Interfaces;

namespace OmniChat.Infrastructure.AI;

public class GeminiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    public string ProviderName => "Gemini";

    public GeminiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["AI:Gemini:ApiKey"];
    }

    // Implementação da Interface V2
    public async Task<string> GenerateResponseAsync(List<(string Role, string Content)> history)
    {
        // Gemini tem formato diferente (parts/text).
        // Simplificação: Pegando apenas a última mensagem do usuário para este exemplo,
        // mas idealmente converteria todo o histórico para o formato "contents" do Google.
        
        var lastUserMessage = history.LastOrDefault(x => x.Role == "User").Content ?? "Olá";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";
        
        var payload = new 
        {
            contents = new[] { new { parts = new[] { new { text = lastUserMessage } } } }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        return result.candidates[0].content.parts[0].text;
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