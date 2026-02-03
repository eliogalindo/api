using api.Core.Interfaces.Repositories;
using api.Modules.VerificationCodes.Enums;
using api.Modules.VerificationCodes.Models;

namespace api.Modules.VerificationCodes.Interfaces.Repositories;

public interface IVerificationCodesRepository : IRepository<VerificationCode>
{
    Task<VerificationCode?> FindValidCodeAsync(string code, int userId, VerificationCodeType type);
    Task<List<VerificationCode>> FindAllValidCodesForUserAndTypeAsync(int userId, VerificationCodeType type);
}