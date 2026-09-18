using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Woo.AICopilot.EntityFrameworkCore;
using Woo.AICopilot.IdentityService.Contracts;
using Woo.AICopilot.Infrastructure.Authentication;
using Woo.AICopilot.Infrastructure.Storage;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructures(this IHostApplicationBuilder builder)
    {
        builder.AddEfCore();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
    }   
}