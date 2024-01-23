using HarmonyLib;

namespace DeathTweaks;

[HarmonyPatch(typeof(Game), nameof(Game.FindSpawnPoint))]
file class FindSpawnPoint
{
    [HarmonyPrefix]
    private static bool Prefix(ref Vector3 point, ref bool usedLogoutPoint, bool ___m_firstSpawn, ref bool __result)
    {
        if (!modEnabled.Value || ___m_firstSpawn)
            return true;

        if (spawnAtStart.Value)
        {
            usedLogoutPoint = false;

            Vector3 a;
            if (ZoneSystem.instance.GetLocationIcon(Game.instance.m_StartLocation, out a))
            {
                point = a + Vector3.up * 2f;
                ZNet.instance.SetReferencePosition(point);
                __result = ZNetScene.instance.IsAreaReady(point);
                if (__result)
                    Debug($"respawning at start: {point}");
            } else
            {
                Debug("start point not found");
                ZNet.instance.SetReferencePosition(Vector3.zero);
                point = Vector3.zero;
                __result = false;
            }

            return false;
        }

        if (useFixedSpawnCoordinates.Value)
        {
            usedLogoutPoint = false;

            point = fixedSpawnCoordinates.Value;
            ZNet.instance.SetReferencePosition(point);
            __result = ZNetScene.instance.IsAreaReady(point);
            if (__result)
                Debug($"respawning at custom point {point}");
            return false;
        }

        return true;
    }
}