using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Services;

public class JwtTokenService : ITokenService
{
    #region Fields

    private readonly IConfiguration _configuration;


    #endregion

    #region Constructors

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    #endregion

    #region Methods

    public string GenerateToken(User user)
    {
        // Retrieve JWT settings from configuration
        var jwtSettings = _configuration.GetSection("Jwt");

        // Validate that the required JWT settings are present
        var key = jwtSettings["Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured.");

        var issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured.");

        var audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured.");

        var expirationMinutes = int.Parse(
            jwtSettings["expirationMinutes"] ?? "60");

        // Create claims based on the user information
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // Signature: Create the security key and signing credentials
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        // Create signing credentials using the security key and HMAC SHA256 algorithm
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion
}
