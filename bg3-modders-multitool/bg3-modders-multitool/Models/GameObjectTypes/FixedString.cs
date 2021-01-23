/// <summary>
/// The FixedString model.
/// </summary>
namespace bg3_modders_multitool.Models.GameObjectTypes
{
    using System;

    public class FixedString
    {
        private string _fixedString;

        public FixedString(string fixedString)
        {
            _fixedString = fixedString;
        }

        public static implicit operator string(FixedString fixedString)
        {
            if (fixedString == null)
                return null;
            return fixedString.ToString();
        }

        public static implicit operator FixedString(string fixedString)
        {
            if (fixedString == null)
                return null;
            return new FixedString(fixedString);
        }

        public override string ToString()
        {
            return _fixedString;
        }

        public static FixedString FromString(string fixedString)
        {
            return fixedString;
        }

        public int IndexOf(string value, StringComparison comparisonType)
        {
            return _fixedString.IndexOf(value, comparisonType);
        }
    }
}
