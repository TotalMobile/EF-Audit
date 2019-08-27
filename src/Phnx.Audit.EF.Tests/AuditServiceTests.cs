using Moq;
using NUnit.Framework;
using Phnx.Audit.EF.Tests.Fakes;
using System;
using System.Collections.Generic;

namespace Phnx.Audit.EF.Tests
{
    public class AuditServiceTests : ContextTestBase
    {
        public AuditService<FakeContext> GenerateAuditService(IChangeDetectionService<FakeContext> changeDetectionService = null)
        {
            if (changeDetectionService is null)
            {
                var fakeChangeDetector = new Mock<IChangeDetectionService<FakeContext>>();
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
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, ModelToAudit, string>(new ModelToAudit(), auditedOn);

            Assert.AreEqual(auditedOn, newEntry.AuditedOn);
        }

        [Test]
        public void GenerateEntry_WhenIdIsDbGeneratedAndModelIsNew_SetsEntityIdOnSave()
        {
            var model = new ModelToAudit();
            Context.Add(model);

            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, ModelToAudit, string>(model);

            model.Audits = new List<AuditEntryModel>
            {
                newEntry
            };

            Context.SaveChanges();

            Assert.AreEqual(model.Id, newEntry.EntityId);
        }

        [Test]
        public void GenerateEntry_WhenIdIsNotDbGenerated_SetsEntityIdOnSave()
        {
            var model = new ModelToAudit();
            Context.Add(model);

            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, ModelToAudit, string>(model);

            model.Audits = new List<AuditEntryModel>
            {
                newEntry
            };

            Context.SaveChanges();

            Assert.AreEqual(model.Id, newEntry.EntityId);
        }
    }
}
