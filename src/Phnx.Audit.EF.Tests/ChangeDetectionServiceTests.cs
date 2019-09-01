using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using Phnx.Audit.EF.Models;
using Phnx.Audit.EF.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Phnx.Audit.EF.Tests
{
    public class ChangeDetectionServiceTests : ContextTestBase
    {
        [Test]
        public void GetChangeType_WhenModelIsNotTracked_ThrowsArgumentException()
        {
            var cds = new ChangeDetectionService();
            var entity = Context.Entry(new ModelToAudit());

            Assert.Throws<ArgumentException>(() => cds.GetChangeType(entity));
        }

        [Test]
        public void GetChangeType_WhenModelIsAdded_ReturnsInsert()
        {
            var cds = new ChangeDetectionService();
            var model = GenerateModel(false);
            Context.Add(model);
            var entity = Context.Entry(model);

            var changeType = cds.GetChangeType(entity);

            Assert.AreEqual(AuditedOperationTypeEnum.Insert, changeType);
        }

        [Test]
        public void GetChangeType_WhenModelIsRemoved_ReturnsDelete()
        {
            var cds = new ChangeDetectionService();
            var model = GenerateModel(false);
            Context.Remove(model);
            var entity = Context.Entry(model);

            var changeType = cds.GetChangeType(entity);

            Assert.AreEqual(AuditedOperationTypeEnum.Delete, changeType);
        }

        [Test]
        public void GetChangeType_WhenModelIsAttachedWithId_ReturnsUpdate()
        {
            var cds = new ChangeDetectionService();
            var model = GenerateModel(false);

            Context.Attach(model);
            var entity = Context.Entry(model);

            var changeType = cds.GetChangeType(entity);

            Assert.AreEqual(AuditedOperationTypeEnum.Update, changeType);
        }

        [Test]
        public void GetChangeType_WhenModelMemberIsUpdated_ReturnsUpdate()
        {
            var cds = new ChangeDetectionService();
            var model = GenerateModel();

            model.Name = Guid.NewGuid().ToString();
            var entity = Context.Entry(model);
            var changeType = cds.GetChangeType(entity);

            Assert.AreEqual(AuditedOperationTypeEnum.Update, changeType);
        }

        [Test]
        public void SerializeEntityChanges_WhenEntityIsInserted_SerializesEntityToAfter()
        {
            var cds = new ChangeDetectionService();
            var model = GenerateModel();
            var entity = Context.Entry(model);

            var (before, after) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Insert, entity);

            Assert.IsNull(before);
            Assert.IsNotNull(after);
        }

        [Test]
        public void SerializeEntityChanges_WhenEntityIsDeleted_SerializesEntityToBefore()
        {
            var cds = new ChangeDetectionService();
            var model = GenerateModel();
            var entity = Context.Entry(model);

            var (before, after) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Delete, entity);

            Assert.IsNotNull(before);
            Assert.IsNull(after);
        }

        [Test]
        public void SerializeEntityChanges_WhenEntityIsUpdated_SerializesBeforeAndAfter()
        {
            var cds = new ChangeDetectionService();
            var model = GenerateModel();
            var entity = Context.Entry(model);

            var (before, after) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Update, entity);

            Assert.IsNotNull(before);
            Assert.IsNotNull(after);
        }

        [Test]
        public void SerializeEntityChanges_WhenEntityIsUpdated_SerializesOnlyChangedMembers()
        {
            string newName = "New Name";
            string originalName;

            var cds = new ChangeDetectionService();
            var model = GenerateModel();
            originalName = model.Name;
            model.Name = newName;
            var entity = Context.Entry(model);

            var (beforeJson, afterJson) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Update, entity);
            var before = JsonConvert.DeserializeObject<Dictionary<string, object>>(beforeJson);
            var after = JsonConvert.DeserializeObject<Dictionary<string, object>>(afterJson);

            Assert.IsTrue(before.Count == 1);
            Assert.AreEqual(nameof(model.Name), before.First().Key);
            Assert.AreEqual(originalName, before.First().Value);

            Assert.IsTrue(after.Count == 1);
            Assert.AreEqual(nameof(model.Name), after.First().Key);
            Assert.AreEqual(newName, after.First().Value);
        }

        [Test]
        public void SerializeEntityChanges_WhenChangeTypeInvalid_ThrowsArgumentOutOfRangeException()
        {
            var cds = new ChangeDetectionService();
            var model = GenerateModel(false);
            var entity = Context.Entry(model);

            Assert.Throws<ArgumentOutOfRangeException>(() => cds.SerializeEntityChanges((AuditedOperationTypeEnum)5, entity));
        }
    }
}
