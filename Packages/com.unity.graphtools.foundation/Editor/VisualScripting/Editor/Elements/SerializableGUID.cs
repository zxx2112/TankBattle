using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.VisualScripting.Editor
{
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public struct SerializableGUID
    {
        [FieldOffset(0)]
        GUID m_GUID;

        [SerializeField]
        [FieldOffset(0)]
        ulong m_Value0;
        [SerializeField]
        [FieldOffset(8)]
        ulong m_Value1;

        public GUID GUID
        {
            get => m_GUID;
            set => m_GUID = value;
        }

        public override string ToString()
        {
            return m_GUID.ToString();
        }

        public static implicit operator GUID(SerializableGUID sGuid) => sGuid.m_GUID;
        public static implicit operator SerializableGUID(GUID guid) => new SerializableGUID{m_GUID = guid};
    }
}
