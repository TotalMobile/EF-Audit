using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NUnit.Framework;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;
using Phnx.Audit.EF.Tests.Fakes;
using System;
using System.Linq;

namespace Phnx.Audit.EF.Tests.Fluent
{
    public class FluentAuditTests : ContextTestBase
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
            var fluent = auditService.GenerateEntry<ModelToAudit, AuditEntryModel, string>(model);

            fluent.AddToDatabase();
            Context.SaveChanges();

            Assert.AreEqual(1, Context.AuditEntries.Count());
        }
    }
}
