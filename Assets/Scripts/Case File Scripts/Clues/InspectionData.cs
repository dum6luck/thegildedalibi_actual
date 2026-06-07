using UnityEngine;
using System.Collections.Generic;

public static class InspectionData
{
    public static string PrefabToLoad;
    public static Vector3 LastPlayerPosition;
    public static Quaternion LastPlayerRotation;
    public static bool ReturningFromInspection;

    // FIX: Stores the exact name of the asset resource loaded
    public static string LastInspectedPrefabName = "";

    public static List<Clue_Data> SavedClues = new List<Clue_Data>();
}