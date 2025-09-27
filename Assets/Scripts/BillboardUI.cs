using UnityEngine;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    public Transform target;
    public float heightOffset = 2f;
    private Camera mainCamera;
    public Slider slider;
    public BuildingManager buildingManager; // link naar het script om health te volgen

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null || mainCamera == null) return;

        // Zorg dat de healthbar boven het gebouw blijft
        transform.position = target.position + Vector3.up * heightOffset;

        // Kijk altijd naar de camera
        transform.forward = mainCamera.transform.forward;

        // update slider op basis van BuildingManager health
        if (slider != null && buildingManager != null)
        {
            slider.value = buildingManager.health;
        }
    }
}
