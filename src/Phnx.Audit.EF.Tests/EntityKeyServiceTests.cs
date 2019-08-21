using NUnit.Framework;
using Phnx.Audit.EF.Tests.Fakes;
using System;
using System.Collections.Generic;

namespace Phnx.Audit.EF.Tests
{
    public class EntityKeyServiceTests : ContextTestBase
    {
        [Test]
        public void GetPrimaryKeys_ForObjectWithOneKey_GetsKey()
        {
            var id = Guid.NewGuid().ToString();
            var model = new ModelToAudit
            {
                Id = id
            };
            var keyService = new EntityKeyService<FakeContext>(Context);

            var result = keyService.GetKey<ModelToAudit, string>(model);

            Assert.AreEqual(id, result);
        }

        [Test]
        public void GetPrimaryKeys_ForObjectWithMultipleKeys_GetsMultipleKeys()
        {
            var model = new MultiKeyModel
            {
                Id1 = Guid.NewGuid().ToString(),
                Id2 = Guid.NewGuid().ToString()
            };
            var keyService = new EntityKeyService<FakeContext>(Context);

            dynamic keyValues = keyService.GetKey<MultiKeyModel, object>(model);
            IDictionary<string, object> keys = keyValues;

            Assert.AreEqual(model.Id1, keys[nameof(MultiKeyModel.Id1)]);
            Assert.AreEqual(model.Id2, keys[nameof(MultiKeyModel.Id2)]);
        }

        [Test]
        public void GetPrimaryKeys_ForObjectWithDifferentKeyType_ThrowsInvalidCastException()
        {
            var model = new ModelToAudit
            {
                Id = Guid.NewGuid().ToString()
            };
            var keyService = new EntityKeyService<FakeContext>(Context);

            Assert.Throws<InvalidCastException>(() => keyService.GetKey<ModelToAudit, int>(model));
        }
    }
}
