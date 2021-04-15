/// <summary>
/// The StringType model.
/// </summary>
namespace bg3_modders_multitool.Models.GameObjectTypes
{
    using bg3_modders_multitool.Enums;
    using System;

    public class StringType
    {
        public StringType() { }

        public StringType(string value, string type)
        {
            this.Value = value;
            this.Type = (TagStringType)Enum.Parse(typeof(TagStringType), type);
        }

        private TagStringType _type;

        public TagStringType Type {
            get { return _type; }
            set { _type = value; }
        }

        private string _value;

        public string Value {
            get { return _value; }
            set { _value = value; }
        }
    }
}
