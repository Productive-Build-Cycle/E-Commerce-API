using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Persistence.SeedData;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<ICartRepository, CartRepository>();

        services.AddScoped<ICartService, CartService>();

        services.AddScoped<IProductService, ProductService>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<ICategoryService, CategoryService>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddSingleton<WelcomeVoucherCodeGenerator>();

        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<DatabaseSeeder>();

        services.AddScoped<IDiscountRepository, DiscountRepository>();

        services.AddScoped<IUserVoucherRepository, UserVoucherRepository>();

        services.AddScoped<IDiscountService, DiscountService>();

        services.AddScoped<ICheckoutService, CheckoutService>();

        return services;
    }
}
