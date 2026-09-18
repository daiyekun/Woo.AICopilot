using System.Reflection;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Woo.AICopilot.AiGatewayService;
using Woo.AICopilot.DataAnalysisService;
using Woo.AICopilot.HttpApi.Infrastructure;
using Woo.AICopilot.Infrastructure.Authentication;
using Woo.AICopilot.McpService;
using Woo.AICopilot.RagService;
using Woo.AICopilot.Services.Common.Behaviors;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.HttpApi;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationService()
        {
            builder.Services.AddMediatR(cfg =>
            {
                cfg.LicenseKey =
                    "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzg0NzY0ODAwIiwiaWF0IjoiMTc1MzIzNjIyMSIsImFjY291bnRfaWQiOiIwMTk4MzUwNDdkNGI3ZmU5YmZlMzdhMWQ2MDQwMzM4NSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxazB0Z2JiNHZ2N2tnZDlyMXBtc2pmNG4xIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.s5kG1QZdtbY_jtqsxQpdOQUSoXFb5MwFGp6AP1rBqPycBn03RUsmFVHaAQJKBMOHvTUGLLJPts1q7TaY7pV2Dut5n0LtnNXaq4r8AZ5rOSQWOAcLfuMUFMLDwhR9BGuPODvNje74evts-4zB6qJKwxcdk8a-DrN1qGEQcB3Zksh1Su02jIDBiUjvAG07wjUdt-n8AdMF2kM-hPAMdxBV4Wr_cqJV_EbimBAiEeMUpey7G4qaLPcsJo0lKu7T6KRjc3YNpiZ9hGh9Tf_JWHMS__ed9wpueK6kvFjwQuBAGjpFb51FFdQDUh2Uuuuo7ldvofBSaX6xIfXjLL1hqQ0MEQ";

                cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(IdentityService.DependencyInjection))!);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            });
            
            builder.AddAiGatewayService();
            
            builder.AddDataAnalysisService();
            
            builder.AddRagService();
            
            builder.AddMcpService();
        }

        public void AddWebServices()
        {
            // 从配置文件中读取JwtSettings，并注入到容器中
            var configurationSection = builder.Configuration.GetSection("JwtSettings");
            var jwtSettings = configurationSection.Get<JwtSettings>();
            if (jwtSettings is null) throw new NullReferenceException(nameof(jwtSettings));

            builder.Services.Configure<JwtSettings>(configurationSection);

            // 添加认证服务
            builder.Services.AddAuthentication(options =>
                {
                    // 默认的认证方案和质询方案都设置为 JWT Bearer
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options => // 添加 JWT Bearer 认证处理器
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // --- 关键验证项 ---
                        ValidateIssuer = true, // 验证颁发者
                        ValidateAudience = true, // 验证受众
                        ValidateLifetime = true, // 验证生命周期（是否过期）
                        ValidateIssuerSigningKey = true, // 验证签名密钥

                        // --- 配置具体的值 ---
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                        ),

                        // 允许5分钟的服务器时钟偏差
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddScoped<ICurrentUser, CurrentUser>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddExceptionHandler<UseCaseExceptionHandler>();

            builder.Services.AddProblemDetails();
        }
    }
}