using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public ChangeDetectionService MakeService()
        {
            return new ChangeDetectionService(new JsonSerializerService());
        }

        [Test]
        public void GetChangeType_WhenModelIsNotTracked_ThrowsArgumentException()
        {
            ChangeDetectionService cds = MakeService();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(new ModelToAudit());

            Assert.Throws<ArgumentException>(() => cds.GetChangeType(entity));
        }

        [Test]
        public void GetChangeType_WhenModelIsAdded_ReturnsInsert()
        {
            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel(false);
            Context.Add(model);
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            AuditedOperationTypeEnum changeType = cds.GetChangeType(entity);

            Assert.AreEqual(AuditedOperationTypeEnum.Insert, changeType);
        }

        [Test]
        public void GetChangeType_WhenModelIsRemoved_ReturnsDelete()
        {
            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel(false);
            Context.Remove(model);
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            AuditedOperationTypeEnum changeType = cds.GetChangeType(entity);

            Assert.AreEqual(AuditedOperationTypeEnum.Delete, changeType);
        }

        [Test]
        public void GetChangeType_WhenModelIsAttachedWithId_ReturnsUpdate()
        {
            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel(false);

            Context.Attach(model);
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            AuditedOperationTypeEnum changeType = cds.GetChangeType(entity);

            Assert.AreEqual(AuditedOperationTypeEnum.Update, changeType);
        }

        [Test]
        public void GetChangeType_WhenModelMemberIsUpdated_ReturnsUpdate()
        {
            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel();

            model.Name = Guid.NewGuid().ToString();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);
            AuditedOperationTypeEnum changeType = cds.GetChangeType(entity);

            Assert.AreEqual(AuditedOperationTypeEnum.Update, changeType);
        }

        [Test]
        public void SerializeEntityChanges_WhenEntityIsInserted_SerializesEntityToAfter()
        {
            ChangeDetectionService cds = MakeService(); ;
            ModelToAudit model = GenerateModel();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            (string before, string after) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Insert, entity);

            Assert.IsNull(before);
            Assert.IsNotNull(after);
        }

        [Test]
        public void SerializeEntityChanges_WhenEntityIsDeleted_SerializesEntityToBefore()
        {
            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            (string before, string after) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Delete, entity);

            Assert.IsNotNull(before);
            Assert.IsNull(after);
        }

        [Test]
        public void SerializeEntityChanges_WhenEntityIsUpdated_SerializesBeforeAndAfter()
        {
            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            (string before, string after) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Update, entity);

            Assert.IsNotNull(before);
            Assert.IsNotNull(after);
        }

        [Test]
        public void SerializeEntityChanges_WhenEntityIsUpdated_SerializesOnlyChangedMembers()
        {
            var newName = "New Name";
            string originalName;

            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel();
            originalName = model.Name;
            model.Name = newName;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            (string beforeJson, string afterJson) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Update, entity);
            Dictionary<string, object> before = JsonConvert.DeserializeObject<Dictionary<string, object>>(beforeJson);
            Dictionary<string, object> after = JsonConvert.DeserializeObject<Dictionary<string, object>>(afterJson);

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
            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel(false);
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            Assert.Throws<ArgumentOutOfRangeException>(() => cds.SerializeEntityChanges((AuditedOperationTypeEnum)5, entity));
        }

        [Test]
        public void SerializeEntityChanges_WhenModelIsSelfReferencing_IgnoresSelfReferencing()
        {
            ChangeDetectionService cds = MakeService();
            ModelToAudit model = GenerateModel();
            model.OneToOneChildModel = new OneToOneChildModel { Parent = model };

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ModelToAudit> entity = Context.Entry(model);

            (string _, string afterJson) = cds.SerializeEntityChanges(AuditedOperationTypeEnum.Insert, entity);
            Dictionary<string, object> after = JsonConvert.DeserializeObject<Dictionary<string, object>>(afterJson);

            var oneToOne = after[nameof(model.OneToOneChildModel)] as JObject;

            Assert.IsNotNull(oneToOne);
            Assert.IsNull(oneToOne[nameof(OneToOneChildModel.Parent)]);
        }
    }
}
