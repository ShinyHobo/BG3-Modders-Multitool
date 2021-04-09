/// <summary>
/// The GameObject attribute model.
/// </summary>
namespace bg3_modders_multitool.Models.GameObjects
{
    public class GameObjectAttribute
    {
        public GameObjectAttribute(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        private string _name;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        private object _value;

        public object Value { 
            get { return _value; }
            set { _value = value; }
        }
    }
}
