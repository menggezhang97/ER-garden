using UnityEngine;

public class GrowablePlant : MonoBehaviour
{
    [Header("Growth Settings")]
    [SerializeField] private float plantedScale = 0.2f;
    [SerializeField] private float maxScale = 1.0f;
    [SerializeField] private float growthPerSecond = 0.15f;

    private float currentScale;

    private void Start()
    {
        currentScale = plantedScale;
        ApplyScale();
    }

    public void Water(float deltaTime)
    {
        if (currentScale >= maxScale)
            return;

        currentScale += growthPerSecond * deltaTime;
        currentScale = Mathf.Min(currentScale, maxScale);
        ApplyScale();
    }

    private void ApplyScale()
    {
        transform.localScale = Vector3.one * currentScale;
    }
}