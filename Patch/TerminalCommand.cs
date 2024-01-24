using HarmonyLib;

namespace DeathTweaks;

[HarmonyPatch(typeof(Terminal), "InputText")] [HarmonyWrapSafe]
internal class TerminalCommand
{
    [HarmonyPrefix]
    private static bool Prefix(Terminal __instance)
    {
        if (!modEnabled.Value) return true;
        var text = __instance.m_input.text;
        if (text.ToLower().Equals($"{typeof(Plugin).Namespace.ToLower()} reset"))
        {
            context.Config.Reload();
            context.Config.Save();

            __instance.AddString(text);
            __instance.AddString($"{context.Info.Metadata.Name} config reloaded");
            return false;
        }

        return true;
    }
}