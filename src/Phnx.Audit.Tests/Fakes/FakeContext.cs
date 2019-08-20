using Microsoft.EntityFrameworkCore;
using Phnx.Audit.Tests.Fakes;
using System;

namespace Phnx.Audit.Tests
{
    public class FakeContext : DbContext
    {
        public FakeContext()
        {
        }

        public FakeContext(DbContextOptions<FakeContext> options) : base(options)
        {
        }

        public DbSet<AuditEntryModel> AuditEntries { get; set; }

        public DbSet<ModelToAudit> Models { get; set; }

        public DbSet<OneToOneChildModel> OneToOneChildModels { get; set; }

        public DbSet<OneToManyChildModel> OneToManyChildModels { get; set; }

        public DbSet<ManyToManyChildModel> ManyToManyChildModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModelToAudit>()
                .HasMany(t => t.ManyToManyChildModels);

            modelBuilder.Entity<ManyToManyChildModel>()
                .HasMany(t => t.Parents);

            modelBuilder.Entity<ModelToAudit>()
                .HasOne(t => t.OneToOneChildModel)
                .WithOne(t => t.Parent)
                .HasForeignKey<OneToOneChildModel>(t => t.ParentId);

            base.OnModelCreating(modelBuilder);
        }

        public static FakeContext Create()
        {
            var options = new DbContextOptionsBuilder<FakeContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FakeContext(options);
        }
    }
}
