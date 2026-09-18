using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Woo.AICopilot.Embedding;
using Woo.AICopilot.EventBus;

namespace Woo.AICopilot.RagService;

public static class DependencyInjection
{
    public static void AddRagService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        builder.AddEventBus();
        builder.AddEmbedding();
    }
}