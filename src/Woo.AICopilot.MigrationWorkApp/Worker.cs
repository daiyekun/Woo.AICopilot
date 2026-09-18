using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Woo.AICopilot.EntityFrameworkCore;
using Woo.AICopilot.MigrationWorkApp.SeedData;

namespace Woo.AICopilot.MigrationWorkApp;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AiCopilotDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            
            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, roleManager, userManager, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(AiCopilotDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => { await dbContext.Database.MigrateAsync(cancellationToken); });
    }

    private static async Task SeedDataAsync(
        AiCopilotDbContext dbContext,
        RoleManager<IdentityRole> roleManager, 
        UserManager<IdentityUser> userManager,
        CancellationToken cancellationToken)
    {
        // 创建默认角色
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // 创建默认管理员账户
        const string adminUserName = "admin";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByNameAsync(adminUserName);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminUserName
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
            else
                Console.WriteLine("创建管理员失败：" + string.Join(",", result.Errors.Select(e => e.Description)));
        }

        // 创建默认语言模型
        if (!await dbContext.LanguageModels.AnyAsync(cancellationToken: cancellationToken))
        {
            await dbContext.LanguageModels.AddRangeAsync(AiGatewayData.LanguageModels(), cancellationToken);
        }
        
        // 创建默认对话模板
        if (!await dbContext.ConversationTemplates.AnyAsync(cancellationToken: cancellationToken))
        {
            await dbContext.ConversationTemplates.AddRangeAsync(AiGatewayData.ConversationTemplates(), cancellationToken);
        }
        
        // 创建默认嵌入模型
        if (!await dbContext.EmbeddingModels.AnyAsync(cancellationToken: cancellationToken))
        {
            await dbContext.EmbeddingModels.AddRangeAsync(RagData.EmbeddingModels(), cancellationToken);
        }
        
        // 创建默认知识库
        if (!await dbContext.KnowledgeBases.AnyAsync(cancellationToken: cancellationToken))
        {
            await dbContext.KnowledgeBases.AddRangeAsync(RagData.KnowledgeBases(), cancellationToken);
        }
        
        // 创建默认业务数据库
        if (!await dbContext.BusinessDatabases.AnyAsync(cancellationToken: cancellationToken))
        {
            await dbContext.BusinessDatabases.AddRangeAsync(DataAnalysisData.GetDatabases(), cancellationToken);
        }
        
        // 创建默认MCPServer
        if (!await dbContext.McpServerInfos.AnyAsync(cancellationToken: cancellationToken))
        {
            await dbContext.McpServerInfos.AddRangeAsync(McpServerInfoData.GetMcpServerInfos(), cancellationToken);
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}