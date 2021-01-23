/// <summary>
/// The LSString model.
/// </summary>
namespace bg3_modders_multitool.Models.GameObjectTypes
{
    using System;

    public class LSString
    {
        private string _lSString;

        public LSString(string lSString)
        {
            _lSString = lSString;
        }

        public static implicit operator string(LSString lSString)
        {
            if (lSString == null)
                return null;
            return lSString.ToString();
        }

        public static implicit operator LSString(string lSString)
        {
            if (lSString == null)
                return null;
            return new LSString(lSString);
        }

        public override string ToString()
        {
            return _lSString;
        }

        public static LSString FromString(string lSString)
        {
            return lSString;
        }

        public int IndexOf(string value, StringComparison comparisonType)
        {
            return _lSString.IndexOf(value, comparisonType);
        }
    }
}
