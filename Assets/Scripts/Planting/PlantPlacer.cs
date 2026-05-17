using UnityEngine;
using UnityEngine.InputSystem;

public class PlantPlacer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlantInventory inventory;
    [SerializeField] private Transform rayOrigin;

    [Header("VR Input")]
    [SerializeField] private InputActionReference placeAction;

    [Header("Placement")]
    [SerializeField] private LayerMask plantableLayer;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float minDistanceBetweenPlants = 0.7f;

    [Header("Preview")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private float previewScale = 1f;

    private GameObject currentPreview;
    private GameObject lastPreviewPrefab;
    private bool canPlace;
    private Vector3 currentPlacePosition;

    private void OnEnable()
    {
        if (placeAction != null)
            placeAction.action.performed += OnPlaceAction;
    }

    private void OnDisable()
    {
        if (placeAction != null)
            placeAction.action.performed -= OnPlaceAction;
    }

    private void Update()
    {
        UpdatePreview();

        // PC debug: always L
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
        {
            Debug.Log("L pressed");
            TryPlacePlant();
        }
    }

    private void OnPlaceAction(InputAction.CallbackContext context)
    {
        Debug.Log("VR place action pressed");
        TryPlacePlant();
    }

    public void TryPlacePlant()
    {
        if (inventory == null)
        {
            Debug.Log("Cannot place: inventory is missing.");
            return;
        }

        if (inventory.SelectedPlantPrefab == null)
        {
            Debug.Log("Cannot place: no plant selected. Open inventory and select a plant first.");
            return;
        }

        if (!canPlace)
        {
            Debug.Log("Cannot place: no valid plantable position.");
            return;
        }

        Instantiate(inventory.SelectedPlantPrefab, currentPlacePosition, Quaternion.identity);
        Debug.Log("Placed plant: " + inventory.SelectedPlantPrefab.name);
    }

    private void UpdatePreview()
    {
        if (inventory == null || rayOrigin == null || !inventory.HasSelectedPlant)
        {
            canPlace = false;

            if (currentPreview != null)
                currentPreview.SetActive(false);

            return;
        }

        GameObject selectedPrefab = inventory.SelectedPlantPrefab;

        if (selectedPrefab != lastPreviewPrefab)
            RecreatePreview(selectedPrefab);

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * maxDistance, Color.red);

if (!Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, maxDistance, plantableLayer))
{
    Debug.Log("Raycast missed Plantable");

    canPlace = false;

    if (currentPreview != null)
        currentPreview.SetActive(false);

    return;
}

Debug.Log("Raycast hit: " + hit.collider.name);

        currentPlacePosition = hit.point;
        canPlace = !IsTooCloseToAnotherPlant(hit.point);

        if (currentPreview != null)
        {
            currentPreview.SetActive(true);
            currentPreview.transform.position = currentPlacePosition;
            currentPreview.transform.rotation = Quaternion.identity;
        }
    }

    private void RecreatePreview(GameObject prefab)
    {
        if (currentPreview != null)
            Destroy(currentPreview);

        currentPreview = Instantiate(prefab);
        currentPreview.name = "Plant Preview";
        lastPreviewPrefab = prefab;

        foreach (Collider col in currentPreview.GetComponentsInChildren<Collider>())
            col.enabled = false;

        foreach (Rigidbody rb in currentPreview.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;

        PlantPreview preview = currentPreview.AddComponent<PlantPreview>();
        preview.Initialize(previewMaterial, previewScale);
    }

    private bool IsTooCloseToAnotherPlant(Vector3 position)
    {
        GrowablePlant[] plants = FindObjectsByType<GrowablePlant>(FindObjectsSortMode.None);

        foreach (GrowablePlant plant in plants)
        {
            if (Vector3.Distance(position, plant.transform.position) < minDistanceBetweenPlants)
                return true;
        }

        return false;
    }
}