using UnityEngine;

/// <summary>
/// Detects whether a plant is in shadow by raycasting toward the sun (or straight up).
/// When in shadow, the plant's materials gradually fade to a pale/desaturated colour.
/// When back in sunlight the colour is restored.
///
/// How to use
/// ----------
/// 1. Attach this script to each GrowablePlant prefab (the root GameObject that also has GrowablePlant.cs).
/// 2. Assign the Sun Transform (your Directional Light) in the Inspector, OR leave it null to
///    cast straight up instead.
/// 3. Tune the tuneable fields in the Inspector to match your scene.
/// </summary>
public class PlantSunlight : MonoBehaviour
{
    // -----------------------------------------------------------------------
    //  Inspector fields
    // -----------------------------------------------------------------------
    [Header("Sun Reference")]
    [Tooltip("Drag your scene's Directional Light Transform here. " +
             "Leave empty to cast straight upward instead.")]
    public Transform sunTransform;

    [Header("Raycast Settings")]
    [Tooltip("Layer mask for objects that block sunlight (terrain, roofs, other plants…). " +
             "Make sure the plant itself is NOT on this layer.")]
    public LayerMask shadowCasterLayers = Physics.DefaultRaycastLayers;

    [Tooltip("How high above the plant the ray starts, to avoid self-intersection.")]
    public float rayOriginOffset = 0.1f;

    [Tooltip("Maximum ray length. Should be long enough to reach any overhead obstacle.")]
    public float rayLength = 30f;

    [Header("Sun Exposure")]
    [Tooltip("How fast (0-1 per second) sun exposure fills up when in direct light.")]
    [Range(0.01f, 2f)]
    public float exposureRecoveryRate = 0.2f;

    [Tooltip("How fast (0-1 per second) sun exposure drains when in shadow.")]
    [Range(0.01f, 2f)]
    public float exposureDepletionRate = 0.1f;

    [Tooltip("Exposure below this value is considered 'in shadow'.")]
    [Range(0f, 1f)]
    public float shadowThreshold = 0.3f;

    [Header("Fade Settings")]
    [Tooltip("The healthy, well-lit colour tint applied to every renderer on this plant.")]
    public Color healthyColor = Color.white;

    [Tooltip("The pale/faded colour tint applied when the plant is fully in shadow.")]
    public Color fadedColor = Color.white; // no colour tint by default — wilt animation is the main effect

    [Tooltip("How fast the visual colour lerps toward the target (higher = snappier).")]
    [Range(0.5f, 10f)]
    public float colorLerpSpeed = 2f;

    [Header("Wilt Animation")]
    [Tooltip("How small the plant shrinks when fully in shadow. 1 = no shrink, 0.5 = half size.")]
    [Range(0.3f, 1f)]
    public float wiltedScaleMultiplier = 0.75f;

    [Tooltip("Degrees the plant tilts on its local X axis when fully in shadow (drooping effect).")]
    [Range(0f, 45f)]
    public float wiltTiltDegrees = 20f;

    [Tooltip("How fast the wilt lerps (higher = snappier).")]
    [Range(0.1f, 10f)]
    public float wiltLerpSpeed = 1.5f;

    [Header("Growth Penalty (optional)")]
    [Tooltip("When enabled, GrowablePlant growth is slowed/stopped in shadow.")]
    public bool penaliseGrowthInShadow = true;

    [Tooltip("Growth rate multiplier applied when the plant is in shadow (0 = no growth).")]
    [Range(0f, 1f)]
    public float shadowGrowthMultiplier = 0f;

    // -----------------------------------------------------------------------
    //  Public read-only state (inspectable at runtime)
    // -----------------------------------------------------------------------

    [Header("Runtime State (read-only)")]
    [Range(0f, 1f)]
    [SerializeField] private float sunExposure = 1f;   // starts healthy
    [SerializeField] private bool isInShadow = false;

    // -----------------------------------------------------------------------
    //  Private fields
    // -----------------------------------------------------------------------

    private Renderer[] renderers;
    private GrowablePlant growablePlant;   // optional sibling component

    // Cache original growth rate so we can restore it
    private float originalGrowthRate = -1f;

    // Material property block avoids creating extra material instances
    private MaterialPropertyBlock propBlock;

    // Timer to periodically refresh the renderer cache
    // (PlantGrowSpot starts empty; children are added when the player plants a seed)
    private float _rendererRefreshTimer = 0f;
    private const float RendererRefreshInterval = 1f; // seconds

    // Wilt animation — cached so we can restore after sun returns
    private Vector3    _originalLocalScale;
    private Quaternion _originalLocalRotation;

    // -----------------------------------------------------------------------
    //  Unity lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        // Gather all Renderer components on this object and its children
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        // GrowablePlant is optional — only used when penaliseGrowthInShadow is true
        growablePlant = GetComponent<GrowablePlant>();

        propBlock = new MaterialPropertyBlock();

        // Auto-discover the sun if not assigned in the Inspector
        if (sunTransform == null)
        {
            // First try: the light marked as the scene's environment sun
            Light envSun = RenderSettings.sun;
            if (envSun != null && envSun.type == LightType.Directional)
            {
                sunTransform = envSun.transform;
            }
            else
            {
                // Fallback: first directional light in the scene
                Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
                foreach (Light l in lights)
                {
                    if (l.type == LightType.Directional)
                    {
                        sunTransform = l.transform;
                        break;
                    }
                }
            }

            if (sunTransform == null)
                Debug.LogWarning("[PlantSunlight] No Directional Light found in scene. " +
                                 "Raycasting straight up instead. Add a Directional Light to your scene.", this);
        }
    }

    private void Start()
    {
        // Cache the original growth rate from GrowablePlant via reflection so we
        // can restore it without hardcoding the field name in a fragile way.
        // (GrowablePlant exposes growthPerSecond as a private SerializedField —
        //  we read it once via reflection just to store it.)
        if (penaliseGrowthInShadow && growablePlant != null)
        {
            var field = typeof(GrowablePlant).GetField(
                "growthPerSecond",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
                originalGrowthRate = (float)field.GetValue(growablePlant);
            else
                Debug.LogWarning("[PlantSunlight] Could not find 'growthPerSecond' field " +
                                 "on GrowablePlant. Growth penalty will not work.");
        }

        // Cache original transform state for wilt animation
        _originalLocalScale    = transform.localScale;
        _originalLocalRotation = transform.localRotation;

        // Initialise colour to healthy
        ApplyColorToRenderers(healthyColor);
    }

    private void Update()
    {
        // Re-scan for renderers every second while none are cached.
        // This handles PlantGrowSpot which starts empty and receives children
        // only after the player places a seed.
        _rendererRefreshTimer -= Time.deltaTime;
        if (_rendererRefreshTimer <= 0f)
        {
            _rendererRefreshTimer = RendererRefreshInterval;
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_overrideActive) return; // debug tester holds the state
#endif
        UpdateSunExposure();
        UpdateVisuals();
        UpdateGrowthPenalty();
    }

    // -----------------------------------------------------------------------
    //  Shadow detection
    // -----------------------------------------------------------------------

    private void UpdateSunExposure()
    {
        bool blocked = IsBlockedByShadow();

        if (blocked)
        {
            sunExposure -= exposureDepletionRate * Time.deltaTime;
        }
        else
        {
            sunExposure += exposureRecoveryRate * Time.deltaTime;
        }

        sunExposure = Mathf.Clamp01(sunExposure);
        isInShadow  = sunExposure < shadowThreshold;
    }

    /// <summary>Returns true if the upward ray toward the sun is blocked.</summary>
    private bool IsBlockedByShadow()
    {
        Vector3 origin    = transform.position + Vector3.up * rayOriginOffset;
        Vector3 direction = GetSunDirection();

        // Draw a debug ray visible in Scene view during play mode
        Debug.DrawRay(origin, direction * rayLength,
                      isInShadow ? Color.blue : Color.yellow, 0f, false);

        return Physics.Raycast(origin, direction, rayLength, shadowCasterLayers,
                               QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// Returns the direction from the plant toward the sun.
    /// Falls back to straight up when no sun transform is assigned.
    /// </summary>
    private Vector3 GetSunDirection()
    {
        if (sunTransform != null)
        {
            // Directional lights point along their forward axis; the sun direction
            // as seen by objects on the ground is the *opposite* (light comes from above).
            return -sunTransform.forward;
        }
        return Vector3.up;
    }

    // -----------------------------------------------------------------------
    //  Visual fading
    // -----------------------------------------------------------------------

    private void UpdateVisuals()
    {
        // Target colour is a lerp between faded (0 exposure) and healthy (1 exposure)
        Color targetColor = Color.Lerp(fadedColor, healthyColor, sunExposure);

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            rend.GetPropertyBlock(propBlock);
            Color current = propBlock.GetColor("_BaseColor");

            // If the property hasn't been set yet the getter returns black (0,0,0,0).
            // Initialise it from the material's own colour on first frame.
            if (current == Color.clear)
                current = GetMaterialBaseColor(rend);

            Color next = Color.Lerp(current, targetColor, colorLerpSpeed * Time.deltaTime);
            propBlock.SetColor("_BaseColor", next);

            // Also write _Color for Standard / Built-in RP shaders
            propBlock.SetColor("_Color", next);

            rend.SetPropertyBlock(propBlock);
        }

        // ── Wilt animation ──────────────────────────────────────────────────
        // sunExposure == 0 → full wilt;  sunExposure == 1 → fully upright
        float t = sunExposure; // 0..1

        // Scale: shrink toward wiltedScaleMultiplier as exposure drops
        //Vector3 targetScale = _originalLocalScale * Mathf.Lerp(wiltedScaleMultiplier, 1f, t);
        //transform.localScale = Vector3.Lerp(
            //transform.localScale, targetScale, wiltLerpSpeed * Time.deltaTime);

        // Tilt: droop (rotate on local X) as exposure drops
        float      targetTiltDeg = Mathf.Lerp(wiltTiltDegrees, 0f, t);
        Quaternion targetRot     = _originalLocalRotation * Quaternion.Euler(targetTiltDeg, 0f, 0f);
        transform.localRotation  = Quaternion.Slerp(
            transform.localRotation, targetRot, wiltLerpSpeed * Time.deltaTime);
    }

    /// <summary>Reads _BaseColor (URP/HDRP) or _Color (Built-in) from the first material.</summary>
    private Color GetMaterialBaseColor(Renderer rend)
    {
        if (rend.sharedMaterial == null) return healthyColor;

        if (rend.sharedMaterial.HasProperty("_BaseColor"))
            return rend.sharedMaterial.GetColor("_BaseColor");

        if (rend.sharedMaterial.HasProperty("_Color"))
            return rend.sharedMaterial.GetColor("_Color");

        return healthyColor;
    }

    private void ApplyColorToRenderers(Color color)
    {
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning($"[PlantSunlight] '{name}' has NO renderers — colour cannot change. " +
                             "Check that the plant has MeshRenderer/SkinnedMeshRenderer components.", this);
            return;
        }

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            // Method 1 – MaterialPropertyBlock (no extra instance, preferred)
            rend.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", color);
            propBlock.SetColor("_Color",     color);
            rend.SetPropertyBlock(propBlock);

            // Method 2 – Direct material colour (works with ANY shader,
            // creates a per-renderer instance on first call but always visible)
            if (rend.material != null)
            {
                if (rend.material.HasProperty("_BaseColor"))
                    rend.material.SetColor("_BaseColor", color);
                else if (rend.material.HasProperty("_Color"))
                    rend.material.SetColor("_Color", color);

                rend.material.color = color; // also sets the main colour slot
            }
        }
    }

    // -----------------------------------------------------------------------
    //  Growth penalty
    // -----------------------------------------------------------------------

    private void UpdateGrowthPenalty()
    {
        if (!penaliseGrowthInShadow || growablePlant == null || originalGrowthRate < 0f)
            return;

        var field = typeof(GrowablePlant).GetField(
            "growthPerSecond",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field == null) return;

        float targetRate = isInShadow
            ? originalGrowthRate * shadowGrowthMultiplier
            : originalGrowthRate;

        field.SetValue(growablePlant, targetRate);
    }

    // -----------------------------------------------------------------------
    //  Public helpers (can be called by other scripts / UI)
    // -----------------------------------------------------------------------

    /// <summary>Current sun exposure (0 = fully in shadow, 1 = full sunlight).</summary>
    public float SunExposure => sunExposure;

    /// <summary>
    /// Compatibility property for PlantGrowth.cs.
    /// PlantGrowth expects currentExposure, so we return the real sunExposure value.
    /// </summary>
    public float currentExposure => sunExposure;

    /// <summary>True when the plant is considered to be in shadow.</summary>
    public bool IsInShadow => isInShadow;

    /// <summary>
    /// Force-set exposure to a specific value (e.g. call from a time-of-day system).
    /// </summary>
    public void SetSunExposure(float value) => sunExposure = Mathf.Clamp01(value);

    // -----------------------------------------------------------------------
    //  Debug / test helpers  (used by PlantSunlightTester)
    // -----------------------------------------------------------------------

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool _overrideActive = false;

    /// <summary>Force exposure to <paramref name="value"/> and stop raycasting.</summary>
    public void DEBUG_ForceExposure(float value)
    {
        // Always refresh renderer cache before forcing — PlantGrowSpot may have
        // received children since Awake was called.
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        _overrideActive = true;
        sunExposure     = Mathf.Clamp01(value);
        isInShadow      = value < shadowThreshold;

        // Diagnostic — tells you exactly what is being targeted
        int count = renderers == null ? 0 : renderers.Length;
        string matNames = count == 0 ? "NONE" :
            string.Join(", ", System.Array.ConvertAll(renderers,
                r => r == null ? "null" : $"{r.name}({(r.sharedMaterial ? r.sharedMaterial.name : "no mat")})"));
        Debug.Log($"[PlantSunlight DEBUG] '{name}' forcing exposure={value:F2}. " +
                  $"Renderers found: {count} → {matNames}", this);

        // Apply immediately — skip lerp so testers see the result at once
        Color target = Color.Lerp(fadedColor, healthyColor, sunExposure);
        ApplyColorToRenderers(target);

        // Apply wilt immediately (no lerp) so the tester result is instant
        // Safety: if Start() hasn't cached the scale yet, read it now
        if (_originalLocalScale == Vector3.zero)
            _originalLocalScale = transform.localScale;
        transform.localScale    = _originalLocalScale * Mathf.Lerp(wiltedScaleMultiplier, 1f, sunExposure);
        float tilt              = Mathf.Lerp(wiltTiltDegrees, 0f, sunExposure);
        transform.localRotation = _originalLocalRotation * Quaternion.Euler(tilt, 0f, 0f);
    }

    /// <summary>Release the override — normal sun detection resumes on next Update.</summary>
    public void DEBUG_ReleaseOverride() => _overrideActive = false;

    /// <summary>Returns current sun exposure (0–1).</summary>
    public float DEBUG_GetExposure() => sunExposure;

    /// <summary>Returns current shadow state.</summary>
    public bool DEBUG_IsInShadow() => isInShadow;
#else
    // Stubs so PlantSunlightTester compiles in release too (it's editor-only anyway)
    public void DEBUG_ForceExposure(float v) { }
    public void DEBUG_ReleaseOverride() { }
    public float DEBUG_GetExposure()    => sunExposure;
    public bool  DEBUG_IsInShadow()     => isInShadow;
#endif
}
