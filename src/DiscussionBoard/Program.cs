
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DiscussionBoard.Authorization;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Serilog;


namespace DiscussionBoard
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string logPath = Path.Combine(
                builder.Environment.ContentRootPath,
                "Logs",
                "discussionboard-.json");

            builder.Configuration["Serilog:WriteTo:1:Args:configure:0:Args:path"] =
                logPath;

            builder.Services.AddSerilog((services, configuration) =>
                configuration.ReadFrom.Configuration(builder.Configuration));

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Identity with role support.
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                    options.SignIn.RequireConfirmedAccount =
                        !builder.Environment.IsDevelopment())
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Strengthen authentication cookie security.
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Register authorization policies and handlers.
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    ApplicationAuthorizationPolicies.IsOwnerOrAdmin,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.AddRequirements(
                            new IsOwnerOrAdminRequirement());
                    });
            });

            builder.Services.AddScoped<
                IAuthorizationHandler,
                IsOwnerOrAdminHandler>();

            // Require authentication for forum and discussion pages.
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Forums");
                options.Conventions.AuthorizeFolder("/Discussions");
            });

            // Register application services.
            builder.Services.AddScoped<ForumService>();
            builder.Services.AddScoped<DiscussionThreadService>();
            builder.Services.AddScoped<PostService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();

            builder.Services.AddScoped<
                IApplicationAuthorizationService,
                ApplicationAuthorizationService>();

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
