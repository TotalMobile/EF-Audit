using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Tests.Fakes;
using System;

namespace Phnx.Audit.EF.Tests
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

        public DbSet<MultiKeyModel> MultiKeyModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModelToAudit>()
                .HasAudit<ModelToAudit, AuditEntryModel, string>(m => m.Audits)
                .HasMany(t => t.ManyToManyChildModels);

            modelBuilder.Entity<ManyToManyChildModel>()
                .HasMany(t => t.Parents);

            modelBuilder.Entity<ModelToAudit>()
                .HasOne(t => t.OneToOneChildModel)
                .WithOne(t => t.Parent)
                .HasForeignKey<OneToOneChildModel>(t => t.ParentId);

            modelBuilder.Entity<MultiKeyModel>()
                .HasKey(m => new { m.Id1, m.Id2 });

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
