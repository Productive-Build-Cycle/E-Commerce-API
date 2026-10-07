using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.DTOs.Authentication;

namespace ECommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<CurrentUserResponse> GetCurrentUserAsync(int userId);
}
