using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NUnit.Framework;
using Phnx.Audit.EF;
using Phnx.Audit.EF.Models;
using Phnx.Audit.EF.Tests;
using Phnx.Audit.EF.Tests.Fakes;
using System;

namespace Phnx.Audit.EF.Tests
{
    public class AuditServiceTests : ContextTestBase
    {
        public AuditService<FakeContext> GenerateAuditService(IAuditWriter<FakeContext> auditWriter = null, IChangeDetectionService<FakeContext> changeDetectionService = null, IEntityKeyService<FakeContext> entityKeyService = null)
        {
            if (auditWriter is null)
            {
                var fakeAuditWriter = new Mock<IAuditWriter<FakeContext>>();
                auditWriter = fakeAuditWriter.Object;
            }

            if (changeDetectionService is null)
            {
                var fakeChangeDetector = new Mock<IChangeDetectionService<FakeContext>>();
                changeDetectionService = fakeChangeDetector.Object;
            }

            if (entityKeyService is null)
            {
                var fakeEntityKeyService = new Mock<IEntityKeyService<FakeContext>>();
                entityKeyService = fakeEntityKeyService.Object;
            }

            var auditService = new AuditService<FakeContext>(auditWriter, changeDetectionService, entityKeyService);

            return auditService;
        }

        [Test]
        public void GenerateEntry_WithEntityId_SetsEntityId()
        {
            var id = Guid.NewGuid().ToString();
            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, string>(id, DateTime.UtcNow);

            Assert.AreEqual(id, newEntry.EntityId);
        }

        [Test]
        public void GenerateEntry_WithAuditedOn_SetsAuditedOn()
        {
            var auditedOn = new DateTime(2000, 1, 1);
            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, string>("", auditedOn);

            Assert.AreEqual(auditedOn, newEntry.AuditedOn);
        }

        [Test]
        public void GenerateForEntries_WithEntitySelector_SetsEntityIdAccordingToSelectorResult()
        {
            var before = "before_changes";
            var after = "after_changes";
            AuditedOperationTypeEnum type = AuditedOperationTypeEnum.Update;
            var model = new ModelToAudit
            {
                Id = Guid.NewGuid().ToString()
            };

            var mockChanges = new Mock<IChangeDetectionService<FakeContext>>();
            mockChanges
                .Setup(c => c.GetChangeType(It.IsAny<EntityEntry>()))
                .Returns(type);

            mockChanges
                .Setup(c => c.SerializeEntityChanges(It.IsAny<AuditedOperationTypeEnum>(), It.IsAny<EntityEntry>()))
                .Returns((before, after));

            var mockKey = new Mock<IEntityKeyService<FakeContext>>();
            mockKey.Setup(e => e.GetPrimaryKey<ModelToAudit, string>(It.IsAny<ModelToAudit>()))
                .Returns(model.Id);


            AuditService<FakeContext> auditService = GenerateAuditService(null, mockChanges.Object, mockKey.Object);

            AuditEntryModel entry = auditService.GenerateEntry<ModelToAudit, AuditEntryModel, string>(model);

            Assert.AreEqual(model.Id, entry.EntityId);
            Assert.AreEqual(type, entry.Type);
            Assert.AreEqual(before, entry.EntityBeforeJson);
            Assert.AreEqual(after, entry.EntityAfterJson);
        }
    }
}
