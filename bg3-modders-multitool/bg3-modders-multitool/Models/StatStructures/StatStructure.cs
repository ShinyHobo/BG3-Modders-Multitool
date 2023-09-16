/// <summary>
/// The stat structure model.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
            if (filename == "Armor")
            {
                return Enums.StatStructure.Armor;
            }
            else if (filename == "Character")
            {
                return Enums.StatStructure.Character;
            }
            else if (filename == "Object")
            {
                return Enums.StatStructure.Object;
            }
            else if (filename == "Weapon")
            {
                return Enums.StatStructure.Weapon;
            }
            else if (filename == "Passive")
            {
                return Enums.StatStructure.PassiveData;
            }
            else if (filename.Contains("Spell_"))
            {
                return Enums.StatStructure.SpellData;
            }
            else if (filename.Contains("Status_"))
            {
                return Enums.StatStructure.StatusData;
            }
            else if (filename == "Interrupt")
            {
                return Enums.StatStructure.Interrupt;
            }
            else if (filename == "CriticalHitTypeData")
            {
                return Enums.StatStructure.CriticalHitTypeData;
            }
            else if (filename == "CriticalHitTypes")
            {
                return Enums.StatStructure.CriticalHitTypes;
            }
            throw new Exception($"Stat structure file type '{file}' not accounted for.");
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

                case Enums.StatStructure.Interrupt:
                    newEntry = new Interrupt();
                    break;

                case Enums.StatStructure.CriticalHitTypeData:
                    newEntry = new CriticalHitTypeData();
                    break;

                case Enums.StatStructure.CriticalHitTypes:
                    newEntry = new CriticalHitType();
                    break;

                default:
                    throw new Exception($"Stats structure {fileType} not recognized.");
            }
            newEntry.Entry = name.Replace("\"", "");
            return newEntry;
        }

        public abstract StatStructure Clone();

        /// <summary>
        /// Inherit properties from a structure.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <param name="statStructures">The list of stat structures to search through.</param>
        public void InheritProperties(string line, List<StatStructure> statStructures)
        {
            try
            {
                var usingEntry = line.Substring(6).Replace("\"", "");
                var match = statStructures.FirstOrDefault(ss => ss?.Entry == usingEntry);
                if (match != null)
                {
                    var clone = match.Clone();
                    clone.Entry = Entry;
                    clone.Type = Type;
                    clone.Using = usingEntry;
                    statStructures.Remove(this);
                    statStructures.Add(clone);
                }
            }
            catch(Exception ex)
            {
                GeneralHelper.WriteToConsole($"{ex.Message}\n{ex.StackTrace}");
            }
            
        }

        /// <summary>
        /// Use reflection to set property value from data file line.
        /// </summary>
        /// <param name="line">The line text to parse.</param>
        public void LoadProperty(string line)
        {
            var paramPair = line.Substring(5).Replace("\" \"", "|").Replace("\"", "").Split(new[] { '|' }, 2);
            try
            {
                if (!string.IsNullOrEmpty(paramPair[1]))
                {
                    var property = GetType().GetProperty(paramPair[0].Replace(" ", ""));
                    var propertyType = property.PropertyType;
                    if (propertyType.IsEnum)
                    {
                        property.SetValue(this, Enum.Parse(property.PropertyType, paramPair[1].Replace(" ", "")), null);
                    }
                    else if (propertyType == typeof(Guid))
                    {
                        property.SetValue(this, Guid.Parse(paramPair[1]), null);
                    }
                    else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        List<string> paramList = null;
                        if (paramPair[1].Contains(';'))
                        {
                            paramList = paramPair[1].Split(';').ToList();
                        }
                        else
                        {
                            paramList = paramPair[1].Split(',').ToList();
                        }

                        Type itemType = propertyType.GetGenericArguments().First();
                        if (itemType == typeof(Guid))
                        {
                            property.SetValue(this, paramList.Select(Guid.Parse).ToList(), null);
                        }
                        else if (itemType == typeof(string))
                        {
                            property.SetValue(this, paramList, null);
                        }
                        else
                        {
                            var enums = paramList.Select(p => Enum.Parse(itemType, p)).ToList();
                            var cast = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(new Type[] { itemType }).Invoke(null, new object[] { enums });
                            var enumList = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(new Type[] { itemType }).Invoke(null, new object[] { cast });
                            property.SetValue(this, Convert.ChangeType(enumList, property.PropertyType), null);
                        }
                    }
                    else if (propertyType == typeof(bool))
                    {
                        property.SetValue(this, Convert.ChangeType(paramPair[1] == "Yes", property.PropertyType), null);
                    }
                    else
                    {
                        property.SetValue(this, Convert.ChangeType(paramPair[1], property.PropertyType), null);
                    }
                }
            }
            catch
            {
                // This can usually be fixed by adding the Modifier data to the given StatStructure type
                #if DEBUG
                //Services.GeneralHelper.WriteToConsole(Resources.ErrorParsingProperty, line, Enum.GetName(Type.GetType(), Type), ex.Message);
                using (System.IO.StreamWriter writetext = new System.IO.StreamWriter($"Development\\{Enum.GetName(Type.GetType(), Type)}_{paramPair[0]}.txt", true))
                {
                    writetext.WriteLine($"{paramPair[1].Replace(";",",")},");
                }
                #endif
            }
        }

        public Enums.StatStructure Type { get; set; }
        public string Entry { get; set; }
        public string Using { get; set; }
    }
}