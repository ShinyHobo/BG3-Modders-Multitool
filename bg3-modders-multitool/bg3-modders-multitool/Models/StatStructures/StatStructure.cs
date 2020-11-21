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
        public static StatStructure New(Enums.StatStructure fileType)
        {
            switch (fileType)
            {
                case Enums.StatStructure.Armor:
                    return new Armor();
                case Enums.StatStructure.Character:
                    return new Character();
                case Enums.StatStructure.Object:
                    return new Object();
                case Enums.StatStructure.PassiveData:
                    return new PassiveData();
                case Enums.StatStructure.SpellData:
                    return new SpellData();
                case Enums.StatStructure.StatusData:
                    return new StatusData();
                case Enums.StatStructure.Weapon:
                    return new Weapon();
                default:
                    throw new Exception($"Stats structure {fileType} not recognized.");
            }
        }

        public Enums.StatStructure Type { get; set; }
    }
}
