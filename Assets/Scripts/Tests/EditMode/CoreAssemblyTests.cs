using System;
using System.Linq;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Guards the architecture rule the whole project rests on: the game logic
    /// assembly must stay compilable without Unity. The asmdef enforces it at
    /// compile time; this test makes a regression visible in the test report.
    /// </summary>
    public sealed class CoreAssemblyTests
    {
        [Test]
        public void CoreAssembly_HasNoUnityReferences()
        {
            var unityReferences = typeof(Direction).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .Where(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)
                            || name.StartsWith("UnityEditor", StringComparison.Ordinal))
                .ToArray();

            Assert.That(unityReferences, Is.Empty,
                "Game.Core must stay free of Unity references. Found: " + string.Join(", ", unityReferences));
        }
    }
}
