using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Entities;
using UnityEditor.Searcher;
using UnityEditor.VisualScripting.Editor;
using UnityEditor.VisualScripting.Editor.SmartSearch;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEditor.VisualScripting.Model.Compilation;
using UnityEditor.VisualScriptingTests;
using UnityEngine;
using VisualScripting.Model.Stencils;

namespace UnityEditor.VisualScriptingECSTests.Stencils
{
    public class EcsTypeSearcherDatabaseTests : BaseFixture
    {
        protected override bool CreateGraphOnStartup => false;

        sealed class TestStencil : Stencil
        {
            public override ISearcherDatabaseProvider GetSearcherDatabaseProvider()
            {
                return new ClassSearcherDatabaseProvider(this);
            }

            [CanBeNull]
            public override IBuilder Builder => null;
        }

        struct TestComponent : IComponentData {}
        struct TestSharedComponent : ISharedComponentData {}

        [TestCase(typeof(MacroStencil), 0)]
        [TestCase(typeof(ClassStencil), 1)]
        [TestCase(typeof(EcsStencil), 0)]
        public void TestGraphs(Type stencilType, int expectedResult)
        {
            const string graphName = "TestGraphs";
            const string path = "Assets/" + graphName + ".asset";

            m_Store.Dispatch(new CreateGraphAssetAction(stencilType, "TestGraph", path));

            var db = new EcsTypeSearcherDatabase(Stencil, new List<ITypeMetadata>())
                .AddGraphs()
                .Build();

            var result = db.Search(graphName, out _);
            Assert.AreEqual(expectedResult, result.Count);

            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void TestComponents()
        {
            var stencil = ScriptableObject.CreateInstance<TestStencil>();
            var source = new List<ITypeMetadata>
            {
                stencil.GenerateTypeHandle(typeof(TestComponent)).GetMetadata(stencil),
                stencil.GenerateTypeHandle(typeof(TestSharedComponent)).GetMetadata(stencil),
                stencil.GenerateTypeHandle(typeof(string)).GetMetadata(stencil)
            };
            var db = new EcsTypeSearcherDatabase(stencil, source).AddComponents().Build();
            ValidateHierarchy(db.Search("", out _), new[]
            {
                new SearcherItem("Component Data", "", new List<SearcherItem>
                {
                    new SearcherItem("UnityEditor", "", new List<SearcherItem>
                    {
                        new SearcherItem("VisualScriptingECSTests", "", new List<SearcherItem>
                        {
                            new SearcherItem("Stencils", "", new List<SearcherItem>
                            {
                                new TypeSearcherItem(
                                    typeof(TestComponent).GenerateTypeHandle(stencil),
                                    typeof(TestComponent).FriendlyName()
                                )
                            })
                        })
                    })
                }),
                new SearcherItem("Shared Component Data", "", new List<SearcherItem>
                {
                    new SearcherItem("UnityEditor", "", new List<SearcherItem>
                    {
                        new SearcherItem("VisualScriptingECSTests", "", new List<SearcherItem>
                        {
                            new SearcherItem("Stencils", "", new List<SearcherItem>
                            {
                                new TypeSearcherItem(
                                    typeof(TestSharedComponent).GenerateTypeHandle(stencil),
                                    typeof(TestSharedComponent).FriendlyName()
                                )
                            })
                        })
                    })
                })
            });
        }

        static void ValidateHierarchy(IReadOnlyList<SearcherItem> result, IEnumerable<SearcherItem> hierarchy)
        {
            var index = 0;
            TraverseHierarchy(result, hierarchy, ref index);
            Assert.AreEqual(result.Count, index);
        }

        static void TraverseHierarchy(
            IReadOnlyList<SearcherItem> result,
            IEnumerable<SearcherItem> hierarchy,
            ref int index
        )
        {
            foreach (var item in hierarchy)
            {
                Assert.AreEqual(item.Name, result[index].Name);

                if (item.Parent != null)
                    Assert.AreEqual(item.Parent.Name, result[index].Parent.Name);

                index++;

                TraverseHierarchy(result, item.Children, ref index);
            }
        }
    }
}
