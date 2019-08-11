using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace Ternacode.Persistence.Example.Domain.UnitTest
{
    public class CustomAutoFixture : Fixture
    {
        public ICollection<object> Entities { get; set; }

        public CustomAutoFixture()
        {
            Customize(new AutoNSubstituteCustomization
            {
                ConfigureMembers = true
            });

            Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => Behaviors.Remove(b));

            Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public void AddEntities<T>(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Entities.Add(entity);
            }
        }
    }
}
