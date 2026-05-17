using UnityEngine;

public class PlantInventory : MonoBehaviour
{
    [SerializeField] private GameObject[] plantPrefabs;
    [SerializeField] private int selectedIndex = -1;

    public GameObject[] PlantPrefabs => plantPrefabs;
    public int SelectedIndex => selectedIndex;
    public bool HasSelectedPlant => selectedIndex >= 0;

    private void Awake()
    {
        selectedIndex = -1;
    }

    public GameObject SelectedPlantPrefab
    {
        get
        {
            if (!HasSelectedPlant || plantPrefabs == null || plantPrefabs.Length == 0)
                return null;

            return plantPrefabs[selectedIndex];
        }
    }

    public void SelectPlant(int index)
    {
        if (plantPrefabs == null || plantPrefabs.Length == 0)
            return;

        selectedIndex = Mathf.Clamp(index, 0, plantPrefabs.Length - 1);
        Debug.Log("Selected plant: " + plantPrefabs[selectedIndex].name);
    }
}