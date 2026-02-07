using Microsoft.Extensions.DependencyInjection; // <--- 1. Resolve 'GetRequiredService'
using OmniChat.Domain.Entities;
using OmniChat.Domain.Interfaces;

namespace OmniChat.Infrastructure.AI;

public class AiFactory : IAIFactory
{
    private readonly IServiceProvider _serviceProvider;

    // 2. Construtor para injetar o ServiceProvider (Resolve o aviso de campo não inicializado)
    public AiFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAIService GetProvider(Plan plan)
    {
        // 3. Correção: Acessa 'Features.AllowGpt4' em vez de 'CanUseGPT4'
        // Adicionamos verificação de nulo para segurança
        if (plan.Features != null && plan.Features.AllowGpt4) 
        {
            return _serviceProvider.GetRequiredService<OpenAIService>();
        }
        
        // Se não tiver GPT-4, retorna Gemini (Padrão/Mais barato)
        return _serviceProvider.GetRequiredService<GeminiService>();
    }

    public IAIService GetFallbackProvider(Plan plan)
    {
        // Lógica de redundância: Se o principal falhar, tenta o outro.
        
        // Se o principal era GPT (AllowGpt4 == true), o fallback é Gemini
        if (plan.Features != null && plan.Features.AllowGpt4)
        {
            return _serviceProvider.GetRequiredService<GeminiService>();
        }
            
        // Se o principal era Gemini, o fallback é OpenAI (GPT-3.5 ou 4 dependendo da config interna)
        return _serviceProvider.GetRequiredService<OpenAIService>();
    }
}