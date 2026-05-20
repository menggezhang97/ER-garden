using UnityEngine;

/// <summary>
/// Controls the scene's directional light to simulate a full day/night cycle.
/// The rotation of the sun will cast moving shadows across the garden.
/// Plants with PlantSunlight components will naturally react as shadows pass over them!
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Sun Reference")]
    [Tooltip("The Directional Light representing the sun. If left empty, it will auto-detect one.")]
    public Light sunLight;

    [Header("Time Settings")]
    [Tooltip("How many real-time seconds a full 24-hour game day takes.")]
    public float dayDurationSeconds = 30f; // Fast cycle for testing

    [Tooltip("Current time of day: 0 = Midnight, 0.25 = Sunrise, 0.5 = Noon, 0.75 = Sunset")]
    [Range(0f, 1f)]
    public float timeOfDay = 0.35f; // Start in the morning

    public float CurrentTimeOfDay => timeOfDay;

    [Tooltip("Should time automatically progress?")]
    public bool autoAdvanceTime = true;

    [Header("Visual Settings")]
    [Tooltip("Maximum intensity of the sun at high noon.")]
    public float maxSunIntensity = 1f;

    [Tooltip("Intensity of the moonlight at night (prevents pitch black).")]
    public float nightIntensity = 0.25f;

    [Tooltip("Color of the sun at high noon.")]
    public Color noonColor = new Color(1f, 0.95f, 0.9f);

    [Tooltip("Color of the sun at sunrise/sunset.")]
    public Color sunriseColor = new Color(1f, 0.5f, 0.2f); // Warm orange

    [Tooltip("Color of the moon at night.")]
    public Color moonColor = new Color(0.3f, 0.4f, 0.7f); // Cool blue

    [Tooltip("Color of the ambient light (night/shadows).")]
    public Color nightAmbientColor = new Color(0.25f, 0.3f, 0.4f); // Brighter ambient night
    
    [Tooltip("Color of the ambient light during the day.")]
    public Color dayAmbientColor = new Color(0.5f, 0.5f, 0.5f);

    private void Start()
    {
        // Auto-detect the sun if not assigned
        if (sunLight == null)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    sunLight = l;
                    break;
                }
            }

            if (sunLight == null)
            {
                Debug.LogWarning("[DayNightCycle] No Directional Light found in the scene! Please add one.");
            }
        }
    }

    private void Update()
    {
        if (sunLight == null) return;

        if (autoAdvanceTime)
        {
            // Advance time
            timeOfDay += Time.deltaTime / dayDurationSeconds;

            // Loop back to midnight
            if (timeOfDay >= 1f) 
                timeOfDay -= 1f;
        }

        UpdateSun();
    }

    private void UpdateSun()
    {
        // 1. Calculate sun rotation based on time of day.
        // timeOfDay: 0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset
        // We rotate on the X axis to create the arc over the sky.
        float sunAngle = (timeOfDay * 360f) - 90f;

        // Keep the sun slightly tilted on the Y axis (e.g. 30 degrees) so shadows 
        // fall diagonally across the garden instead of perfectly straight down.
        sunLight.transform.localRotation = Quaternion.Euler(sunAngle, 30f, 0f);

        // 2. Calculate sun intensity based on whether the sun is above the horizon.
        // The sun is roughly above the horizon from time 0.23 to 0.77.
        float intensityMultiplier = 0f;

        if (timeOfDay > 0.23f && timeOfDay < 0.77f)
        {
            // Map the daytime window (0.25 to 0.75) to a 0 to 1 range
            float mapped = (timeOfDay - 0.25f) / 0.5f;
            
            // Use a sine wave curve so the sun gets bright quickly and stays bright
            intensityMultiplier = Mathf.Clamp01(Mathf.Sin(mapped * Mathf.PI));
        }

        // Apply intensity (never goes to 0, drops to nightIntensity instead)
        sunLight.intensity = Mathf.Lerp(nightIntensity, maxSunIntensity, intensityMultiplier);

        // 3. Change color based on time
        if (intensityMultiplier > 0.05f)
        {
            // When intensity is low (near horizon), it's sunset/sunrise.
            // When intensity is 1 (high noon), it's noon color.
            sunLight.color = Color.Lerp(sunriseColor, noonColor, intensityMultiplier);
        }
        else
        {
            // Crossfade between moon color and sunrise color right at the horizon
            float twilight = Mathf.Clamp01(intensityMultiplier * 20f);
            sunLight.color = Color.Lerp(moonColor, sunriseColor, twilight);
        }

        // 4. Adjust the overall ambient light in the scene (so shadows aren't pitch black)
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, intensityMultiplier);
    }

#if UNITY_EDITOR
    // This magically auto-spawns the script when you hit Play, so you don't have to manually attach it!
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawnForTesting()
    {
        if (FindObjectsByType<DayNightCycle>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
        {
            GameObject go = new GameObject("DayNightCycle (Auto-Spawned)");
            go.AddComponent<DayNightCycle>();
        }
    }
#endif
}
