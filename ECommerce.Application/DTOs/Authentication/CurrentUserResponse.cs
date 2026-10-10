using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.DTOs.Authentication;

public class CurrentUserResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}
