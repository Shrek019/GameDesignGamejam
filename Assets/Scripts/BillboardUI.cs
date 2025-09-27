// Nieuw script: BillboardFollow.cs
using UnityEngine;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    public Transform target;
    public float heightOffset = 2f;
    private Camera mainCamera;
    public Slider slider;
    public GameObject buildingObject; // verwijzing naar het gebouw
    private BuildingManager buildingManager; // component

    void Start()
    {
        mainCamera = Camera.main;


        if (buildingObject != null)
        {
            buildingManager = buildingObject.GetComponent<BuildingManager>();
        }
    }

    void LateUpdate()
    {
        if (target == null || mainCamera == null) return;

        // Position boven het gebouw
        transform.position = target.position + Vector3.up * heightOffset;

        // Kijk naar camera
        transform.forward = mainCamera.transform.forward;

        // update slider
        if (slider != null && buildingManager != null)
        {
            slider.value = buildingManager.health;
        }

    }
}
