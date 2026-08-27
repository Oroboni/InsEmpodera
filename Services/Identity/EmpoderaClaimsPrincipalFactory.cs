using System.Security.Claims;
using Empodera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Empodera.Services.Identity;

public sealed class EmpoderaClaimsPrincipalFactory(
    UserManager<Usuario> userManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<Usuario>(userManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Nome));
        identity.AddClaim(new Claim("empodera:profile_id", user.FkIdPerfil.ToString()));
        return identity;
    }
}
