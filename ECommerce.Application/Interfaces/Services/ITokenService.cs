using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
