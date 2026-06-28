using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Core.Entities;
using TheUnraveller.Service.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;

namespace TheUnraveller.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        return GenerateJwtToken(user);
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null) return false;

        var defaultEnergy = _configuration.GetValue<int>("GameRules:DefaultEnergy", 100);
        var defaultMaxEnergy = _configuration.GetValue<int>("GameRules:DefaultMaxEnergy", 100);
        var defaultEnglishLevel = _configuration.GetValue<string>("GameRules:DefaultEnglishLevel") ?? "B1";

        var newUser = new User
        {
            Username = username,
            Email = email,
            Energy = defaultEnergy,
            MaxEnergy = defaultMaxEnergy,
            XpBalance = 0,
            IsPremium = false,
            EnglishLevel = defaultEnglishLevel,
            CreatedAt = DateTime.UtcNow,
            LastEnergyRechargedAt = DateTime.UtcNow,
            LastActiveDate = DateTime.UtcNow
        };

        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<string> LoginWithGoogleAsync(string idToken)
    {
        var googleSettings = _configuration.GetSection("Google");
        var clientId = googleSettings["ClientId"];

        try
        {
            GoogleJsonWebSignature.Payload? payload = null;
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
                    break;
                }
                catch (Exception ex) when (ex.Message.Contains("transient") && i < maxRetries - 1)
                {
                    await Task.Delay(1000 * (i + 1));
                }
            }

            if (payload == null)
            {
                throw new InvalidOperationException("Failed to validate Google token due to persistent transient network errors on the server.");
            }

            var email = payload.Email;
            var name = payload.Name;

            var defaultEnergy = _configuration.GetValue<int>("GameRules:DefaultEnergy", 100);
            var defaultMaxEnergy = _configuration.GetValue<int>("GameRules:DefaultMaxEnergy", 100);
            var defaultEnglishLevel = _configuration.GetValue<string>("GameRules:DefaultEnglishLevel") ?? "B1";

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    Username = name ?? email.Split('@')[0],
                    Email = email,
                    Energy = defaultEnergy,
                    MaxEnergy = defaultMaxEnergy,
                    XpBalance = 0,
                    IsPremium = false,
                    EnglishLevel = defaultEnglishLevel,
                    CreatedAt = DateTime.UtcNow,
                    LastEnergyRechargedAt = DateTime.UtcNow,
                    LastActiveDate = DateTime.UtcNow
                };
                user.PasswordHash = _passwordHasher.HashPassword(user, Guid.NewGuid().ToString());

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }

            return GenerateJwtToken(user);
        }
        catch (Exception ex)
        {
            var innerDetail = ex.InnerException != null
                ? $" | InnerException: {ex.InnerException.Message} | StackTrace: {ex.InnerException.StackTrace}"
                : "";
            throw new UnauthorizedAccessException($"Google authentication failed: {ex.Message}{innerDetail}");
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("IsPremium", user.IsPremium.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(jwtSettings["DurationInHours"] ?? "24")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
