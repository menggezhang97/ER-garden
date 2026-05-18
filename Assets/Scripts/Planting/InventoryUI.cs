using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlantInventory inventory;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private TextMeshProUGUI selectedPlantText;

    [Header("XR Input Actions")]
    [SerializeField] private InputActionReference toggleInventoryAction;
    [SerializeField] private InputActionReference selectionMoveAction;
    [SerializeField] private InputActionReference confirmSelectionAction;

    [Header("Input Timing")]
    [SerializeField] private float selectionCooldown = 0.35f;
    [SerializeField] private float stickThreshold = 0.5f;

    [Header("Panel Position")]
    [SerializeField] private float distanceFromCamera = 1.2f;
    [SerializeField] private float heightOffset = -0.15f;

    private bool isOpen;
    private int previewIndex;
    private float lastSelectionTime = -999f;

    private void OnEnable()
    {
        EnableAction(toggleInventoryAction, OnToggleInventory);
        EnableAction(selectionMoveAction, OnSelectionMove);
        EnableAction(confirmSelectionAction, OnConfirmSelection);
    }

    private void OnDisable()
    {
        DisableAction(toggleInventoryAction, OnToggleInventory);
        DisableAction(selectionMoveAction, OnSelectionMove);
        DisableAction(confirmSelectionAction, OnConfirmSelection);
    }

    private void Start()
    {
        CloseInventory();
    }

    private void Update()
    {
        HandleKeyboardInput();

        if (isOpen)
            KeepPanelInFrontOfPlayer();
    }

    private void EnableAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionRef == null || actionRef.action == null)
            return;

        actionRef.action.performed += callback;
        actionRef.action.Enable();
    }

    private void DisableAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionRef == null || actionRef.action == null)
            return;

        actionRef.action.performed -= callback;
        actionRef.action.Disable();
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        ToggleInventory();
    }

    private void OnSelectionMove(InputAction.CallbackContext context)
    {
        if (!isOpen)
            return;

        if (Time.time - lastSelectionTime < selectionCooldown)
            return;

        Vector2 moveValue = context.ReadValue<Vector2>();

        if (moveValue.x > stickThreshold)
        {
            lastSelectionTime = Time.time;
            MoveSelection(1);
        }
        else if (moveValue.x < -stickThreshold)
        {
            lastSelectionTime = Time.time;
            MoveSelection(-1);
        }
    }

    private void OnConfirmSelection(InputAction.CallbackContext context)
    {
        if (!isOpen)
            return;

        ConfirmSelection();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
            ToggleInventory();

        if (!isOpen)
            return;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            MoveSelection(1);

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            MoveSelection(-1);

        if (Keyboard.current.enterKey.wasPressedThisFrame)
            ConfirmSelection();
    }

    private void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    private void OpenInventory()
    {
        if (inventory == null || inventoryPanel == null)
        {
            Debug.LogWarning("InventoryUI is missing references.");
            return;
        }

        isOpen = true;
        previewIndex = inventory.SelectedIndex >= 0 ? inventory.SelectedIndex : 0;

        inventory.SelectPlant(previewIndex);

        inventoryPanel.SetActive(true);
        UpdateText();
        KeepPanelInFrontOfPlayer();
    }

    private void CloseInventory()
    {
        isOpen = false;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    private void MoveSelection(int direction)
    {
        GameObject[] prefabs = inventory.PlantPrefabs;

        if (prefabs == null || prefabs.Length == 0)
            return;

        previewIndex += direction;

        if (previewIndex < 0)
            previewIndex = prefabs.Length - 1;

        if (previewIndex >= prefabs.Length)
            previewIndex = 0;

        inventory.SelectPlant(previewIndex);
        UpdateText();
    }

    private void ConfirmSelection()
    {
        inventory.SelectPlant(previewIndex);
        CloseInventory();
    }

    private void UpdateText()
    {
        if (selectedPlantText == null)
            return;

        GameObject[] prefabs = inventory.PlantPrefabs;

        if (prefabs == null || prefabs.Length == 0)
        {
            selectedPlantText.text = "No plants";
            return;
        }

        selectedPlantText.text =
            "Inventory\n\n" +
            "Selected:\n" +
            prefabs[previewIndex].name +
            "\n\nStick Left/Right: switch\nTrigger/A: select";
    }

    private void KeepPanelInFrontOfPlayer()
    {
        if (playerCamera == null || inventoryPanel == null)
            return;

        Vector3 targetPosition =
            playerCamera.position +
            playerCamera.forward * distanceFromCamera +
            Vector3.up * heightOffset;

        inventoryPanel.transform.position = targetPosition;
        inventoryPanel.transform.rotation =
            Quaternion.LookRotation(inventoryPanel.transform.position - playerCamera.position);
    }
}