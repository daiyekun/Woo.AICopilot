using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Woo.AICopilot.AgentPlugin;

public static class AgentPluginExtensions
{
    public static IServiceCollection AddAgentPlugin(
        this IServiceCollection services, 
        Action<IAgentPluginRegistrar> configure)
    {
        // 1. 创建注册器实例
        var registrar = new AgentPluginRegistrar();

        // 2. 执行用户配置（用户在这里指定要扫描的程序集）
        configure(registrar);

        // 3. 将注册器注册为单例。注意这里没有使用 TryAdd，
        // 因为我们允许用户多次调用 AddAgentPlugin 来注册不同来源的插件。
        services.AddSingleton<IAgentPluginRegistrar>(registrar);

        // 4. 注册加载器。加载器只应有一个，它会收集容器中所有的 Registrar。
        services.TryAddSingleton<AgentPluginLoader>();

        return services;
    }
}