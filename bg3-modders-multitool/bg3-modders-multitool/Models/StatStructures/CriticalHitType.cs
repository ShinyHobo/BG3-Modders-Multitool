/// <summary>
/// Critical hit type
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    public class CriticalHitType : StatStructure
    {
        public string AcidFX { get; set; }
        public string BludgeoningFX { get; set; }
        public string ColdFX { get; set; }
        public string FireFX { get; set; }
        public string ForceFX { get; set; }
        public string LightningFX { get; set; }
        public string NecroticFX { get; set; }
        public string PiercingFX { get; set; }
        public string PoisonFX { get; set; }
        public string PsychicFX { get; set; }
        public string RadiantFX { get; set; }
        public string SlashingFX { get; set; }
        public string ThunderFX { get; set; }

        public override StatStructure Clone()
        {
            return (CriticalHitType)MemberwiseClone();
        }
    }
}
