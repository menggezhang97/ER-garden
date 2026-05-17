using UnityEngine;

public class PlantPreview : MonoBehaviour
{
    private Renderer[] renderers;
    private Color baseColor;
    private float blinkSpeed = 3f;

    public void Initialize(Material material, float previewScale)
    {
        transform.localScale = Vector3.one * previewScale;

        renderers = GetComponentsInChildren<Renderer>();

        if (material == null)
            return;

        baseColor = material.color;

        foreach (Renderer renderer in renderers)
        {
            renderer.material = new Material(material);
        }
    }

    private void Update()
    {
        if (renderers == null)
            return;

        float alpha = Mathf.Lerp(
            0.2f,
            0.65f,
            Mathf.PingPong(Time.time * blinkSpeed, 1f)
        );

        foreach (Renderer renderer in renderers)
        {
            if (renderer.material == null)
                continue;

            Color color = baseColor;
            color.a = alpha;
            renderer.material.color = color;
        }
    }
}