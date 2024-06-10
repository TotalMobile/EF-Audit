using Moq;
using NUnit.Framework;
using Phnx.Audit.EF.Tests.Fakes;
using System;
using System.Collections.Generic;

namespace Phnx.Audit.EF.Tests
{
    public class AuditServiceTests : ContextTestBase
    {
        public AuditService<FakeContext> GenerateAuditService(IChangeDetectionService changeDetectionService = null)
        {
            if (changeDetectionService is null)
            {
                var fakeChangeDetector = new Mock<IChangeDetectionService>();
                changeDetectionService = fakeChangeDetector.Object;
            }

            var auditService = new AuditService<FakeContext>(Context, changeDetectionService);

            return auditService;
        }

        [Test]
        public void GenerateEntry_WithAuditedOn_SetsAuditedOn()
        {
            var auditedOn = new DateTime(2000, 1, 1);
            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, ModelToAudit>(new ModelToAudit(), auditedOn);

            Assert.That(auditedOn, Is.EqualTo(newEntry.AuditedOn));
        }

        [Test]
        public void GenerateEntry_WhenIdIsDbGeneratedAndModelIsNew_SetsEntityIdOnSave()
        {
            var model = new ModelToAudit();
            Context.Add(model);

            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, ModelToAudit>(model);

            model.Audits = new List<AuditEntryModel>
            {
                newEntry
            };

            Context.SaveChanges();

            Assert.That(model.Id, Is.EqualTo(newEntry.EntityId));
        }

        [Test]
        public void GenerateEntry_WhenIdIsNotDbGenerated_SetsEntityIdOnSave()
        {
            var model = new ModelToAudit();
            Context.Add(model);

            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, ModelToAudit>(model);

            model.Audits = new List<AuditEntryModel>
            {
                newEntry
            };

            Context.SaveChanges();

            Assert.That(model.Id, Is.EqualTo(newEntry.EntityId));
        }
    }
}
