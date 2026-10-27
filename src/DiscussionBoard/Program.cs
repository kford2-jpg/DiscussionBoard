
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Serilog;
using Serilog.Formatting.Compact;

namespace DiscussionBoard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string logPath = Path.Combine(
                builder.Environment.ContentRootPath,
                "Logs",
                "discussionboard-.json");

            builder.Services.AddSerilog((services, configuration) =>
                configuration
                    .WriteTo.Console()
                    .WriteTo.File(
                        new RenderedCompactJsonFormatter(),
                        logPath,
                        rollingInterval: RollingInterval.Day,
                        shared: true)
            );

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                    options.SignIn.RequireConfirmedAccount =
                        !builder.Environment.IsDevelopment())
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRazorPages();

            // Register application services.
            builder.Services.AddScoped<ForumService>();
            builder.Services.AddScoped<DiscussionThreadService>();
            builder.Services.AddScoped<PostService>();

            builder.Services.AddSingleton<ILogReader, JsonLogReader>();

            var app = builder.Build();

            // Apply migrations and create a sample forum if needed.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<ApplicationDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    db.Database.Migrate();

                    if (!db.Forums.Any(f => !f.IsDeleted))
                    {
                        db.Forums.Add(new Forum
                        {
                            Name = "General Discussion",
                            Description =
                                "A place to ask questions and share ideas."
                        });

                        db.SaveChanges();

                        logger.LogInformation(
                            "Created the default General Discussion forum.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "An error occurred while initializing the discussion board database.");

                    throw;
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapRazorPages()
                .WithStaticAssets();

            app.Run();
        }
    }
}
