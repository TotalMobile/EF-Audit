using NUnit.Framework;
using Phnx.Audit.Tests.Fakes;
using System;

namespace Phnx.Audit.Tests
{
    public abstract class ContextTestBase
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

        protected ModelToAudit GenerateModel(bool modelIsInDatabase = true)
        {
            var model = new ModelToAudit
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            if (modelIsInDatabase)
            {
                Context.Add(model);
                Context.SaveChanges();
            }

            return model;
        }
    }
}
