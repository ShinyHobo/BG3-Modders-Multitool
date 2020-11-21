/// <summary>
/// The stat structure model.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using System;
    using System.IO;

    public class StatStructure
    {
        /// <summary>
        /// Gets the file type.
        /// </summary>
        /// <param name="file">The path to the file.</param>
        /// <returns>The file type.</returns>
        public static Enums.StatStructures FileType(string file)
        {
            var filename = Path.GetFileNameWithoutExtension(file);
            if(filename == "Armor")
            {
                return Enums.StatStructures.Armor;
            }
            else if(filename == "Character")
            {
                return Enums.StatStructures.Character;
            }
            else if(filename == "Object")
            {
                return Enums.StatStructures.Object;
            }
            else if(filename == "Weapon")
            {
                return Enums.StatStructures.Weapon;
            }
            else if(filename == "Passive")
            {
                return Enums.StatStructures.PassiveData;
            }
            else if(filename.Contains("Spell_"))
            {
                return Enums.StatStructures.SpellData;
            }
            else if(filename.Contains("Status_"))
            {
                return Enums.StatStructures.StatusData;
            }
            throw new Exception("Stat structure file type not accounted for.");
        }
    }
}
