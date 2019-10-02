using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Entities;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEditor.VisualScripting.Model.Translators;
using UnityEditor.VisualScriptingTests;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.VisualScripting;

namespace UnityEditor.VisualScriptingECSTests
{
    public abstract class EndToEndCodeGenBaseFixture : BaseFixture
    {
        public enum CodeGenMode
        {
            NoJobs,
            Jobs
        }

        World m_World;
        protected EntityManager m_EntityManager;

        protected override Type CreatedGraphType => typeof(EcsStencil);
        protected Type m_SystemType;
        protected const int k_EntityCount = 100;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            m_World = new World("test");
            m_EntityManager = m_World.EntityManager;
        }

        [TearDown]
        public override void TearDown()
        {
            m_World.Dispose();

            base.TearDown();

            GC.Collect();
        }

        public delegate void StepDelegate(EntityManager entityManager, Entity[] entities);

        internal static StepDelegate EachEntity(Action<EntityManager, int, Entity> del)
        {
            return (entityManager, entities) =>
            {
                for (var index = 0; index < entities.Length; index++)
                {
                    Entity entity = entities[index];
                    del(entityManager, index, entity);
                }
            };
        }

        protected void SetupTestGraphMultipleFrames(CodeGenMode mode, Action<VSGraphModel> setupGraph, params StepDelegate[] setup)
        {
            m_SystemType = null;
            setupGraph(GraphModel);
            try
            {
                m_SystemType = CompileGraph(mode);
            }
            finally
            {
                GC.Collect();
            }

            if (setup.Length > 0)
            {
                Entity[] entities = new Entity[k_EntityCount];
                for (var index = 0; index < entities.Length; index++)
                    entities[index] = m_EntityManager.CreateEntity();
                setup[0](m_EntityManager, entities);
            }

            for (int i = 1; i < setup.Length; i++)
            {
                TestSystem(m_SystemType);

                setup[i](m_EntityManager, m_EntityManager.GetAllEntities().ToArray());
            }
        }

        protected void SetupTestGraph(CodeGenMode mode, Action<VSGraphModel> setupGraph, Action<EntityManager, int, Entity> setup, Action<EntityManager, int, Entity> checkWorld)
        {
            SetupTestGraphMultipleFrames(mode, setupGraph, EachEntity(setup), EachEntity(checkWorld));
        }

        protected void SetupTestSystem(Type systemType, Func<EntityManager, Entity> setup, Action<Entity, EntityManager> checkWorld)
        {
            var entities = setup(m_EntityManager);
            TestSystem(systemType);
            checkWorld(entities, m_EntityManager);
        }

        Type CompileGraph(CodeGenMode mode)
        {
            RoslynEcsTranslator translator = (RoslynEcsTranslator)GraphModel.CreateTranslator();
            translator.AllowNoJobsFallback = false;

            // because of the hack in the translator constructor, override right after
            ((EcsStencil)Stencil).UseJobSystem = mode == CodeGenMode.Jobs;

            CompilationResult results = GraphModel.Compile(AssemblyType.Memory, translator,
                CompilationOptions.LiveEditing);

            if (results?.sourceCode != null && results.sourceCode.Length != 0)
            {
                LogAssert.Expect(LogType.Log, new Regex("using.*"));
                Debug.Log(results.sourceCode[(int)SourceCodePhases.Initial]);
            }

            Assert.That(results?.status, Is.EqualTo(CompilationStatus.Succeeded),
                () => $"Compilation failed, errors: {String.Join("\n", results?.errors)}");

            return EcsStencil.LiveCompileGraph(GraphModel, results, includeVscriptingAssemblies: true);
        }

        void TestSystem(Type systemType)
        {
            ComponentSystemBase system = m_World.CreateSystem(systemType);

            // Force System.OnUpdate to be executed
            var field = typeof(ComponentSystemBase).GetField("m_AlwaysUpdateSystem", BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(system, true);

            Assert.DoesNotThrow(system.Update);

            system.Complete();
            var endFrame = m_World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            endFrame.Update();
            endFrame.Complete();
        }
    }
}
