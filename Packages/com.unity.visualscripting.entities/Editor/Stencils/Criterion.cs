using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.VisualScripting.Model.Stencils
{
    [Serializable]
    public class Criterion
    {
        [SerializeField]
        AbstractNodeAsset m_Value;

        public TypeHandle ObjectType;
        public TypeMember Member;
        public BinaryOperatorKind Operator;

        public IVariableModel Value
        {
            get => m_Value.Model as IVariableModel;
            set => m_Value = value.NodeAssetReference;
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        [SuppressMessage("ReSharper", "BaseObjectGetHashCodeCallInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ ObjectType.GetHashCode();
                hashCode = (hashCode * 397) ^ Member.GetHashCode();
                hashCode = (hashCode * 397) ^ Operator.GetHashCode();
                if (m_Value != null)
                    hashCode = (hashCode * 397) ^ m_Value.GetHashCode();
                return hashCode;
            }
        }

        public Criterion Clone()
        {
            return new Criterion
            {
                Value = (Value != null) ? Object.Instantiate(m_Value).Model as IVariableModel : null,
                ObjectType = ObjectType,
                Member = Member,
                Operator = Operator
            };
        }
    }
}
