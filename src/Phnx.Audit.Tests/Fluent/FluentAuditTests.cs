using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NUnit.Framework;
using Phnx.Audit.Fluent;
using Phnx.Audit.Models;
using Phnx.Audit.Tests.Fakes;
using System;
using System.Linq;

namespace Phnx.Audit.Tests.Fluent
{
    public class FluentAuditTests : ContextTestBase
    {
        public AuditService<FakeContext> GenerateAuditService(IAuditWriter<FakeContext> auditWriter = null, IChangeDetectionService<FakeContext> changeDetectionService = null)
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

            var auditService = new AuditService<FakeContext>(auditWriter, changeDetectionService);

            return auditService;
        }

        [Test]
        public void New_SetsEntityId_ToEntityId()
        {
            var id = Guid.NewGuid().ToString();
            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, string>(id, DateTime.UtcNow);

            Assert.AreEqual(id, newEntry.EntityId);
        }

        [Test]
        public void New_SetsAuditedOn_ToAuditedOn()
        {
            var auditedOn = new DateTime(2000, 1, 1);
            AuditService<FakeContext> auditService = GenerateAuditService();
            AuditEntryModel newEntry = auditService.GenerateEntry<AuditEntryModel, string>("", auditedOn);

            Assert.AreEqual(auditedOn, newEntry.AuditedOn);
        }

        [Test]
        public void WithDescription_WhenDescriptionIsNotNull_SetsDescription()
        {
            var description = "Sample";
            AuditService<FakeContext> auditService = GenerateAuditService();
            FluentAudit<FakeContext, AuditEntryModel, string> fluent = auditService.GenerateEntry<AuditEntryModel, string>("", DateTime.UtcNow);

            AuditEntryModel entry = fluent.WithDescription(description);

            Assert.AreEqual(description, entry.Description);
        }

        [Test]
        public void WithUserId_WhenUserIdIsNotNull_SetsUserId()
        {
            var userId = "Sample";
            AuditService<FakeContext> auditService = GenerateAuditService();
            FluentAudit<FakeContext, AuditEntryModel, string> fluent = auditService.GenerateEntry<AuditEntryModel, string>("", DateTime.UtcNow);

            AuditEntryModel entry = fluent.WithUserId(userId);

            Assert.AreEqual(userId, entry.UserId);
        }

        [Test]
        public void WithChanges_WhenChangesAreValid_SetsBeforeAndAfterJson()
        {
            var before = "before_changes";
            var after = "after_changes";
            AuditedOperationTypeEnum type = AuditedOperationTypeEnum.Update;
            var model = new ModelToAudit { Id = Guid.NewGuid().ToString() };

            var mockChanges = new Mock<IChangeDetectionService<FakeContext>>();
            mockChanges
                .Setup(c => c.GetChangeType(It.IsAny<EntityEntry>()))
                .Returns(type);

            mockChanges
                .Setup(c => c.SerializeEntityChanges(It.IsAny<AuditedOperationTypeEnum>(), It.IsAny<EntityEntry>()))
                .Returns((before, after));


            AuditService<FakeContext> auditService = GenerateAuditService(null, mockChanges.Object);
            FluentAudit<FakeContext, AuditEntryModel, string> fluent = auditService.GenerateEntry<AuditEntryModel, string>("", DateTime.UtcNow);

            AuditEntryModel entry = fluent.WithChangesFor(model);

            Assert.AreEqual(type, entry.Type);
            Assert.AreEqual(before, entry.EntityBeforeJson);
            Assert.AreEqual(after, entry.EntityAfterJson);
        }

        [Test]
        public void New_WhenGeneratedViaFactory_SetsEntityIdAndChanges()
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


            AuditService<FakeContext> auditService = GenerateAuditService(null, mockChanges.Object);
            var factory = auditService.GenerateForEntries<ModelToAudit, AuditEntryModel, string>(m => m.Id);
            AuditEntryModel entry = factory.GenerateEntry(model, DateTime.UtcNow);

            Assert.AreEqual(model.Id, entry.EntityId);
            Assert.AreEqual(type, entry.Type);
            Assert.AreEqual(before, entry.EntityBeforeJson);
            Assert.AreEqual(after, entry.EntityAfterJson);
        }

        [Test]
        public void Write_WhenAuditIsValid_WritesAuditToDatabase()
        {
            AuditedOperationTypeEnum type = AuditedOperationTypeEnum.Update;
            var model = new ModelToAudit();

            var mockChanges = new Mock<IChangeDetectionService<FakeContext>>();
            mockChanges
                .Setup(c => c.GetChangeType(It.IsAny<EntityEntry>()))
                .Returns(type);

            mockChanges
                .Setup(c => c.SerializeEntityChanges(It.IsAny<AuditedOperationTypeEnum>(), It.IsAny<EntityEntry>()))
                .Returns((string.Empty, string.Empty));

            var auditWriter = new AuditWriter<FakeContext>(Context);

            AuditService<FakeContext> auditService = GenerateAuditService(auditWriter, mockChanges.Object);
            var factory = auditService.GenerateForEntries<ModelToAudit, AuditEntryModel, string>(m => m.Id);
            var fluent = factory.GenerateEntry(model, DateTime.UtcNow);

            fluent.Write();
            Context.SaveChanges();

            Assert.AreEqual(1, Context.AuditEntries.Count());
        }
    }
}
