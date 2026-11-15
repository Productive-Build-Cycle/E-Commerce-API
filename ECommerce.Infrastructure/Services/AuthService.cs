using System.Globalization;
using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Services;

public class AuthService : IAuthService
{
    #region Fields

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserVoucherRepository _userVoucherRepository;
    private readonly WelcomeVoucherCodeGenerator _voucherCodeGenerator;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;


    #endregion

    #region Constructors

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUserVoucherRepository userVoucherRepository,
        WelcomeVoucherCodeGenerator voucherCodeGenerator,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _userVoucherRepository = userVoucherRepository;
        _voucherCodeGenerator = voucherCodeGenerator;
        _context = context;
        _configuration = configuration;
    }

    #endregion

    #region Methods

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var maximumDiscountAmount = GetWelcomeVoucherMaximumDiscountAmount();

        var exists = await _userRepository.GetByEmailAsync(email);

        if (exists != null) throw new InvalidOperationException("User with this email already exists.");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = email,
                    PasswordHash = _passwordHasher.Hash(request.Password),
                    Role = UserRole.Customer
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                var now = DateTime.UtcNow;
                var voucher = new UserVoucher
                {
                    UserId = user.Id,
                    Code = _voucherCodeGenerator.Generate(),
                    Type = DiscountType.Percentage,
                    Value = 20m,
                    StartDate = now,
                    EndDate = now.AddDays(30),
                    MinimumOrderAmount = 0m,
                    MaximumDiscountAmount = maximumDiscountAmount,
                    IsActive = true,
                    IsUsed = false,
                    UsedAtUtc = null
                };

                await _userVoucherRepository.AddAsync(voucher);
                await _userVoucherRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                var token = _tokenService.GenerateToken(user);

                return new AuthResponse
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    WelcomeVoucherCode = voucher.Code
                };
            }
            catch (DbUpdateException ex) when (IsVoucherCodeCollision(ex))
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        throw new InvalidOperationException(
            "Unable to generate a unique welcome voucher code. Please try again.");
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

    private decimal GetWelcomeVoucherMaximumDiscountAmount()
    {
        var configuredValue = _configuration[
            "WelcomeVoucher:MaximumDiscountAmount"];

        if (string.IsNullOrWhiteSpace(configuredValue) ||
            !decimal.TryParse(
                configuredValue,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var maximumDiscountAmount) ||
            maximumDiscountAmount <= 0)
        {
            throw new InvalidOperationException(
                "WelcomeVoucher:MaximumDiscountAmount must be configured as a positive decimal value.");
        }

        return maximumDiscountAmount;
    }

    private static bool IsVoucherCodeCollision(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
               sqlException.Number is 2601 or 2627 &&
               sqlException.Message.Contains(
                   "IX_UserVouchers_Code",
                   StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
