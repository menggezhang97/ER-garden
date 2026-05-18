using UnityEngine;

public class GrowablePlant : MonoBehaviour
{
    [Header("Growth Settings")]
    [SerializeField] private float plantedScale = 0.2f;
    [SerializeField] private float maxScale = 1.0f;
    
    [Tooltip("How much the plant scales up per second when growing.")]
    [SerializeField] private float growthPerSecond = 0.15f;

    [Header("Water & Sun Mechanics")]
    [Tooltip("How much water the plant currently holds.")]
    public float storedWater = 0f;
    
    [Tooltip("How much water is consumed per second of growing.")]
    public float waterConsumptionRate = 1f;

    private float currentScale;
    private PlantSunlight plantSunlight;

    private void Start()
    {
        currentScale = plantedScale;
        plantSunlight = GetComponent<PlantSunlight>();
        ApplyScale();
    }

    private void Update()
    {
        if (currentScale >= maxScale) return;

        // 1. Must have stored water
        if (storedWater <= 0f) return;

        // 2. Must be daytime (and not in the shade if it has PlantSunlight)
        bool hasSunlight = false;

        if (plantSunlight != null)
        {
            // Use actual shadow detection!
            hasSunlight = plantSunlight.currentExposure > 0.5f;
        }
        else
        {
            // Fallback: check global DayNightCycle
            var dayNight = Object.FindFirstObjectByType<DayNightCycle>();
            if (dayNight != null)
                hasSunlight = (dayNight.timeOfDay > 0.25f && dayNight.timeOfDay < 0.75f);
            else
                hasSunlight = true;
        }

        // If we have water AND sunlight, the plant automatically grows over time!
        if (hasSunlight)
        {
            // Consume water slowly
            storedWater -= waterConsumptionRate * Time.deltaTime;
            storedWater = Mathf.Max(0f, storedWater);

            // Grow the plant
            currentScale += growthPerSecond * Time.deltaTime;
            currentScale = Mathf.Min(currentScale, maxScale);
            ApplyScale();
        }
    }

    // Called by the watering can
    public void Water(float amount)
    {
        // Multiply amount so a little watering goes a long way
        storedWater += amount * 10f; 
    }

    private void ApplyScale()
    {
        transform.localScale = Vector3.one * currentScale;
    }
}