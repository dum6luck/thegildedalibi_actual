using UnityEngine;

public class ChandelierSway : MonoBehaviour
{
    public float maxAngle = 4f;
    public float speed = 0.6f;

    void Update()
    {
        float t = Time.time * speed;

        // intentionally mismatched frequencies + phases
        float x =
            Mathf.Sin(t * 1.13f) +
            Mathf.Sin(t * 0.71f + 1.7f) * 0.6f +
            Mathf.Sin(t * 0.37f + 3.2f) * 0.3f;

        float z =
            Mathf.Cos(t * 0.97f + 0.4f) +
            Mathf.Sin(t * 0.53f + 2.1f) * 0.6f +
            Mathf.Cos(t * 0.29f + 1.3f) * 0.3f;

        transform.localRotation = Quaternion.Euler(
            x * maxAngle,
            0f,
            z * maxAngle
        );
    }
}