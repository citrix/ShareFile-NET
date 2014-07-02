using System;

namespace ShareFile.Api.Models
{
    public interface ISafeEnum 
    {
        object Object { set; }
        string Value { get; set; }
    }

    public struct SafeEnum<TEnumSource> : ISafeEnum, IEquatable<SafeEnum<TEnumSource>>
        where TEnumSource : struct
    {
        private TEnumSource? _enum;
        public TEnumSource? Enum
        {
            get { return _enum; }
            set
            {
                _enum = value;
                if (null != value)
                {
                    _value = value.Value.ToString();
                }
            }
        }

        public object Object
        {
            set { Enum = (TEnumSource)value; }
        }

        private string _value;
        public string Value
        {
            get { return _value ?? (Enum.HasValue ? Enum.Value.ToString() : null); }
            set { _value = value; }
        }

        public static SafeEnum<TSource> Create<TSource>(TSource enumSourceValue)
            where TSource : struct
        {
            var instance = Activator.CreateInstance<SafeEnum<TSource>>();
            instance.Enum = enumSourceValue;

            return instance;
        }

        #region equality
        public bool Equals(SafeEnum<TEnumSource> other)
        {
            if (Enum.HasValue && other.Enum.HasValue)
            {
                return Enum.Value.Equals(other.Enum.Value);
            }
            else if (Value != null && other.Value != null)
            {
                return String.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is SafeEnum<TEnumSource>)
            {
                return this.Equals((SafeEnum<TEnumSource>)obj);
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(SafeEnum<TEnumSource> safeEnum1, SafeEnum<TEnumSource> safeEnum2)
        {
            return safeEnum1.Equals(safeEnum2);
        }

        public static bool operator !=(SafeEnum<TEnumSource> safeEnum1, SafeEnum<TEnumSource> safeEnum2)
        {
            return !safeEnum1.Equals(safeEnum2);
        }

        public static implicit operator TEnumSource?(SafeEnum<TEnumSource> safeEnum)
        {
            return safeEnum.Enum;
        }
        #endregion
    }
}
