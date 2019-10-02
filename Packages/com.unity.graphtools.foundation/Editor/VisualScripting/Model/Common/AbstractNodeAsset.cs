using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.VisualScripting.Model
{
    public abstract class AbstractNodeAsset : ScriptableObject
    {
        public abstract INodeModel Model { get; }
    }

    public class NodeAssetListAdapter<T> : IList<T>, IReadOnlyList<T> where T : class, INodeModel
    {
        List<AbstractNodeAsset> m_NodeAssets;
        public NodeAssetListAdapter(List<AbstractNodeAsset> nodeAssets) => m_NodeAssets = nodeAssets;

        public void Add(T item)
        {
            m_NodeAssets.Add(item?.NodeAssetReference);
        }

        public void Clear()
        {
            m_NodeAssets.Clear();
        }

        public bool Contains(T item)
        {
            return m_NodeAssets.Contains(item?.NodeAssetReference);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_NodeAssets.Select(asset => asset.Model).OfType<T>().ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return m_NodeAssets.Remove(item?.NodeAssetReference);
        }

        public int Count => m_NodeAssets.Count;
        public bool IsReadOnly { get; set; }

        public int IndexOf(T item)
        {
            return m_NodeAssets.IndexOf(item?.NodeAssetReference);
        }

        public void Insert(int index, T item)
        {
            m_NodeAssets.Insert(index, item?.NodeAssetReference);
        }

        public void RemoveAt(int index)
        {
            m_NodeAssets.RemoveAt(index);
        }

        public T this[int index]
        {
            get => m_NodeAssets[index]?.Model as T;
            set => m_NodeAssets[index] = value?.NodeAssetReference;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(m_NodeAssets);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        struct Enumerator : IEnumerator<T>
        {
            List<AbstractNodeAsset>.Enumerator m_Enumerator;

            public T Current => m_Enumerator.Current != null ? m_Enumerator.Current.Model as T : null;
            object IEnumerator.Current => Current;

            public Enumerator(List<AbstractNodeAsset> nodeAssets) => m_Enumerator = nodeAssets.GetEnumerator();

            public bool MoveNext() => m_Enumerator.MoveNext();
            public void Reset() => ((IEnumerator)m_Enumerator).Reset();
            public void Dispose() => m_Enumerator.Dispose();
        }
    }
}
