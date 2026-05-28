using UnityEngine;

public class ClueGlowProximity : MonoBehaviour
{
    [Header("Setup")]
    public Transform playerTransform;
    public float activationDistance = 4f;

    [Header("Glow Settings")]
    public Color normalColor = Color.white;
    public Color activeGlowColor = Color.yellow;

    private Renderer myRenderer;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null || myRenderer == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Get the current property block from the renderer
        myRenderer.GetPropertyBlock(propBlock);

        if (distance <= activationDistance)
        {
            float proximityFactor = 1f - (distance / activationDistance);

            // Blend colors based on proximity
            Color blendedColor = Color.Lerp(normalColor, activeGlowColor, proximityFactor);

            // In Unity 2021 URP Lit, "_BaseColor" is the standard property identifier
            propBlock.SetColor("_BaseColor", blendedColor);
        }
        else
        {
            propBlock.SetColor("_BaseColor", normalColor);
        }

        // Push the changes back to the renderer
        myRenderer.SetPropertyBlock(propBlock);
    }
}