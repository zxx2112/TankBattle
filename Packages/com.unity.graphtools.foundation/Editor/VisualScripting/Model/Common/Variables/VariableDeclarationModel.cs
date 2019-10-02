using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model.NodeAssets;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace UnityEditor.VisualScripting.Model
{
    [Serializable]
    public class VariableDeclarationModel : ScriptableObject, IVariableDeclarationModel, IRenamableModel, IObjectReference, IExposeTitleProperty
    {
        [SerializeField]
        TypeHandle m_DataType;
        [SerializeField]
        VariableType m_VariableType;
        [SerializeField]
        GraphModel m_GraphModel;
        [SerializeField]
        bool m_IsExposed;
        [SerializeField]
        string m_Tooltip;
        [SerializeField]
        AbstractNodeAsset m_FunctionAsset;

        [SerializeField, Obsolete("Kept for data migration only")]
        FunctionModel m_FunctionModel;

        internal void MigrateFunctionModel()
        {
#pragma warning disable 618
            if (!m_FunctionAsset && m_FunctionModel != null)
            {
                m_FunctionAsset = m_FunctionModel.NodeAssetReference;
                m_FunctionModel = null;
                EditorUtility.SetDirty(this);
            }
#pragma warning restore 618
        }

        [SerializeField]
        AbstractNodeAsset m_InitializationModel;
        [SerializeField]
        int m_Modifiers;

        public virtual CapabilityFlags Capabilities
        {
            get
            {
                CapabilityFlags caps = CapabilityFlags.Selectable | CapabilityFlags.Movable | CapabilityFlags.Droppable;
                if (!(VariableType == VariableType.FunctionParameter
                      && (FunctionModel is IEventFunctionModel || FunctionModel is LoopStackModel)))
                    caps |= CapabilityFlags.Deletable | CapabilityFlags.Modifiable;
                if (!IsFunctionParameter || FunctionModel != null && FunctionModel.AllowChangesToModel)
                    caps |= CapabilityFlags.Renamable;
                return caps;
            }
        }

        public VariableFlags variableFlags;

        public ModifierFlags Modifiers
        {
            get => (ModifierFlags)m_Modifiers;
            set => m_Modifiers = (int)value;
        }

        public string Title => name.Nicify();

        public string Name => name;

        public string VariableName
        {
            get => TypeSystem.CodifyString(name);
            protected set
            {
                if (name != value)
                    name = ((VSGraphModel)GraphModel).GetUniqueName(value);
            }
        }

        public VariableType VariableType
        {
            get => m_VariableType;
            protected set => m_VariableType = value;
        }

        public string VariableString => IsExposed ? "Exposed variable" : "Variable";
        //public string dataTypeString => (dataType == typeof(ThisType) ? (graphModel)?.friendlyScriptName ?? string.Empty : dataType.FriendlyName());

        public TypeHandle DataType
        {
            get => m_DataType;
            set
            {
                if (m_DataType == value)
                    return;
                m_DataType = value;
                if (m_InitializationModel)
                {
                    Undo.DestroyObjectImmediate(m_InitializationModel);
                    (m_InitializationModel.Model as NodeModel)?.Destroy();
                }
                m_InitializationModel = null;
                if (m_GraphModel.Stencil.GetVariableInitializer().RequiresInspectorInitialization(this))
                    CreateInitializationValue();
            }
        }

        public bool IsExposed
        {
            get => m_IsExposed;
            set => m_IsExposed = value;
        }

        public string Tooltip
        {
            get => m_Tooltip;
            set => m_Tooltip = value;
        }

        public ScriptableObject SerializableAsset => this;
        public IGraphAssetModel AssetModel => GraphModel.AssetModel;

        public IGraphModel GraphModel
        {
            get => m_GraphModel;
            set => m_GraphModel = (GraphModel)value;
        }

        public string GetId()
        {
            return GetInstanceID().ToString();
        }

        public IEnumerable<INodeModel> FindReferencesInGraph()
        {
            return GraphModel.NodeModels.OfType<VariableNodeModel>().Where(v => ReferenceEquals(v.DeclarationModel, this));
        }

        public void Rename(string newName)
        {
            SetNameFromUserName(newName);
            ((VSGraphModel)GraphModel).LastChanges.RequiresRebuild = true;
        }

        bool IsFunctionParameter => VariableType == VariableType.FunctionParameter;

        public IFunctionModel FunctionModel
        {
            get => (m_FunctionAsset ? m_FunctionAsset.Model : null) as IFunctionModel;
            protected set => m_FunctionAsset = value?.NodeAssetReference;
        }

        public IHasVariableDeclaration Owner
        {
            get
            {
                if (m_FunctionAsset)
                    return (IHasVariableDeclaration)m_FunctionAsset.Model;
                return (IHasVariableDeclaration)m_GraphModel;
            }
            set
            {
                var model = value as FunctionModel;
                if (model != null)
                    m_FunctionAsset = (FunctionAsset)model.NodeAssetReference;
                else
                    m_GraphModel = (GraphModel)value;
            }
        }

        public IConstantNodeModel InitializationModel
        {
            get => m_InitializationModel != null ? (IConstantNodeModel)m_InitializationModel.Model : null;
            protected set => m_InitializationModel = value.NodeAssetReference;
        }

        public void CreateInitializationValue()
        {
            if (GraphModel.Stencil.GetConstantNodeModelType(DataType) != null)
            {
                InitializationModel = ((VSGraphModel)GraphModel).CreateConstantNode(
                    name + "_init",
                    DataType,
                    Vector2.zero,
                    SpawnFlags.Default | SpawnFlags.Orphan);

                Utility.SaveAssetIntoObject(InitializationModel.SerializableAsset, (Object)AssetModel);
            }
        }

        public static T Create<T>(string variableName, TypeHandle dataType, bool isExposed,
            GraphModel graph, VariableType variableType, ModifierFlags modifierFlags, FunctionModel functionModel,
            VariableFlags variableFlags = VariableFlags.None,
            IConstantNodeModel initializationModel = null) where T : VariableDeclarationModel
        {
            VariableDeclarationModel decl = CreateDeclarationNoUndoRecord(typeof(T), variableName, dataType, isExposed, graph, variableType, modifierFlags,
                functionModel, variableFlags, initializationModel);
            Undo.RegisterCreatedObjectUndo(decl, "Declare variable");
            return (T)decl;
        }

        public static VariableDeclarationModel Create(string variableName, TypeHandle dataType, bool isExposed,
            GraphModel graph, VariableType variableType, ModifierFlags modifierFlags, FunctionModel functionModel,
            IConstantNodeModel initializationModel = null)
        {
            return Create<VariableDeclarationModel>(variableName, dataType, isExposed, graph, variableType, modifierFlags, functionModel, initializationModel: initializationModel);
        }

        public static T CreateDeclarationNoUndoRecord<T>(string variableName, TypeHandle dataType, bool isExposed,
            GraphModel graph, VariableType variableType, ModifierFlags modifierFlags, FunctionModel functionModel,
            VariableFlags variableFlags,
            IConstantNodeModel initializationModel = null, SpawnFlags spawnFlags = SpawnFlags.Default) where T : VariableDeclarationModel
        {
            Type type = typeof(T);
            return (T)CreateDeclarationNoUndoRecord(type, variableName, dataType, isExposed, graph, variableType, modifierFlags, functionModel, variableFlags,
                initializationModel, spawnFlags);
        }

        public static VariableDeclarationModel CreateDeclarationNoUndoRecord(Type type, string variableName, TypeHandle dataType, bool isExposed,
            GraphModel graph, VariableType variableType, ModifierFlags modifierFlags, FunctionModel functionModel,
            VariableFlags variableFlags,
            IConstantNodeModel initializationModel = null, SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            Assert.IsNotNull(graph);
            Assert.IsNotNull(graph.AssetModel);

            var decl = (VariableDeclarationModel)CreateInstance(type);
            SetupDeclaration(variableName, dataType, isExposed, graph, variableType, modifierFlags, variableFlags, functionModel, decl);
            if (initializationModel != null)
                decl.InitializationModel = initializationModel;
            else if (!spawnFlags.IsOrphan())
                decl.CreateInitializationValue();

            if (spawnFlags.IsSerializable())
            {
                ((VSGraphModel)graph).LastChanges.ChangedElements.Add(decl);
                Utility.SaveAssetIntoObject(decl, (Object)graph.AssetModel);
            }

            return decl;
        }

        internal static void SetupDeclaration<T>(string variableName, TypeHandle dataType, bool isExposed, GraphModel graph, VariableType variableType, ModifierFlags modifierFlags, VariableFlags variableFlags, FunctionModel functionModel, T decl) where T : VariableDeclarationModel
        {
            decl.GraphModel = graph;
            decl.DataType = dataType;
            decl.VariableName = variableName;
            decl.IsExposed = isExposed;
            decl.VariableType = variableType;
            decl.Modifiers = modifierFlags;
            decl.variableFlags = variableFlags;
            decl.FunctionModel = functionModel;
        }

        public static VariableDeclarationModel CreateNoUndoRecord(string variableName, TypeHandle dataType, bool isExposed,
            GraphModel graph, VariableType variableType, ModifierFlags modifierFlags, FunctionModel functionModel, VariableFlags variableFlags, IConstantNodeModel initializationModel, SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            return CreateDeclarationNoUndoRecord<VariableDeclarationModel>(variableName, dataType, isExposed, graph, variableType, modifierFlags, functionModel, variableFlags, initializationModel, spawnFlags);
        }

        public void SetNameFromUserName(string userName)
        {
            string newName = userName.ToUnityNameFormat();
            if (string.IsNullOrWhiteSpace(newName))
                return;

            Undo.RegisterCompleteObjectUndo(this, "Rename Graph Variable");
            VariableName = newName;
        }

        bool Equals(VariableDeclarationModel other)
        {
            return base.Equals(other) && m_DataType.Equals(other.m_DataType) && m_VariableType == other.m_VariableType && m_IsExposed == other.m_IsExposed;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((VariableDeclarationModel)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ m_DataType.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)m_VariableType;
                hashCode = (hashCode * 397) ^ m_IsExposed.GetHashCode();
                return hashCode;
            }
        }

        public Object ReferencedObject => this;
        public string TitlePropertyName => "m_Name";

        public void UseDeclarationModelCopy(ConstantNodeModel constantModel)
        {
            EditorUtility.CopySerializedManagedFieldsOnly(constantModel.SerializableAsset, m_InitializationModel);
        }
    }
}
