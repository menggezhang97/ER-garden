using UnityEngine;
using UnityEngine.InputSystem;

public class XRWateringCan : MonoBehaviour
{
    [Header("XR Input")]
    public InputActionReference waterAction;

    [Header("Watering Detection")]
    public Transform waterOrigin;
    public float wateringDistance = 1.2f;
    public LayerMask plantLayer = ~0;

    [Header("Tilt Requirement")]
    public float requiredTiltAngle = 45f;

    private void OnEnable()
    {
        if (waterAction != null && waterAction.action != null)
        {
            waterAction.action.Enable();
            waterAction.action.performed += OnWaterPerformed;
        }
    }

    private void OnDisable()
    {
        if (waterAction != null && waterAction.action != null)
        {
            waterAction.action.performed -= OnWaterPerformed;
            waterAction.action.Disable();
        }
    }

    private void OnWaterPerformed(InputAction.CallbackContext context)
    {
        TryWater();
    }

    public void TryWater()
    {
        if (!IsTiltedEnough())
        {
            Debug.Log("Tilt the watering can more before watering.");
            return;
        }

        Transform origin = waterOrigin != null ? waterOrigin : transform;

        Collider[] hits = Physics.OverlapSphere(origin.position, wateringDistance, plantLayer);

        foreach (Collider hit in hits)
        {
            PlantGrowth plantGrowth = hit.GetComponentInParent<PlantGrowth>();

            if (plantGrowth != null)
            {
                plantGrowth.WaterPlant();
                Debug.Log("Watering successful: " + plantGrowth.name);
                return;
            }

            GrowablePlant growablePlant = hit.GetComponentInParent<GrowablePlant>();

            if (growablePlant != null)
            {
                growablePlant.Water(Time.deltaTime);
                Debug.Log("Watering successful: " + growablePlant.name);
                return;
            }
        }

        Debug.Log("No plant nearby to water.");
    }

    private bool IsTiltedEnough()
    {
        float zAngle = NormalizeAngle(transform.eulerAngles.z);
        float tiltAmount = Mathf.Abs(zAngle);

        Debug.Log("Current tilt: " + tiltAmount);

        return tiltAmount >= requiredTiltAngle;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}