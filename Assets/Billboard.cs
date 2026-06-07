using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private BillboardType billboardType;

    public enum BillboardType { LookAtCamera, CameraForward };

    // Use LateUpdate so the camera has already finished its movement for the frame
    void LateUpdate()
    {
        if (Camera.main == null) return;

        switch (billboardType)
        {
            case BillboardType.LookAtCamera:
                // 1. Get the camera's position
                Vector3 targetPosition = Camera.main.transform.position;

                // 2. FORCE the target height to match the NPC's exact height
                // This strips away any vertical tilting entirely!
                targetPosition.y = transform.position.y;

                // 3. Face that flattened position safely
                transform.LookAt(targetPosition, Vector3.up);
                break;

            case BillboardType.CameraForward:
                // Get the camera's flat forward vector
                Vector3 camForward = Camera.main.transform.forward;

                // Strip vertical viewing angles from the camera vector
                camForward.y = 0;

                if (camForward.sqrMagnitude > 0.001f)
                {
                    transform.forward = camForward.normalized;
                }
                break;

            default:
                break;
        }
    }
}