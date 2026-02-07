using MongoDB.Driver;
using OmniChat.Domain.Entities;
using OmniChat.Domain.Enums;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Shared.DTOs;

namespace OmniChat.Application.Services;

public class RegisterOrganizationUseCase
{
    private readonly MongoDbContext _db;

    public RegisterOrganizationUseCase(MongoDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(RegisterDto input)
    {
        // 1. Identificar o Plano escolhido
        var plan = await _db.Plans
            .Find(p => p.Name == input.PlanName)
            .FirstOrDefaultAsync();

        if (plan == null) throw new Exception("Plano não encontrado. Certifique-se de ter rodado o Seed.");

        // 2. Criar Organização
        var org = new Organization(input.CompanyName);
        org.Subscription.PlanId = plan.Id;
        org.Subscription.Status = SubscriptionStatus.Trialing;
        org.Subscription.TrialEndsAt = DateTime.UtcNow.AddDays(plan.TrialDays);

        // 3. Criar Usuário Admin
        var adminUser = new User(input.AdminPhone);
        
        adminUser.Email = input.AdminEmail; 
        adminUser.PasswordHash = input.Password;

        adminUser.OrganizationId = org.Id; 
        adminUser.Role = UserRole.OrganizationAdmin;
        org.MemberIds.Add(adminUser.Id);
        
        await _db.Organizations.InsertOneAsync(org);
        await _db.Users.InsertOneAsync(adminUser);
    }
}