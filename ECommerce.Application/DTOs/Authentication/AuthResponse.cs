using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.DTOs.Authentication;

public class AuthResponse
{
    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;
}
