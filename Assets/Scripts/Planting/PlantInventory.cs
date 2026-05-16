using UnityEngine;

public class PlantInventory : MonoBehaviour
{
    [SerializeField] private GameObject[] plantPrefabs;
    [SerializeField] private int selectedIndex;

    public GameObject SelectedPlantPrefab
    {
        get
        {
            if (plantPrefabs == null || plantPrefabs.Length == 0)
                return null;

            selectedIndex = Mathf.Clamp(selectedIndex, 0, plantPrefabs.Length - 1);
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

    public void SelectNextPlant()
    {
        if (plantPrefabs == null || plantPrefabs.Length == 0)
            return;

        selectedIndex = (selectedIndex + 1) % plantPrefabs.Length;
        Debug.Log("Selected plant: " + plantPrefabs[selectedIndex].name);
    }
}