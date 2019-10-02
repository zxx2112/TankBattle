using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEditor.VisualScripting.Editor.SmartSearch;
using UnityEngine;

namespace UnityEditor.VisualScripting.Model.Stencils
{
    public class EcsTypeSearcherDatabase : TypeSearcherDatabase
    {
        readonly Stencil m_Stencil;

        public EcsTypeSearcherDatabase(Stencil stencil, List<ITypeMetadata> typesMetadata)
            : base(stencil, typesMetadata)
        {
            m_Stencil = stencil;
        }

        public EcsTypeSearcherDatabase AddComponents()
        {
            RegisterTypesFromMetadata((items, metadata) =>
            {
                var type = metadata.TypeHandle.Resolve(m_Stencil);
                if (typeof(IComponentData).IsAssignableFrom(type) ||
                    typeof(ISharedComponentData).IsAssignableFrom(type))
                {
                    var item = new TypeSearcherItem(metadata.TypeHandle, metadata.FriendlyName);
                    var root = typeof(IComponentData).IsAssignableFrom(type) ? "Component Data" : "Shared Component Data";
                    var path = BuildPath(root, metadata);
                    items.AddAtPath(item, path);

                    return true;
                }

                return false;
            });
            return this;
        }

        public EcsTypeSearcherDatabase AddMonoBehaviourComponents()
        {
            RegisterTypesFromMetadata((items, metadata) =>
            {
                var type = metadata.TypeHandle.Resolve(m_Stencil);
                if (EcsStencil.IsValidGameObjectComponentType(type))
                {
                    var item = new TypeSearcherItem(metadata.TypeHandle, metadata.FriendlyName);
                    var root = "GameObject Components";
                    var path = BuildPath(root, metadata);
                    items.AddAtPath(item, path);

                    return true;
                }

                return false;
            });
            return this;
        }

        public override TypeSearcherDatabase AddGraphs()
        {
            RegisterTypes(items =>
            {
                var assetGUIDs = AssetDatabase.FindAssets($"t:{typeof(VSGraphAssetModel).Name}");
                var graphs = assetGUIDs.Select(p =>
                    AssetDatabase.LoadAssetAtPath<VSGraphAssetModel>(AssetDatabase.GUIDToAssetPath(p))
                    ).Where(x => x.GraphModel?.Stencil.GetType() != typeof(MacroStencil)
                        && x.GraphModel?.Stencil.GetType() != typeof(EcsStencil));

                foreach (var meta in graphs.Select(graph => graph.GenerateTypeHandle(m_Stencil).GetMetadata(m_Stencil)))
                {
                    var graphItem = new TypeSearcherItem(meta.TypeHandle, meta.Name);
                    items.TryAddGraphItem(graphItem, meta);
                }
            });
            return this;
        }

        static string BuildPath(string parentName, ITypeMetadata meta)
        {
            return parentName + "/" + meta.Namespace.Replace(".", "/");
        }
    }
}
