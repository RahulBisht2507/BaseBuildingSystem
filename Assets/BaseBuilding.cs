using System;
using Unity.VisualScripting;
using UnityEngine;

public class BaseBuildingSystem : MonoBehaviour
{
    public GameObject[] buildingPrefabs; // Array of building prefabs
    public Transform[] buildingPreview; // Preview object for showing where the building will be placed
    public LayerMask groundLayer; // Layer mask for the ground plane

    private GameObject currentBuilding; // Reference to the currently selected building
    private Vector3 lastValidPosition; // Last valid position for the building
    private Quaternion lastValidRotation; // Last valid rotation for the building
    public bool isPlacing = false; // Flag to indicate if a building is currently being placed
    private Transform modelParent = null;
    // Resource variables
    public int wood = 100;
    public int stone = 50;
    private int prefabIndex = 0;
    private int previewIndex = 0;
    private MeshRenderer renderer1;
    private void Awake()
    {
        BuildingPreview();
    }
    void Update()
    {
       
        if (CanPlaceBuilding())
        {
            if (renderer1 != null)
            {
                renderer1.material.color = new Color(0f, 1f, 0f, .5f); // Green color with 50% transparency
            }
        }
        else
        {
            if (renderer1 != null)
            {
                renderer1.material.color = new Color(1f, 0f, 0f, 0.5f); // Red color with 50% transparency
            }
        }
        if (isPlacing)
        {
            // Raycast to the ground plane to determine placement position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                // Update the position of the building preview to the hit point
                buildingPreview[previewIndex].position = hit.point;

                // Check for user input to confirm or cancel placement
                if (Input.GetMouseButtonDown(0))
                {
                    if (CanPlaceBuilding())
                    {
                        // Place the building at the last valid position and rotation
                        PlaceBuilding(lastValidPosition, lastValidRotation);
                    }
                    else
                    {
                        // Display an error message or handle invalid placement
                        Debug.Log("Cannot place building here.");
                    }
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    // Rotate the building preview
                    buildingPreview[previewIndex].Rotate(Vector3.up, 90f);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    // Cancel building placement
                    isPlacing = false;
                    buildingPreview[previewIndex].gameObject.SetActive(false);
                }else if(Input.GetKeyDown(KeyCode.Alpha2))
                {
                    prefabIndex = 1;
                    previewIndex = 1;
                    BuildingPreview();
                }else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    prefabIndex = 0;
                    previewIndex = 0;
                    BuildingPreview();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            SelectBuilding(prefabIndex);
        }
    }

    void BuildingPreview()
    {
        currentBuilding = buildingPrefabs[prefabIndex];
        modelParent = buildingPrefabs[prefabIndex].transform.GetChild(0);
        renderer1 = null;
        renderer1 = modelParent.GetComponent<MeshRenderer>();
    }
    public void SelectBuilding(int index)
    {
        if (buildingPrefabs.Length > index && buildingPrefabs[index] != null)
        {
           
            // Instantiate the building preview
            currentBuilding = buildingPrefabs[index];
            buildingPreview[previewIndex].gameObject.SetActive(true);
            modelParent = buildingPrefabs[index].transform.GetChild(0);
            /*buildingPreview.GetChild(0).GetComponent<MeshFilter>().mesh = currentBuilding.GetComponent<MeshFilter>().sharedMesh;*/
           /* buildingPreview.GetComponent<MeshRenderer>().material = currentBuilding.GetComponent<MeshRenderer>().sharedMaterial;*/
            modelParent.GetComponent<Collider>().enabled = false;
            for (int i = 0; i < buildingPreview.Length; i++)
            {
                buildingPreview[i].gameObject.SetActive(false);
                buildingPreview[previewIndex].gameObject.SetActive(true);
            }
            /*  MeshRenderer renderer = buildingPreview.GetComponent<MeshRenderer>();*/
            isPlacing = true;
        }
    }

    bool CanPlaceBuilding()
    {
        // Check if there's enough resources
       /* BuildingCost buildingCost = currentBuilding.GetComponent<BuildingCost>();*/
       /* if (buildingCost != null)
        {
            if (buildingCost.woodCost > wood || buildingCost.stoneCost > stone)
            {
                Debug.Log("Not enough resources to build.");
                return false;
            }
        }*/

        // Check for collisions with other buildings
        Collider[] colliders = Physics.OverlapBox(buildingPreview[previewIndex].position, currentBuilding.transform.localScale / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Building"))
            {
                Debug.Log("Building overlapping.");
                return false;
            }
        }

        lastValidPosition = buildingPreview[previewIndex].position;
        lastValidRotation = buildingPreview[previewIndex].rotation;
        return true;
    }

    void PlaceBuilding(Vector3 position, Quaternion rotation)
    {
        // Deduct resources
        BuildingCost buildingCost = currentBuilding.GetComponent<BuildingCost>();
        if (buildingCost != null)
        {
            wood -= buildingCost.woodCost;
            stone -= buildingCost.stoneCost;
        }
        // Instantiate the building at the last valid position and rotation
        GameObject newBuilding = Instantiate(currentBuilding, position, rotation);
        newBuilding.tag = "Building";
        Transform newBuild = newBuilding.transform.GetChild(0);
        MeshRenderer renderer = newBuild.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 1f, 1f, 1f); // Green color with 50% transparency
        }
        // Reset preview object
        newBuild.GetComponent<Collider>().enabled = true;
        buildingPreview[previewIndex].gameObject.SetActive(false);
        isPlacing = false;
    }
}

// Example class to hold building cost information
public class BuildingCost : MonoBehaviour
{
    public int woodCost = 10;
    public int stoneCost = 5;
}
