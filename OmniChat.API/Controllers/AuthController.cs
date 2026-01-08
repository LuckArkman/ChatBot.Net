using Microsoft.AspNetCore.Mvc;
using OmniChat.Application.Services;
using OmniChat.Infrastructure.Repositories;
using OmniChat.Infrastructure.Security;
using OmniChat.Shared.DTOs;

namespace OmniChat.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserRepository _userRepo;
    private readonly AuthService _authService;
    private readonly RegisterOrganizationUseCase _registerUseCase; // Nova injeção

    public AuthController(
        UserRepository userRepo, 
        AuthService authService,
        RegisterOrganizationUseCase registerUseCase)
    {
        _userRepo = userRepo;
        _authService = authService;
        _registerUseCase = registerUseCase;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        var user = await _userRepo.GetByEmailAsync(login.Username);
        if (user == null) return Unauthorized("Usuário não encontrado.");

        if (!_authService.VerifyPassword(login.Password, user.PasswordHash))
            return Unauthorized("Senha incorreta.");

        var token = _authService.GenerateJwtToken(user, user.Role.ToString(), user.OrganizationId);

        return Ok(new LoginResultDto 
        { 
            Token = token, 
            Role = user.Role.ToString(),
            Expiration = DateTime.UtcNow.AddHours(8)
        });
    }

    // --- NOVO ENDPOINT DE CADASTRO ---
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            // O UseCase cuida da criação da Org, User e Hash da senha
            // Nota: Certifique-se que o RegisterOrganizationUseCase usa o AuthService.HashPassword
            // ou injete o AuthService lá. Para este exemplo, assumimos que o DTO chega pronto.
            
            // Dica de segurança: Hash a senha aqui se o UseCase não estiver fazendo
            dto.Password = _authService.HashPassword(dto.Password);

            await _registerUseCase.ExecuteAsync(dto);
            return Ok(new { Message = "Conta criada com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}