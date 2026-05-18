using UnityEngine;
using UnityEngine.InputSystem;

public class WateringCan : MonoBehaviour
{
    [Header("Debug Input")]
    public bool allowKeyboardDebugWater = true;
    public Key waterKey = Key.Space;

    [Header("Plant Reference")]
    public PlantGrowth targetPlant;
    public float wateringDistance = 1.2f;

    [Header("Tilt Requirement")]
    public float requiredTiltAngle = 45f;

    private void Update()
    {
        // Temporary laptop/debug fallback only
        if (allowKeyboardDebugWater &&
            Keyboard.current != null &&
            Keyboard.current[waterKey].wasPressedThisFrame)
        {
            TryWater();
        }
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