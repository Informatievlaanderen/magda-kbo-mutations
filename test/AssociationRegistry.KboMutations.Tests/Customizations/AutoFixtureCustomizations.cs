using AutoFixture;

namespace AssociationRegistry.KboMutations.Tests.Customizations;

public static class CustomFixture
{
    public static IFixture Default => new Fixture().CustomizeAll();

    public static IFixture CustomizeAll(this IFixture fixture)
    {
        return fixture;
    }
}