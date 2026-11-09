namespace Asteroids.Data.Tests
{
    /// <summary>
    /// The smallest concrete effect asset: the base settings with nothing drawn. In a file of its own name,
    /// because Unity looks a ScriptableObject's script up by file name when it creates an instance.
    /// </summary>
    internal sealed class TestEffectDefinition : EffectDefinition
    {
    }
}
