using HarmonyLib;

namespace DeathTweaks;

[HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
file class SkillsOnDeath
{
    [HarmonyPrefix]
    private static bool Prefix()
    {
        if (!modEnabled.Value) return true;
        return reduceSkills.Value;
    }
}