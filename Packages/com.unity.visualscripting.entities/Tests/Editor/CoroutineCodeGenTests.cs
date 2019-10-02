using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Packages.VisualScripting.Editor.Stencils;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.TestTools;
using VisualScripting.Entities.Runtime;

namespace UnityEditor.VisualScriptingECSTests
{
    public struct UnitTestCoroutine : ICoroutine
    {
        public float DeltaTime { get; set; }
        public bool MoveNext()
        {
            return true;
        }
    }

    public class CoroutineCodeGenTests : EndToEndCodeGenBaseFixture
    {
        protected override bool CreateGraphOnStartup => true;

        IVariableModel SetupQuery(VSGraphModel graph, string name, IEnumerable<Type> components)
        {
            var query = graph.CreateComponentQuery(name);
            foreach (var component in components)
                query.AddComponent(Stencil, component.GenerateTypeHandle(Stencil), ComponentDefinitionFlags.None);
            return graph.CreateVariableNode(query, Vector2.zero);
        }

        static OnUpdateEntitiesNodeModel SetupOnUpdate(GraphModel graph, IHasMainOutputPort query)
        {
            var onUpdate = graph.CreateNode<OnUpdateEntitiesNodeModel>("On Update", Vector2.zero);
            graph.CreateEdge(onUpdate.InstancePort, query.OutputPort);
            return onUpdate;
        }

        [Test]
        public void TestSendEventInCoroutine([Values] CodeGenMode mode)
        {
            SetupTestGraphMultipleFrames(mode, graph =>
            {
                var query = SetupQuery(graph, "query", new[] { typeof(Translation) });
                var onUpdate = SetupOnUpdate(graph, query);
                onUpdate.CreateStackedNode<CoroutineNodeModel>("Wait", 0, setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });

                var eventTypeHandle = typeof(UnitTestEvent).GenerateTypeHandle(Stencil);
                var sendEvent = onUpdate.CreateStackedNode<SendEventNodeModel>("Send", 1, setup: n =>
                {
                    n.EventType = eventTypeHandle;
                });

                var entityType = typeof(Entity).GenerateTypeHandle(Stencil);
                var entityVar = graph.CreateVariableNode(onUpdate.FunctionParameterModels.Single(p =>
                    p.DataType == entityType), Vector2.zero);
                graph.CreateEdge(sendEvent.EntityPort, entityVar.OutputPort);

                var onEvent = graph.CreateNode<OnEventNodeModel>("On Event", preDefineSetup: n =>
                {
                    n.EventTypeHandle = eventTypeHandle;
                });
                graph.CreateEdge(onEvent.InstancePort, query.OutputPort);
                var setProperty = onEvent.CreateSetPropertyGroupNode(0);
                var member = new TypeMember(TypeHandle.Float, new List<string>
                {
                    nameof(Translation.Value), nameof(Translation.Value.x)
                });
                setProperty.AddMember(member);
                ((FloatConstantModel)setProperty.InputConstantsById[member.GetId()]).value = 10f;

                var translation = graph.CreateVariableNode(onEvent.FunctionParameterModels.Single(p =>
                    p.DataType == typeof(Translation).GenerateTypeHandle(Stencil)), Vector2.zero);
                graph.CreateEdge(setProperty.InstancePort, translation.OutputPort);
            },

                EachEntity((manager, i, e) =>
                {
                    manager.World.CreateSystem<InitializationSystemGroup>();
                    manager.AddComponentData(e, new Translation());
                }),

                // Init State
                EachEntity((manager, i, e) => {}),

                // Wait MoveNext
                EachEntity((manager, i, e) => {}),

                // Send event;
                EachEntity((manager, i, e) =>
                {
                    Assert.That(manager.GetComponentData<Translation>(e).Value.x, Is.EqualTo(10f));
                }));
        }

        [Test]
        public void TestCoroutineWithForEachContext([Values] CodeGenMode mode)
        {
            SetupTestGraphMultipleFrames(mode, graph =>
            {
                var translationQuery = SetupQuery(graph, "translationQuery", new[] { typeof(Translation) });
                var scaleQuery = SetupQuery(graph, "scaleQuery", new[] { typeof(Scale) });

                var onUpdate = SetupOnUpdate(graph, translationQuery);
                onUpdate.CreateStackedNode<CoroutineNodeModel>("Wait", setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });

                var forAllStack = graph.CreateLoopStack<ForAllEntitiesStackModel>(Vector2.zero);
                var forAllNode = forAllStack.CreateLoopNode(onUpdate, 1) as ForAllEntitiesNodeModel;
                Assert.That(forAllNode, Is.Not.Null);
                graph.CreateEdge(forAllNode.InputPort, scaleQuery.OutputPort);
                graph.CreateEdge(forAllStack.InputPort, forAllNode.OutputPort);

                var setProperty = forAllStack.CreateSetPropertyGroupNode(0);
                var member = new TypeMember(TypeHandle.Float, new List<string> { nameof(Scale.Value) });
                setProperty.AddMember(member);
                ((FloatConstantModel)setProperty.InputConstantsById[member.GetId()]).value = 10f;

                var scale = graph.CreateVariableNode(forAllStack.FunctionParameterModels.Single(p =>
                    p.DataType == typeof(Scale).GenerateTypeHandle(Stencil)), Vector2.zero);
                graph.CreateEdge(setProperty.InstancePort, scale.OutputPort);
            },
                EachEntity((manager, i, e) =>
                {
                    if (i % 2 == 0)
                        manager.AddComponentData(e, new Translation());
                    else
                        manager.AddComponentData(e, new Scale());
                }),
                EachEntity((manager, i, e) => {}),  // Init State
                EachEntity((manager, i, e) => {}),  // Wait MoveNext
                EachEntity((manager, i, e) => // ForEach set Scale
                {
                    if (manager.HasComponent<Scale>(e))
                        Assert.That(manager.GetComponentData<Scale>(e).Value, Is.EqualTo(10f));
                })
            );
        }

        [Test]
        public void TestCoroutineAccessComponents([Values] CodeGenMode mode)
        {
            SetupTestGraphMultipleFrames(mode, graph =>
            {
                var query = SetupQuery(graph, "query", new[] { typeof(Translation) });

                var onUpdate = SetupOnUpdate(graph, query);
                onUpdate.CreateStackedNode<CoroutineNodeModel>("Wait", setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });

                var setProperty = onUpdate.CreateSetPropertyGroupNode(1);
                var member = new TypeMember(TypeHandle.Float, new List<string>
                {
                    nameof(Translation.Value), nameof(Translation.Value.x)
                });
                setProperty.AddMember(member);
                ((FloatConstantModel)setProperty.InputConstantsById[member.GetId()]).value = 10f;

                var translation = graph.CreateVariableNode(onUpdate.FunctionParameterModels.Single(p =>
                    p.DataType == typeof(Translation).GenerateTypeHandle(Stencil)), Vector2.zero);
                graph.CreateEdge(setProperty.InstancePort, translation.OutputPort);
            },
                EachEntity((manager, i, e) => manager.AddComponentData(e, new Translation())),
                EachEntity((manager, i, e) => {}),  // Init State
                EachEntity((manager, i, e) => {}),  // Wait MoveNext
                EachEntity((manager, i, e) => Assert.That(manager.GetComponentData<Translation>(e).Value.x, Is.EqualTo(10f)))
            );
        }

        [Test]
        public void TestCoroutineAccessStaticValues([Values] CodeGenMode mode)
        {
            SetupTestGraphMultipleFrames(mode, graph =>
            {
                var query = SetupQuery(graph, "query", new[] { typeof(Translation) });

                var onUpdate = SetupOnUpdate(graph, query);
                onUpdate.CreateStackedNode<CoroutineNodeModel>("Wait", setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });

                var methodInfo = typeof(Time).GetMethod("get_timeScale");
                var timeScale = graph.CreateFunctionCallNode(methodInfo, Vector2.zero);
                var log = onUpdate.CreateStackedNode<LogNodeModel>("Log");
                graph.CreateEdge(log.InputPort, timeScale.OutputPort);
            },
                EachEntity((manager, i, e) => manager.AddComponentData(e, new Translation())),
                EachEntity((manager, i, e) => {}),  // Init State
                EachEntity((manager, i, e) => {}),  // Wait MoveNext
                EachEntity((manager, i, e) => LogAssert.Expect(LogType.Log, $"{Time.timeScale}"))
            );
        }

        [Test]
        public void TestGenerateUniqueCoroutineComponentsAndQueries([Values] CodeGenMode mode)
        {
            SetupTestGraph(mode, graph =>
            {
                var query = SetupQuery(graph, "query", new[] { typeof(Translation) });

                var onUpdate = SetupOnUpdate(graph, query);
                onUpdate.CreateStackedNode<CoroutineNodeModel>("Wait", setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });

                var onUpdate2 = SetupOnUpdate(graph, query);
                onUpdate2.CreateStackedNode<CoroutineNodeModel>("Wait 2", setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });
            },
                (manager, entityIndex, entity) => { manager.AddComponentData(entity, new Translation()); },
                (manager, entityIndex, entity) =>
                {
                    var coroutines = m_SystemType.GetNestedTypes()
                        .Where(t => t.Name.Contains("Coroutine"))
                        .ToList();
                    Assert.That(coroutines.Count, Is.EqualTo(2));
                    Assert.That(coroutines.Distinct().Count(), Is.EqualTo(coroutines.Count));

                    var queries = m_SystemType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                        .Where(f => f.FieldType == typeof(EntityQuery))
                        .ToList();
                    Assert.That(queries.Count, Is.EqualTo(4)); // 2 queries + 2 queries for coroutine initialization
                    Assert.That(queries.Distinct().Count(), Is.EqualTo(queries.Count));
                });
        }

        [Test]
        public void TestCoroutine([Values] CodeGenMode mode)
        {
            SetupTestGraphMultipleFrames(mode, graph =>
            {
                var query = SetupQuery(graph, "query", new[] { typeof(Translation) });

                var onUpdate = SetupOnUpdate(graph, query);
                onUpdate.CreateStackedNode<CoroutineNodeModel>("Wait", setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });
            },
                EachEntity((manager, i, e) =>
                {
                    manager.AddComponentData(e, new Translation());

                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(coroutineType, Is.Not.Null);
                    Assert.That(manager.HasComponent(e, coroutineType), Is.Not.True);
                }),
                EachEntity((manager, i, e) => // Init State
                {
                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(manager.HasComponent(e, coroutineType), Is.True);
                }),
                EachEntity((manager, i, e) => // Wait MoveNext
                {
                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(manager.HasComponent(e, coroutineType), Is.True);
                }),
                EachEntity((manager, i, e) => // Remove component
                {
                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(manager.HasComponent(e, coroutineType), Is.False);
                })
            );
        }

        [Test]
        public void TestCoroutineWithConnectedStack([Values] CodeGenMode mode)
        {
            SetupTestGraphMultipleFrames(mode, graph =>
            {
                var query = SetupQuery(graph, "query", new[] { typeof(Translation) });
                var onUpdate = SetupOnUpdate(graph, query);

                var connectedStack = graph.CreateStack(string.Empty, Vector2.down);
                connectedStack.CreateStackedNode<CoroutineNodeModel>("Wait", setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });

                graph.CreateEdge(connectedStack.InputPorts.First(), onUpdate.OutputPort);
            },
                EachEntity((manager, i, e) =>
                {
                    manager.AddComponentData(e, new Translation());

                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(coroutineType, Is.Not.Null);
                    Assert.That(manager.HasComponent(e, coroutineType), Is.Not.True);
                }),
                EachEntity((manager, i, e) => // Init State
                {
                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(manager.HasComponent(e, coroutineType), Is.True);
                }),
                EachEntity((manager, i, e) => // Wait MoveNext
                {
                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(manager.HasComponent(e, coroutineType), Is.True);
                }),
                EachEntity((manager, i, e) => // Remove component
                {
                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(manager.HasComponent(e, coroutineType), Is.False);
                })
            );
        }

        [Test]
        public void TestCoroutineExecutionStack([Values] CodeGenMode mode)
        {
            SetupTestGraphMultipleFrames(mode, graph =>
            {
                var query = SetupQuery(graph, "query", new[] { typeof(Translation) });

                var onUpdate = SetupOnUpdate(graph, query);
                var loopNode = onUpdate.CreateStackedNode<CoroutineNodeModel>("UnitTest", setup: n =>
                {
                    n.CoroutineType = typeof(UnitTestCoroutine).GenerateTypeHandle(Stencil);
                });

                var loopStack = graph.CreateLoopStack<CoroutineStackModel>(Vector2.down);
                graph.CreateEdge(loopStack.InputPort, loopNode.OutputPort);

                var setProperty = loopStack.CreateSetPropertyGroupNode(0);
                var member = new TypeMember(TypeHandle.Float, new List<string>
                {
                    nameof(Translation.Value), nameof(Translation.Value.x)
                });
                setProperty.AddMember(member);
                ((FloatConstantModel)setProperty.InputConstantsById[member.GetId()]).value = 10f;

                var translation = graph.CreateVariableNode(onUpdate.FunctionParameterModels.Single(p =>
                    p.DataType == typeof(Translation).GenerateTypeHandle(Stencil)), Vector2.zero);
                graph.CreateEdge(setProperty.InstancePort, translation.OutputPort);
            },
                EachEntity((manager, i, e) => manager.AddComponentData(e, new Translation())),
                EachEntity((manager, i, e) => {}),  // Init State
                EachEntity((manager, i, e) => {}),  // MoveNext -> Execute loop stack
                EachEntity((manager, i, e) => Assert.That(manager.GetComponentData<Translation>(e).Value.x, Is.EqualTo(10f)))
            );
        }

        [Test]
        public void TestCoroutineAccessingLocalVariable([Values] CodeGenMode mode)
        {
            SetupTestGraphMultipleFrames(mode, graph =>
            {
                var query = SetupQuery(graph, "query", new[] { typeof(Translation) });

                var onUpdate = SetupOnUpdate(graph, query);
                onUpdate.CreateStackedNode<CoroutineNodeModel>("Wait", setup: n =>
                {
                    n.CoroutineType = typeof(Wait).GenerateTypeHandle(Stencil);
                });

                var localVar = onUpdate.CreateFunctionVariableDeclaration("localVar", TypeHandle.Float);
                var localVarInstance = graph.CreateVariableNode(localVar, Vector2.zero);
                var log = onUpdate.CreateStackedNode<LogNodeModel>("Log");
                graph.CreateEdge(log.InputPort, localVarInstance.OutputPort);
            },
                EachEntity((manager, i, e) =>
                {
                    manager.AddComponentData(e, new Translation());

                    var coroutineType = m_SystemType.GetNestedTypes().First(t => t.Name.Contains("Coroutine"));
                    Assert.That(coroutineType, Is.Not.Null);
                    Assert.That(manager.HasComponent(e, coroutineType), Is.Not.True);
                }),
                EachEntity((manager, i, e) => Assert.Pass())
            );
        }
    }
}
