using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest
{
    public class CustomAutoFixture : Fixture
    {
        public CustomAutoFixture()
        {
            Customize(new AutoNSubstituteCustomization
            {
                ConfigureMembers = true
            });
        }
    }
}
