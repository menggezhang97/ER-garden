using UnityEngine;
using UnityEngine.InputSystem;


public class WateringCanNew : MonoBehaviour
{
    [Header("XR Input")]
    public InputActionReference waterAction;

    [Header("Watering")]
    public float range = 10f;
    public float waterAmountPerSecond = 1f;

    [Header("Raycast")]
    public Transform rayOrigin;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void Update()
    {
        if (!grabInteractable.isSelected)
            return;

        if (waterAction == null || waterAction.action == null)
            return;

        float triggerValue = waterAction.action.ReadValue<float>();

        if (triggerValue > 0.1f)
        {
            WaterPlant();
        }
    }

    private void WaterPlant()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            GrowablePlant plant = hit.collider.GetComponentInParent<GrowablePlant>();

            if (plant != null)
            {
                plant.Water(Time.deltaTime * waterAmountPerSecond);

                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.blue);
            }
        }
    }
}