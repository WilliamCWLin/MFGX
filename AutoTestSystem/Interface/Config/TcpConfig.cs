using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Interface.Config
{
    public sealed class TcpConfig : IEquatable<TcpConfig>
    {

        public bool Toggle { get; private set; }

        public TcpConfig(bool toggle)
        {
            Toggle = toggle;
        }

        public TcpConfig With(bool? toggle = null)
        {
            return new TcpConfig(
                toggle ?? this.Toggle
            );
        }

        public override string ToString()
        {
            return string.Format("TcpConfig(Toggle={0})",
                Toggle);
        }

        #region Value Equality
        public bool Equals(TcpConfig other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Toggle.Equals(other.Toggle);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TcpConfig);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + Toggle.GetHashCode();
                return hash;
            }
        }
        #endregion
    }
}
