using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Vjezba.Model;

namespace Vjezba.DAL
{
    public class AwfulDbContext : IdentityDbContext<AppUser>
    {
        protected AwfulDbContext() { }
        public AwfulDbContext(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasMany(c => c.Comics);
            modelBuilder.Entity<Category>().HasData(new Category { ID = 1, Tag = "Puns" });
            modelBuilder.Entity<Category>().HasData(new Category { ID = 2, Tag = "Math" });
            modelBuilder.Entity<Category>().HasData(new Category { ID = 3, Tag = "Anime" });
            modelBuilder.Entity<Category>().HasData(new Category { ID = 4, Tag = "Joke" });
            modelBuilder.Entity<Category>().HasData(new Category { ID = 5, Tag = "Sketch" });

        }
        public DbSet<Comic> Comics { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }
    }
}
