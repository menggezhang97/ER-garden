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

    [Header("Panel Position")]
    [SerializeField] private float distanceFromCamera = 1.2f;
    [SerializeField] private float heightOffset = -0.15f;

    private bool isOpen;
    private int previewIndex;

    private void Start()
    {
        CloseInventory();
    }

    private void Update()
    {
        HandleKeyboardInput();
        HandleQuestInput();

        if (isOpen)
            KeepPanelInFrontOfPlayer();
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

    private void HandleQuestInput()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null)
            return;

        if (gamepad.startButton.wasPressedThisFrame || gamepad.selectButton.wasPressedThisFrame)
            ToggleInventory();

        if (!isOpen)
            return;

        if (gamepad.dpad.right.wasPressedThisFrame || gamepad.rightStick.right.wasPressedThisFrame)
            MoveSelection(1);

        if (gamepad.dpad.left.wasPressedThisFrame || gamepad.rightStick.left.wasPressedThisFrame)
            MoveSelection(-1);

        if (gamepad.buttonSouth.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame)
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
        Vector3 targetPosition =
            playerCamera.position +
            playerCamera.forward * distanceFromCamera +
            Vector3.up * heightOffset;

        inventoryPanel.transform.position = targetPosition;
        inventoryPanel.transform.rotation =
            Quaternion.LookRotation(inventoryPanel.transform.position - playerCamera.position);
    }
}