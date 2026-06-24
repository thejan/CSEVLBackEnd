using CSEVirtualLabWebAPI.Hubs;
using CSEVirtualLabWebAPI.Models;
using CSEVirtualLabWebAPI.Services;
using CSEVirtualLabDataAccessLayer;
using CSEVirtualLabDataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

namespace CSEVirtualLabWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddHttpClient();

            builder.Services.AddDbContext<AtmecsevlabContext>(
                options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString(
                            "AtmecsevlabConnection")));

            builder.Services.AddScoped<VirtualLabRepository>();

            builder.Services.AddScoped<CCompilerService>();

            builder.Services.AddScoped<LabReportService>();

            builder.Services.AddScoped<GeminiCProgrammingService>();

            builder.Services.Configure<SmtpSettings>(
                builder.Configuration.GetSection("Smtp"));

            builder.Services.AddScoped<RegistrationEmailService>();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;

                options.AddPolicy(
                    "ChatPolicy",
                    httpContext =>
                    {
                        var clientKey =
                            httpContext.Connection.RemoteIpAddress?
                                .ToString() ??
                            "unknown";

                        return RateLimitPartition
                            .GetFixedWindowLimiter(
                                clientKey,
                                _ => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 10,
                                    Window = TimeSpan.FromMinutes(1),
                                    QueueLimit = 0,
                                    AutoReplenishment = true
                                });
                    });
            });

            builder.Services.AddSignalR();

            builder.Services.AddSingleton<ExecutionSessionService>();

            builder.Services.AddSingleton<LiveTerminalService>();

            builder.Services.AddSingleton<AdminSessionService>();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy(
            //        "AllowAngular",
            //        policy =>
            //        {
            //            policy
            //                .WithOrigins("http://localhost:4200")
            //                .AllowAnyHeader()
            //                .AllowAnyMethod()
            //                .AllowCredentials();
            //        });
            //});

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowAngular",
                    policy =>
                    {
                        policy
                            .WithOrigins(
                                "http://localhost:4200",
                                "https://atme-cse-virtual-lab.web.app",
                                "https://atme-cse-virtual-lab.firebaseapp.com"
                            )
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            var app = builder.Build();

            DatabaseSchemaInitializer
                .EnsureApplicationTablesAsync(app.Services)
                .GetAwaiter()
                .GetResult();

            app.UseHttpsRedirection();

            app.UseCors("AllowAngular");

            app.UseRateLimiter();

            app.Use(async (context, next) =>
            {
                if (
                    context.Request.Path
                        .StartsWithSegments("/api/admin")
                    &&
                    !context.Request.Path
                        .StartsWithSegments("/api/admin-auth")
                )
                {
                    var sessionService =
                        context.RequestServices
                            .GetRequiredService<AdminSessionService>();

                    string? token =
                        context.Request.Headers[
                            "X-Admin-Token"]
                            .FirstOrDefault();

                    if (!sessionService.IsValid(token))
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        await context.Response.WriteAsJsonAsync(
                            new
                            {
                                success = false,
                                message =
                                    "Administrator login is required."
                            });

                        return;
                    }
                }

                await next();
            });

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI();

            app.MapControllers();

            app.MapHub<TerminalHub>("/terminalHub");

            Console.WriteLine("TerminalHub mapped at /terminalHub");

            app.Run();
        }
    }
}
