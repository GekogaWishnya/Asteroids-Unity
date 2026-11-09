using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Asteroids.Data.Tests
{
    internal sealed class TestTypeSource : IEnemyTypeSource
    {
        public TestTypeSource(params EnemyTypeDefinition[] types) => ReferencedTypes = types;

        public IEnumerable<EnemyTypeDefinition> ReferencedTypes { get; }
    }

    /// <summary>
    /// Creates assets in memory and destroys them all on <see cref="DestroyAll"/>. Private serialized fields are
    /// written through <see cref="SerializedObject"/>, the same way the inspector writes them.
    /// </summary>
    internal sealed class TestAssets
    {
        private readonly List<Object> _created = new();

        public EnemyTypeDefinition EnemyType(string name) => Create<EnemyTypeDefinition>(name);

        public TestEffectDefinition Effect(string name) => Create<TestEffectDefinition>(name);

        public static void SetEffects(Object owner, EffectDefinition hit, bool playHitOnFatal, EffectDefinition death)
        {
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("effects.hit").objectReferenceValue = hit;
            serialized.FindProperty("effects.playHitOnFatal").boolValue = playHitOnFatal;
            serialized.FindProperty("effects.death").objectReferenceValue = death;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public void DestroyAll()
        {
            foreach (var asset in _created)
                if (asset) Object.DestroyImmediate(asset);
            _created.Clear();
        }

        private T Create<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            _created.Add(asset);
            return asset;
        }
    }
}
