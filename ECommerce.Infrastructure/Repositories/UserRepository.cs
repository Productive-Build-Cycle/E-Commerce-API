using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    #region Fields

    private readonly ApplicationDbContext _context;

    #endregion

    #region Constructors

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    #endregion

    #region Methods

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<bool> ExistByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    #endregion
}
