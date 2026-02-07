using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace OmniChat.Client.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string token = null;

        try
        {
            // Tenta ler o token do LocalStorage.
            // Se estivermos na fase de pré-renderização (Server-side), isso lançará uma exceção
            // porque o JavaScript ainda não está disponível no navegador.
            token = await _localStorage.GetItemAsync<string>("authToken");
        }
        catch (InvalidOperationException) 
        {
            // Ignora o erro de JS Interop durante a pré-renderização.
            // Retorna estado anônimo para permitir que o HTML seja gerado.
        }
        catch (Exception)
        {
            // Fallback para outros erros
        }

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        // Se encontrou o token, configura o HttpClient
        _http.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Retorna o usuário autenticado
        return new AuthenticationState(new ClaimsPrincipal(ParseClaimsFromJwt(token)));
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(ParseClaimsFromJwt(token));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private ClaimsIdentity ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return new ClaimsIdentity(token.Claims, "jwt");
    }
}