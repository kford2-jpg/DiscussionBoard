using DiscussionBoard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Forum> Forums { get; set; }

        public DbSet<DiscussionThread> DiscussionThreads { get; set; }

        public DbSet<Post> Posts { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Forum>()
                .HasMany(f => f.DiscussionThreads)
                .WithOne(t => t.Forum)
                .HasForeignKey(t => t.ForumId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DiscussionThread>()
                .HasMany(t => t.Posts)
                .WithOne(p => p.DiscussionThread)
                .HasForeignKey(p => p.DiscussionThreadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DiscussionThread>()
                .HasOne(t => t.ApplicationUser)
                .WithMany(u => u.DiscussionThreads)
                .HasForeignKey(t => t.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Post>()
                .HasOne(p => p.ApplicationUser)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Post>()
                .HasOne(p => p.ParentPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ParentPostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Forum>()
                .Property(f => f.Name)
                .HasMaxLength(100);

            builder.Entity<Forum>()
                .Property(f => f.Description)
                .HasMaxLength(1000);

            builder.Entity<DiscussionBase>()
                .Property(d => d.Title)
                .HasMaxLength(200);

            builder.Entity<DiscussionBase>()
                .Property(d => d.Content)
                .HasMaxLength(10000);
        }

    }
}
