using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    #region Fields

    private readonly PasswordHasher<object> _passwordHasher = new();

    #endregion

    #region Constructors

    public PasswordHasher(PasswordHasher<object> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    #endregion

    #region Methods

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool verify(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            null!,
            passwordHash,
            password);

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }

    #endregion
}
