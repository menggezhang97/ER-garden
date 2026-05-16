using UnityEngine;
using UnityEngine.InputSystem;

public class XRWateringCan : MonoBehaviour
{
    [Header("XR Input")]
    public InputActionReference waterAction;

    [Header("Plant Target")]
    public PlantGrowth targetPlant;
    public float wateringDistance = 1.2f;

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
        if (targetPlant == null)
        {
            Debug.Log("No target plant assigned.");
            return;
        }

        float distance = Vector3.Distance(transform.position, targetPlant.transform.position);
        if (distance > wateringDistance)
        {
            Debug.Log("Plant is too far away to water.");
            return;
        }

        if (!IsTiltedEnough())
        {
            Debug.Log("Tilt the watering can more before watering.");
            return;
        }

        targetPlant.WaterPlant();
        Debug.Log("Watering successful.");
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