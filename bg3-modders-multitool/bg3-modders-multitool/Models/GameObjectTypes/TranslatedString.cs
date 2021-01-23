/// <summary>
/// The TranslatedString model.
/// </summary>
namespace bg3_modders_multitool.Models.GameObjectTypes
{
    using System;

    public class TranslatedString
    {
        private string _translatedString;

        public TranslatedString(string translatedString)
        {
            _translatedString = translatedString;
        }

        public static implicit operator string(TranslatedString translatedString)
        {
            if (translatedString == null)
                return null;
            return translatedString.ToString();
        }

        public static implicit operator TranslatedString(string translatedString)
        {
            if (translatedString == null)
                return null;
            return new TranslatedString(translatedString);
        }

        public override string ToString()
        {
            return _translatedString;
        }

        public static TranslatedString FromString(string translatedString)
        {
            return translatedString;
        }

        public int IndexOf(string value, StringComparison comparisonType)
        {
            return _translatedString.IndexOf(value, comparisonType);
        }
    }
}
