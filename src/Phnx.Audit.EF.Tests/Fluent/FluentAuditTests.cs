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
        public void WithDescription_WhenDescriptionIsNotNull_SetsDescription()
        {
            var description = "Sample";
            AuditService<FakeContext> auditService = GenerateAuditService();
            FluentAudit<FakeContext, AuditEntryModel, ModelToAudit> fluent = auditService.GenerateEntry<AuditEntryModel, ModelToAudit>(new ModelToAudit());

            AuditEntryModel entry = fluent.WithDescription(description);

            Assert.That(description, Is.EqualTo(entry.Description));
        }

        [Test]
        public void WithUserId_WhenUserIdIsNotNull_SetsUserId()
        {
            var userId = "Sample";
            AuditService<FakeContext> auditService = GenerateAuditService();
            FluentAudit<FakeContext, AuditEntryModel, ModelToAudit> fluent = auditService.GenerateEntry<AuditEntryModel, ModelToAudit>(new ModelToAudit());

            AuditEntryModel entry = fluent.WithUserId(userId);

            Assert.That(userId, Is.EqualTo(entry.UserId));
        }

        [Test]
        public void WithChanges_WhenChangesAreValid_SetsBeforeAndAfterJson()
        {
            var before = "before_changes";
            var after = "after_changes";
            AuditedOperationTypeEnum type = AuditedOperationTypeEnum.Update;
            var model = new ModelToAudit { Id = Guid.NewGuid().ToString() };

            var mockChanges = new Mock<IChangeDetectionService>();
            mockChanges
                .Setup(c => c.GetChangeType(It.IsAny<EntityEntry>()))
                .Returns(type);

            mockChanges
                .Setup(c => c.SerializeEntityChanges(It.IsAny<AuditedOperationTypeEnum>(), It.IsAny<EntityEntry>()))
                .Returns((before, after));


            AuditService<FakeContext> auditService = GenerateAuditService(mockChanges.Object);
            AuditEntryModel entry = auditService.GenerateEntry<AuditEntryModel, ModelToAudit>(model);

            Assert.That(type, Is.EqualTo(entry.Type));
            Assert.That(before, Is.EqualTo(entry.EntityBeforeJson));
            Assert.That(after, Is.EqualTo(entry.EntityAfterJson));
        }

        [Test]
        public void Write_WhenAuditIsValid_WritesAuditToDatabase()
        {
            AuditedOperationTypeEnum type = AuditedOperationTypeEnum.Update;
            var model = new ModelToAudit();

            var mockChanges = new Mock<IChangeDetectionService>();
            mockChanges
                .Setup(c => c.GetChangeType(It.IsAny<EntityEntry>()))
                .Returns(type);

            mockChanges
                .Setup(c => c.SerializeEntityChanges(It.IsAny<AuditedOperationTypeEnum>(), It.IsAny<EntityEntry>()))
                .Returns((string.Empty, string.Empty));

            AuditService<FakeContext> auditService = GenerateAuditService(mockChanges.Object);
            var fluent = auditService.GenerateEntry<AuditEntryModel, ModelToAudit>(model);

            Context.SaveChanges();

            Assert.That(1, Is.EqualTo(Context.AuditEntries.Count()));
        }
    }
}
