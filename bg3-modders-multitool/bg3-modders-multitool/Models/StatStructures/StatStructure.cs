/// <summary>
/// The stat structure model.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using System;
    using System.IO;

    public abstract class StatStructure
    {
        /// <summary>
        /// Gets the file type.
        /// </summary>
        /// <param name="file">The path to the file.</param>
        /// <returns>The file type.</returns>
        public static Enums.StatStructure FileType(string file)
        {
            var filename = Path.GetFileNameWithoutExtension(file);
            if(filename == "Armor")
            {
                return Enums.StatStructure.Armor;
            }
            else if(filename == "Character")
            {
                return Enums.StatStructure.Character;
            }
            else if(filename == "Object")
            {
                return Enums.StatStructure.Object;
            }
            else if(filename == "Weapon")
            {
                return Enums.StatStructure.Weapon;
            }
            else if(filename == "Passive")
            {
                return Enums.StatStructure.PassiveData;
            }
            else if(filename.Contains("Spell_"))
            {
                return Enums.StatStructure.SpellData;
            }
            else if(filename.Contains("Status_"))
            {
                return Enums.StatStructure.StatusData;
            }
            throw new Exception("Stat structure file type not accounted for.");
        }

        /// <summary>
        /// Generates a new stat structure.
        /// </summary>
        /// <param name="fileType">The type of stat structure to create.</param>
        /// <returns>The new stat structure.</returns>
        public static StatStructure New(Enums.StatStructure fileType, string name)
        {
            StatStructure newEntry = null;
            switch (fileType)
            {
                case Enums.StatStructure.Armor:
                    newEntry = new Armor();
                    break;
                case Enums.StatStructure.Character:
                    newEntry = new Character();
                    break;
                case Enums.StatStructure.Object:
                    newEntry = new Object();
                    break;
                case Enums.StatStructure.PassiveData:
                    newEntry = new PassiveData();
                    break;
                case Enums.StatStructure.SpellData:
                    newEntry = new SpellData();
                    break;
                case Enums.StatStructure.StatusData:
                    newEntry = new StatusData();
                    break;
                case Enums.StatStructure.Weapon:
                    newEntry = new Weapon();
                    break;
                default:
                    throw new Exception($"Stats structure {fileType} not recognized.");
            }
            newEntry.Entry = name.Replace("\"", "");
            return newEntry;
        }

        public Enums.StatStructure Type { get; set; }

        public string Entry { get; set; }
    }
}
