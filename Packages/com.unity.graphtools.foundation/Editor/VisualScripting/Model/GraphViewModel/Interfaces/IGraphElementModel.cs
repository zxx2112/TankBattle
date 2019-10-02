using System;
using UnityEngine;

namespace UnityEditor.VisualScripting.GraphViewModel
{
    public interface IGraphElementModel : ICapabilitiesModel
    {
        ScriptableObject SerializableAsset { get; }
        IGraphAssetModel AssetModel { get; }
        IGraphModel GraphModel { get; }
        string GetId();
    }

    public interface IUndoRedoAware
    {
        void UndoRedoPerformed();
    }
}
