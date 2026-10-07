using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Services;

public class AuthService : IAuthService
{
    #region Fields

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;


    #endregion

    #region Constructors

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    #endregion

    #region Methods

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var exists = await _userRepository.GetByEmailAsync(email);

        if (exists != null) throw new InvalidOperationException("User with this email already exists.");

        // Continue with registration logic...
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Customer // Default role
        };

        await _userRepository.AddAsync(user);

        await _userRepository.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null) return null;

        var passwordIsValid = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordIsValid) return null;

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null) return null;

        return new CurrentUserResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    #endregion
}
