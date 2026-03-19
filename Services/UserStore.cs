using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MafiaStore.Models.Auth;

namespace MafiaStore.Services;

public sealed class UserStore : IUserStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = true
    };

    private readonly string _usersPath;
    private readonly object _syncRoot = new();
    private List<UserAccount>? _cache;

    public UserStore(IWebHostEnvironment environment)
    {
        _usersPath = Path.Combine(environment.ContentRootPath, "context", "users.json");
    }

    public UserAccount? Authenticate(string username, string password)
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            var user = _cache!.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                return null;
            }

            return VerifyPassword(password, user.PasswordSalt, user.PasswordHash)
                ? CloneUser(user)
                : null;
        }
    }

    public bool CreateUser(string username, string email, string password, string role, out string error)
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            var users = _cache!;

            var normalizedUsername = username.Trim();
            var normalizedEmail = email.Trim();

            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                error = "Username is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                error = "Email is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                error = "Password must have at least 6 characters.";
                return false;
            }

            if (users.Any(u => u.Username.Equals(normalizedUsername, StringComparison.OrdinalIgnoreCase)))
            {
                error = "Username already exists.";
                return false;
            }

            if (users.Any(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
            {
                error = "Email already exists.";
                return false;
            }

            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);

            users.Add(new UserAccount
            {
                Username = normalizedUsername,
                Email = normalizedEmail,
                PasswordSalt = salt,
                PasswordHash = hash,
                Role = string.IsNullOrWhiteSpace(role) ? "Customer" : role.Trim()
            });

            Persist();
            error = string.Empty;
            return true;
        }
    }

    public UserAccount? FindByUsername(string username)
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            var user = _cache!.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
            return user is null ? null : CloneUser(user);
        }
    }

    public IReadOnlyList<UserAccount> GetAll()
    {
        lock (_syncRoot)
        {
            EnsureLoaded();
            return _cache!
                .Select(CloneUser)
                .ToList();
        }
    }

    private void EnsureLoaded()
    {
        if (_cache is not null)
        {
            return;
        }

        if (!File.Exists(_usersPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_usersPath)!);
            _cache = new List<UserAccount>();
            EnsureDefaultAccounts();
            Persist();
            return;
        }

        var json = File.ReadAllText(_usersPath);
        _cache = JsonSerializer.Deserialize<List<UserAccount>>(json, JsonOptions) ?? new List<UserAccount>();
        var changed = EnsureDefaultAccounts();
        if (changed)
        {
            Persist();
        }
    }

    private bool EnsureDefaultAccounts()
    {
        _cache ??= new List<UserAccount>();
        var changed = false;

        changed |= UpsertSeedUser("admin", "admin@hypecartel.local", "Admin@123", "Admin");
        changed |= UpsertSeedUser("cliente", "cliente@hypecartel.local", "Cliente@123", "Customer");

        return changed;
    }

    private bool UpsertSeedUser(string username, string email, string password, string role)
    {
        var existing = _cache!.FirstOrDefault(user =>
            user.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            CreateSeedUser(username, email, password, role);
            return true;
        }

        var expectedHash = HashPassword(password, existing.PasswordSalt);
        var roleChanged = !existing.Role.Equals(role, StringComparison.OrdinalIgnoreCase);
        var hashChanged = !expectedHash.Equals(existing.PasswordHash, StringComparison.Ordinal);
        var emailChanged = !existing.Email.Equals(email, StringComparison.OrdinalIgnoreCase);

        if (!roleChanged && !hashChanged && !emailChanged)
        {
            return false;
        }

        existing.Email = email;
        existing.Role = role;
        existing.PasswordHash = expectedHash;
        return true;
    }

    private void CreateSeedUser(string username, string email, string password, string role)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);

        _cache!.Add(new UserAccount
        {
            Username = username,
            Email = email,
            PasswordSalt = salt,
            PasswordHash = hash,
            Role = role
        });
    }

    private void Persist()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_usersPath)!);
        var json = JsonSerializer.Serialize(_cache, JsonOptions);
        var tempPath = _usersPath + ".tmp";

        File.WriteAllText(tempPath, json);
        if (File.Exists(_usersPath))
        {
            File.Delete(_usersPath);
        }

        File.Move(tempPath, _usersPath);
    }

    private static string GenerateSalt()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password, string base64Salt)
    {
        var salt = Convert.FromBase64String(base64Salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var payload = new byte[salt.Length + passwordBytes.Length];
        Buffer.BlockCopy(salt, 0, payload, 0, salt.Length);
        Buffer.BlockCopy(passwordBytes, 0, payload, salt.Length, passwordBytes.Length);
        var hash = SHA256.HashData(payload);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string salt, string expectedHash)
    {
        var computed = HashPassword(password, salt);
        return computed.Equals(expectedHash, StringComparison.Ordinal);
    }

    private static UserAccount CloneUser(UserAccount source)
    {
        return new UserAccount
        {
            Username = source.Username,
            Email = source.Email,
            PasswordHash = source.PasswordHash,
            PasswordSalt = source.PasswordSalt,
            Role = source.Role
        };
    }
}
