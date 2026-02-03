using api.Core.Data;
using api.Core.Repositories;
using api.Modules.VerificationCodes.Enums;
using api.Modules.VerificationCodes.Interfaces.Repositories;
using api.Modules.VerificationCodes.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Modules.VerificationCodes.Repositories;

public class VerificationCodesRepository(AppDbContext dbContext)
    : Repository<VerificationCode>(dbContext), IVerificationCodesRepository
{
    public async Task<VerificationCode?> FindValidCodeAsync(string code, int userId, VerificationCodeType type)
    {
        return await DbSet
            .Where(vc => vc.Code == code && vc.UserId == userId && vc.Type == type &&
                         vc.ExpiresAt > DateTime.UtcNow && !vc.IsUsed && vc.DeletedAt == null)
            .FirstOrDefaultAsync();
    }

    public async Task<List<VerificationCode>> FindAllValidCodesForUserAndTypeAsync(int userId,
        VerificationCodeType type)
    {
        return await DbSet
            .Where(vc => vc.UserId == userId && vc.Type == type &&
                         vc.ExpiresAt > DateTime.UtcNow && !vc.IsUsed && vc.DeletedAt == null)
            .ToListAsync();
    }
}