using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Packages.VisualScripting.Editor.Stencils;
using UnityEditor.VisualScripting.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.VisualScripting.Model.Stencils
{
    [Flags]
    public enum ComponentDefinitionFlags
    {
        None = 0,
        Subtract = 1 << 0,
        Shared = 1 << 1
    }

    [Serializable]
    public class ComponentDefinition
    {
        public TypeHandle TypeHandle;
        public bool Subtract;
        public bool IsShared;
    }

    [PublicAPI]
    public class ComponentQueryDeclarationModel : VariableDeclarationModel, ICriteriaModelContainer
    {
        [SerializeField]
        QueryContainer m_QueryContainer;

        [FormerlySerializedAs("m_Criteria")]
        [SerializeField]
        List<CriteriaModel> m_CriteriaModels;

        public bool ExpandOnCreateUI { get; set; }

        public QueryContainer Query
        {
            get => m_QueryContainer ?? (m_QueryContainer = new QueryContainer("<empty>"));
            private set => m_QueryContainer = value;
        }

        public IReadOnlyList<CriteriaModel> CriteriaModels => m_CriteriaModels ?? (m_CriteriaModels = new List<CriteriaModel>());

        public void AddCriteriaModelNoUndo(CriteriaModel criteriaModel)
        {
            m_CriteriaModels.Add(criteriaModel);
            UpdateTakenNames();
        }

        public void InsertCriteriaModelNoUndo(int index, CriteriaModel criteriaModel)
        {
            m_CriteriaModels.Insert(index, criteriaModel);
            UpdateTakenNames();
        }

        public void RemoveCriteriaModelNoUndo(CriteriaModel criteriaModel)
        {
            m_CriteriaModels.Remove(criteriaModel);
            UpdateTakenNames();
        }

        public int IndexOfCriteriaModel(CriteriaModel criteriaModel)
        {
            return m_CriteriaModels.IndexOf(criteriaModel);
        }

        void UpdateTakenNames()
        {
            m_TakenNames = CriteriaModels.Select(x => x.Name).ToHashSet();
        }

        public bool AddTakenName(string proposedName) => m_TakenNames.Add(proposedName);

        public void AddComponent(Stencil stencil, TypeHandle typeHandle, ComponentDefinitionFlags flags)
        {
            var componentDefinition = new ComponentDefinition
            {
                TypeHandle = typeHandle,
                Subtract = flags.HasFlag(ComponentDefinitionFlags.Subtract),
                IsShared = flags.HasFlag(ComponentDefinitionFlags.Shared)
            };
            if (Query.Find(componentDefinition, out _) == null)
            {
                Query.AddComponent(Query.RootGroup, new QueryComponent(componentDefinition));
                ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(this);
                EditorUtility.SetDirty(this);
            }
        }

        public void RemoveComponent(ComponentDefinition componentDefinition)
        {
            var queryComponent = Query.Find(componentDefinition, out var owner);
            if (queryComponent != null)
            {
                Undo.RegisterCompleteObjectUndo(this, "Remove Component From Query");
                Query.RemoveComponent(owner, queryComponent.Component.TypeHandle);
                ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(this);
                ((VSGraphModel)GraphModel).LastChanges.DeletedElements++;
                EditorUtility.SetDirty(this);
            }
        }

        public void SetName(string newName)
        {
            Undo.RegisterCompleteObjectUndo(this, "Change Component Query Name");
            name = newName;
            ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(this);
            EditorUtility.SetDirty(this);
        }

        public void ChangeComponentUsage(ComponentDefinition componentDefinition, bool subtract)
        {
            QueryComponent foundQueryPart = Query.Find(componentDefinition, out _);
            if (foundQueryPart != null)
            {
                Undo.RegisterCompleteObjectUndo(this, "Change Component Usage Type");
                componentDefinition.Subtract = subtract;
                foundQueryPart.Component = componentDefinition;
                ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(this);
                EditorUtility.SetDirty(this);
            }
        }

        public void MoveComponent(ComponentDefinition componentDefinition, ComponentDefinition targetComponentDefinition, bool insertAtEnd, out int oldIndex, out int newIndex)
        {
            Undo.RegisterCompleteObjectUndo(this, "Move Component In Query");
            Query.ReorderComponent(componentDefinition, targetComponentDefinition, insertAtEnd, out oldIndex, out newIndex);
            ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(this);
            EditorUtility.SetDirty(this);
        }

        public void ChangeComponentType(ComponentDefinition componentDefinition, TypeHandle typeHandle)
        {
            var foundQueryPart = Query.Find(componentDefinition, out _);
            if (foundQueryPart != null)
            {
                if (!Query.HasType(typeHandle))
                {
                    Undo.RegisterCompleteObjectUndo(this, "Change Component Type");
                    componentDefinition.TypeHandle = typeHandle;
                    foundQueryPart.Component = componentDefinition;
                    ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(this);
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public void OnEnable()
        {
            UpdateTakenNames();
        }

        public IEnumerable<QueryComponent> Components => Query != null ? Query.GetFlattenedComponentDefinitions() : Enumerable.Empty<QueryComponent>();

        HashSet<string> m_TakenNames;
        public IReadOnlyCollection<string> TakenNames => m_TakenNames ?? (m_TakenNames = new HashSet<string>());
    }
}
