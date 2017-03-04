using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;
using Xunit;

namespace XunitTestImmutable
{
    public class UnitTest1
    {
        [Fact]
        public void AddAdds()
        {
            // Arrange
            var list = ImmutableList<string>.Empty;

            // Act
            list = list.Add("Hello World");

            // Assert
            var item = Assert.Single(list);
            Assert.Equal("Hello World", item);
        }

        [Fact]
        public void CreateCreates()
        {
            // Arrange & Act
            var compilation = CSharpCompilation.Create("__unrealName");

            // Assert
            Assert.NotNull(compilation);
        }

        [Fact]
        public void GetGets()
        {
            // Arrange
            var compilation = CSharpCompilation.Create("__unrealName", references: GetReferences());

            // Act
            var typeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");

            // Assert
            Assert.NotNull(typeSymbol);
        }

        private static List<MetadataReference> GetReferences()
        {
            var applicationAssembly = Assembly.GetExecutingAssembly();
            var dependencyContext = DependencyContext.Load(applicationAssembly);
            var references = dependencyContext
                ?.CompileLibraries
                .SelectMany(library => library.ResolveReferencePaths())
                .ToList();

            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var metadataReferences = new List<MetadataReference>();
            foreach (var path in references ?? Enumerable.Empty<string>())
            {
                if (libraryPaths.Add(path))
                {
                    var metadataReference = CreateMetadataReference(path);
                    metadataReferences.Add(metadataReference);
                }
            }

            return metadataReferences;
        }

        private static MetadataReference CreateMetadataReference(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                return assemblyMetadata.GetReference(filePath: path);
            }
        }
    }
}
