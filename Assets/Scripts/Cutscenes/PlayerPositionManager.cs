using UnityEngine;

public class PlayerPositionManager : MonoBehaviour
{
    // Static variables persist across scene loads
    public static Vector3 SavedPlayerPosition = Vector3.zero;
    public static Quaternion SavedPlayerRotation = Quaternion.identity;
    public static bool HasSavedPosition = false;

    public static void SavePosition(Transform playerTransform)
    {
        if (playerTransform != null)
        {
            SavedPlayerPosition = playerTransform.position;
            SavedPlayerRotation = playerTransform.rotation;
            HasSavedPosition = true;
        }
    }

    public static void ClearPosition()
    {
        HasSavedPosition = false;
    }
}