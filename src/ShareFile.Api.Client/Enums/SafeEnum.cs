using System;

// ReSharper disable once CheckNamespace
namespace ShareFile.Api.Client.Models
{
    public interface ISafeEnum 
    {
        object Object { set; }
        string Value { get; set; }
    }

    public struct SafeEnum<TEnumSource> : ISafeEnum, IEquatable<SafeEnum<TEnumSource>>
        where TEnumSource : struct
    {
        public TEnumSource? Enum { get; set; }

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
            var instance = default(SafeEnum<TSource>);
            instance.Enum = enumSourceValue;

            return instance;
        }

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            if (Enum.HasValue)
            {
                return Enum.GetValueOrDefault().GetHashCode();
            }
            else
            {
                return _value == null ? 0 : _value.GetHashCode();
            }
        }
        
        public bool Equals(SafeEnum<TEnumSource> other)
        {
            if(Enum.HasValue)
            {
                if(other.Enum.HasValue)
                {
                    return Enum.Value.Equals(other.Enum.Value);
                }
                else if(other._value != null)
                {
                    return string.Equals(Enum.Value.ToString(), other._value, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }
            else if(_value != null)
            {
                if(other.Enum.HasValue)
                {
                    return string.Equals(_value, other.Enum.Value.ToString(), StringComparison.OrdinalIgnoreCase);
                }
                else if(other._value != null)
                {
                    return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }
            return other.Enum == null && other._value == null;
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

        public static implicit operator SafeEnum<TEnumSource>(TEnumSource source)
        {
            return Create(source);
        }

        public static SafeEnum<TEnumSource> operator |(SafeEnum<TEnumSource> safeEnum, TEnumSource source)
        {
            int left = (int)(object)(safeEnum.Enum ?? default(TEnumSource));
            int right = (int)(object)source;
            int result = left | right;
            return Create((TEnumSource)(object)result);
        }

        public static SafeEnum<TEnumSource> operator &(SafeEnum<TEnumSource> safeEnum, TEnumSource source)
        {
            int left = (int)(object)(safeEnum.Enum ?? default(TEnumSource));
            int right = (int)(object)source;
            int result = left & right;
            return Create((TEnumSource)(object)result);
        }
    }
}
