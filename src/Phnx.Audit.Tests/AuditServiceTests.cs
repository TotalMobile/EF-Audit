using Newtonsoft.Json;
using NUnit.Framework;
using Phnx.Audit;
using Phnx.Audit.Models;
using Phnx.Audit.Tests;
using Phnx.Audit.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class AuditServiceTests
    {
        public FakeContext Context { get; set; }

        [SetUp]
        public void SetupContext()
        {
            Context = FakeContext.Create();
        }

        [TearDown]
        public void DestroyContext()
        {
            Context?.Dispose();
        }

        [Test]
        public void CreatingANewAuditEntry_WhenObjectIsCreated_InsertsAuditEntryToDatabase()
        {
            var audit = new AuditService<FakeContext>(Context);
            var modelToAudit = new ModelToAudit();
            Context.Add(modelToAudit);

            audit.AuditChanges(
                ctx => ctx.AuditEntries,
                modelToAudit,
                "Create",
                m => m.Id,
                Guid.NewGuid().ToString());

            Context.SaveChanges();

            var allEntries = Context.AuditEntries.ToList();

            Assert.AreEqual(allEntries.Count, 1);
        }

        [Test]
        public void CreatingANewAuditEntry_WhenObjectIsCreated_InsertsRelevantAuditEntry()
        {
            var audit = new AuditService<FakeContext>(Context);
            var modelToAudit = new ModelToAudit();
            Context.Add(modelToAudit);
            string userId = Guid.NewGuid().ToString();
            string description = "Sample Message";

            audit.AuditChanges(
                ctx => ctx.AuditEntries,
                modelToAudit,
                description,
                m => m.Id,
                userId);

            Context.SaveChanges();

            var newEntry = Context.AuditEntries.FirstOrDefault();

            Assert.AreEqual(userId, newEntry.UserId);
            Assert.AreEqual(AuditedOperationTypeEnum.Insert, newEntry.Type);
            Assert.IsNull(newEntry.EntityBeforeJson);
            Assert.IsNotNull(newEntry.EntityAfterJson);
            Assert.AreEqual(modelToAudit.Id, newEntry.EntityId);
            Assert.AreEqual(description, newEntry.Description);
            Assert.AreNotEqual(default(DateTime), newEntry.AuditedOn);
        }

        [Test]
        public void CreatingANewAuditEntry_WhenObjectIsUpdated_InsertsRelevantAuditEntry()
        {
            string oldName = "Old Name", newName = "New Name";
            var audit = new AuditService<FakeContext>(Context);
            var modelToAudit = new ModelToAudit { Name = oldName };
            Context.Add(modelToAudit);
            Context.SaveChanges();

            modelToAudit.Name = newName;
            Context.Update(modelToAudit);

            string userId = Guid.NewGuid().ToString();
            string description = "Updated";

            audit.AuditChanges(
                ctx => ctx.AuditEntries,
                modelToAudit,
                description,
                m => m.Id,
                userId);

            Context.SaveChanges();

            var newEntry = Context.AuditEntries.FirstOrDefault();

            Assert.AreEqual(userId, newEntry.UserId);
            Assert.AreEqual(AuditedOperationTypeEnum.Update, newEntry.Type);
            Assert.IsNotNull(newEntry.EntityBeforeJson);
            Assert.IsNotNull(newEntry.EntityAfterJson);
            Assert.AreEqual(modelToAudit.Id, newEntry.EntityId);
            Assert.AreEqual(description, newEntry.Description);
            Assert.AreNotEqual(default(DateTime), newEntry.AuditedOn);

            Dictionary<string, object> before = JsonConvert.DeserializeObject<Dictionary<string, object>>(newEntry.EntityBeforeJson);
            Dictionary<string, object> after = JsonConvert.DeserializeObject<Dictionary<string, object>>(newEntry.EntityAfterJson);

            Assert.AreEqual(1, before.Count);
            Assert.AreEqual(1, after.Count);
            Assert.AreEqual(nameof(ModelToAudit.Name), before.First().Key);
            Assert.AreEqual(after.First().Key, before.First().Key);

            Assert.AreEqual(oldName, before.First().Value);
            Assert.AreEqual(newName, after.First().Value);
        }
    }
}
