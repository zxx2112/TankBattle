using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VisualScripting.Editor;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = UnityEditor.VisualScripting.Editor.Blackboard;

namespace Packages.VisualScripting.Editor.Elements
{
    public class ComponentQueriesRow : VisualElement
    {
        public static readonly string BlackboardEcsProviderTypeName = typeof(BlackboardEcsProvider).Name;

        public ComponentQueriesRow(IReadOnlyCollection<ComponentQueryDeclarationModel> componentQueryDeclarationModels,
                                   Blackboard blackboard,
                                   Stencil stencil,
                                   Blackboard.RebuildCallback rebuildCallback)
        {
            AddToClassList("componentQueriesRow");

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "BlackboardECS.uss"));
            // @TODO: This might need to be reviewed in favor of a better / more scalable approach (non preprocessor based)
            // that would ideally bring the same level of backward/forward compatibility and/or removed when a 2013 beta version lands.
#if UNITY_2019_3_OR_NEWER
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "BlackboardECS.2019.3.uss"));
#endif

            IReadOnlyCollection<ComponentQueryDeclarationModel> activeQueryDeclarationModels = componentQueryDeclarationModels;

            State state = blackboard.Store.GetState();

            var componentsSubSection = new ComponentsSubSection(stencil, activeQueryDeclarationModels, blackboard);
            Add(componentsSubSection);
            if (state.EditorDataModel.ShouldExpandBlackboardRowUponCreation($"{BlackboardEcsProviderTypeName}/{componentsSubSection.GetType().Name}"))
                componentsSubSection.Expanded = true;

            var criteriaSubSection = new CriteriaSubSection(stencil, activeQueryDeclarationModels, blackboard);
            Add(criteriaSubSection);
            if (state.EditorDataModel.ShouldExpandBlackboardRowUponCreation($"{BlackboardEcsProviderTypeName}/{criteriaSubSection.GetType().Name}"))
                criteriaSubSection.Expanded = true;

            userData = "ComponentQueriesRow";
        }
    }
}
