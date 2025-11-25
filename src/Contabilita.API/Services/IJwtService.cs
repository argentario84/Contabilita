using Contabilita.Core.Entities;

namespace Contabilita.API.Services;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
}
