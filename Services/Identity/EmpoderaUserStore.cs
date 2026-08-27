using System.Security.Cryptography;
using Empodera.Data;
using Empodera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Services.Identity;

/// <summary>
/// Adapta a entidade de domínio Usuario às APIs oficiais do ASP.NET Core Identity
/// sem duplicar usuários nem substituir os perfis e relacionamentos existentes.
/// </summary>
public sealed class EmpoderaUserStore(ApplicationDbContext context) :
    IQueryableUserStore<Usuario>,
    IUserPasswordStore<Usuario>,
    IUserEmailStore<Usuario>,
    IUserSecurityStampStore<Usuario>,
    IUserLockoutStore<Usuario>,
    IUserTwoFactorStore<Usuario>,
    IUserPhoneNumberStore<Usuario>
{
    public IQueryable<Usuario> Users => context.Usuarios;

    public async Task<IdentityResult> CreateAsync(Usuario user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();

        if (user.IdUsuario <= 0)
            user.IdUsuario = await NextRandomIdAsync(cancellationToken);

        user.ConcurrencyStamp = NewStamp();
        user.SecurityStamp = string.IsNullOrWhiteSpace(user.SecurityStamp) ? NewStamp() : user.SecurityStamp;
        context.Usuarios.Add(user);
        return await SaveAsync(cancellationToken);
    }

    public async Task<IdentityResult> UpdateAsync(Usuario user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();

        user.ConcurrencyStamp = NewStamp();
        context.Usuarios.Update(user);
        return await SaveAsync(cancellationToken);
    }

    public async Task<IdentityResult> DeleteAsync(Usuario user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();

        context.Usuarios.Remove(user);
        return await SaveAsync(cancellationToken);
    }

    public Task<string> GetUserIdAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.IdUsuario.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public Task<string?> GetUserNameAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.UserName);

    public Task SetUserNameAsync(Usuario user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(Usuario user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    public async Task<Usuario?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(userId, out var id))
            return null;
        return await context.Usuarios.SingleOrDefaultAsync(user => user.IdUsuario == id, cancellationToken);
    }

    public Task<Usuario?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
        context.Usuarios.SingleOrDefaultAsync(
            user => user.NormalizedUserName == normalizedUserName,
            cancellationToken);

    public Task SetPasswordHashAsync(Usuario user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.Senha = passwordHash ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Senha);

    public Task<bool> HasPasswordAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(!string.IsNullOrWhiteSpace(user.Senha));

    public Task SetEmailAsync(Usuario user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Email);

    public Task<bool> GetEmailConfirmedAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.EmailConfirmed);

    public Task SetEmailConfirmedAsync(Usuario user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<Usuario?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        context.Usuarios.SingleOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail,
            cancellationToken);

    public Task<string?> GetNormalizedEmailAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(Usuario user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task SetSecurityStampAsync(Usuario user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.SecurityStamp);

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.LockoutEnd);

    public Task SetLockoutEndDateAsync(Usuario user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(++user.AccessFailedCount);

    public Task ResetAccessFailedCountAsync(Usuario user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.AccessFailedCount);

    public Task<bool> GetLockoutEnabledAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.LockoutEnabled);

    public Task SetLockoutEnabledAsync(Usuario user, bool enabled, CancellationToken cancellationToken)
    {
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task SetTwoFactorEnabledAsync(Usuario user, bool enabled, CancellationToken cancellationToken)
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.TwoFactorEnabled);

    public Task SetPhoneNumberAsync(Usuario user, string? phoneNumber, CancellationToken cancellationToken)
    {
        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    public Task<string?> GetPhoneNumberAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PhoneNumber);

    public Task<bool> GetPhoneNumberConfirmedAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PhoneNumberConfirmed);

    public Task SetPhoneNumberConfirmedAsync(Usuario user, bool confirmed, CancellationToken cancellationToken)
    {
        user.PhoneNumberConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // O DbContext é controlado pelo escopo de injeção de dependência.
    }

    private async Task<int> NextRandomIdAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 64; attempt++)
        {
            var candidate = RandomNumberGenerator.GetInt32(100_000, 1_000_000);
            if (!await context.Usuarios.AnyAsync(user => user.IdUsuario == candidate, cancellationToken))
                return candidate;
        }

        throw new InvalidOperationException("Não foi possível gerar um identificador de usuário único.");
    }

    private async Task<IdentityResult> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "ConcurrencyFailure",
                Description = "O usuário foi alterado por outra operação. Recarregue a página e tente novamente."
            });
        }
        catch (DbUpdateException)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PersistenceFailure",
                Description = "Não foi possível salvar o usuário. Verifique se o e-mail já está cadastrado."
            });
        }
    }

    private static string NewStamp() => Guid.NewGuid().ToString("N");
}
