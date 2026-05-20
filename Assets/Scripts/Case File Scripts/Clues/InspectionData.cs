using UnityEngine;

public static class InspectionData
{
    public static string PrefabToLoad;

    // Add these to remember where the player was standing
    public static Vector3 LastPlayerPosition;
    public static Quaternion LastPlayerRotation;
    public static bool ReturningFromInspection = false;
}