using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject sandboxPrefab;
    public GameObject wallPrefab;
    public GameObject swingPrefab;
    public GameObject slidePrefab;

    [Header("References")]
    public MoneyManager moneyManager;
    public CardManager cardManager;

    [Header("Placement Settings")]
    public Material placeholderMaterial;
    public LayerMask groundLayer;

    [Header("Range Preview")]
    public GameObject rangePreviewPrefab;

    [HideInInspector] public int currentBuildingCost = 0;

    private GameObject currentBuilding;
    private GameObject currentRangePreview;
    private Dictionary<GameObject, Material[]> originalMaterialsDict = new Dictionary<GameObject, Material[]>();
    private bool isPlacing = false;
    private GameObject gridParent;
    private GameObject selectedPrefab;
    private float currentRotation = 0f;

    [Header("UI")]
    public GameObject healthBarPrefab; // world-space canvas met slider
    private GameObject buildingInstance;
    private Slider buildingHealthSlider;
    private GameObject buildingHealthBar;

    public int health = 20;

    void Update()
    {
        HandleBuildingSelection();
        HandleRotation();

        if (Input.GetKeyDown(KeyCode.E)) TogglePlacement();

        if (isPlacing && currentBuilding != null)
        {
            MoveBuildingWithMouse();
            UpdateRangePreview();

            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding();
                DestroyRangePreview();
            }
        }
    }
    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    #region Building Selection
    void HandleBuildingSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectPrefab(sandboxPrefab);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectPrefab(wallPrefab);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectPrefab(swingPrefab);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectPrefab(slidePrefab);
    }

    void SelectPrefab(GameObject prefab)
    {
        if (isPlacing && currentBuilding != null) Destroy(currentBuilding);

        selectedPrefab = prefab;
        if (isPlacing)
        {
            currentBuilding = Instantiate(selectedPrefab);
            SetPlaceholderMaterial(currentBuilding);
            ShowRangePreview();
        }
    }

    void TogglePlacement()
    {
        isPlacing = !isPlacing;

        if (isPlacing && selectedPrefab != null)
        {
            currentBuilding = Instantiate(selectedPrefab);
            SetPlaceholderMaterial(currentBuilding);
            ShowRangePreview();
        }
        else
        {
            if (currentBuilding != null) Destroy(currentBuilding);
            DestroyRangePreview();
        }
    }
    #endregion

    #region Rotation
    void HandleRotation()
    {
        if (!isPlacing || currentBuilding == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) currentRotation += 90f;
        else if (scroll < 0f) currentRotation -= 90f;

        currentBuilding.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

        if (currentRangePreview != null) UpdateRangePreviewPosition();
    }
    #endregion

    #region Placement
    void MoveBuildingWithMouse()
    {
        if (currentBuilding == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 position = hit.point; // GEEN SnapToGrid
            float objectHeight = GetObjectHeight(currentBuilding);
            position.y += objectHeight / 2f;
            currentBuilding.transform.position = position;

            if (currentRangePreview != null) UpdateRangePreviewPosition();
        }
    }

    float GetObjectHeight(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return 1f;
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer rend in renderers) bounds.Encapsulate(rend.bounds);
        return bounds.size.y;
    }

    void PlaceBuilding()
    {
        int cost = 0;

        if (selectedPrefab == sandboxPrefab) cost = 10;
        else if (selectedPrefab == wallPrefab) cost = 5;
        else if (selectedPrefab == swingPrefab) cost = 20;
        else if (selectedPrefab == slidePrefab) cost = 40;

        if (moneyManager != null)
        {
            bool couldSpend = moneyManager.SpendMoney(cost);
            if (!couldSpend)
            {
                Debug.Log("Niet genoeg geld om deze building te plaatsen!");
                return;
            }
        }

        RestoreOriginalMaterials(currentBuilding);
        buildingInstance = currentBuilding;

        currentBuilding = null;
        DestroyRangePreview();

        // In PlaceBuilding()
        if (healthBarPrefab != null)
        {
            // Instantiate los van parent
            buildingHealthBar = Instantiate(
                healthBarPrefab,
                buildingInstance.transform.position + Vector3.up * 2f,
                Quaternion.identity
            );

            buildingHealthSlider = buildingHealthBar.GetComponentInChildren<Slider>();
            buildingHealthSlider.maxValue = health;
            buildingHealthSlider.value = health;


        }
    }


    #endregion

    #region Materials
    void SetPlaceholderMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Material[] mats = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            mats[i] = renderers[i].material;

            Material mat = new Material(placeholderMaterial);
            Color c = mat.color;
            c.a = 0.5f;
            mat.color = c;

            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            renderers[i].material = mat;
        }
        originalMaterialsDict[obj] = mats;
    }

    void RestoreOriginalMaterials(GameObject obj)
    {
        if (obj == null) return;
        if (originalMaterialsDict.TryGetValue(obj, out Material[] mats))
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < Mathf.Min(renderers.Length, mats.Length); i++)
                renderers[i].material = mats[i];

            originalMaterialsDict.Remove(obj);
        }
    }
    #endregion

    #region Range Preview
    void ShowRangePreview()
    {
        DestroyRangePreview();
        if (selectedPrefab == null || currentBuilding == null) return;

        currentRangePreview = Instantiate(rangePreviewPrefab, currentBuilding.transform.position, Quaternion.identity, currentBuilding.transform);

        if (selectedPrefab == swingPrefab)
        {
            float range = currentBuilding.GetComponent<Swing>().range;

            currentRangePreview.transform.localScale = new Vector3(1f, 0.1f, range * 2f);
            UpdateRangePreviewPosition();
        }
        else if (selectedPrefab == slidePrefab)
        {
            float range = currentBuilding.GetComponent<Slide>().range;

            currentRangePreview.transform.localScale = new Vector3(1f, 0.1f, range);
            UpdateRangePreviewPosition();
        }
    }

    void UpdateRangePreview()
    {
        if (currentRangePreview != null) UpdateRangePreviewPosition();
    }

    void UpdateRangePreviewPosition()
    {
        if (currentBuilding == null || currentRangePreview == null) return;

        if (selectedPrefab == swingPrefab)
        {
            currentBuilding.transform.rotation = Quaternion.Euler(-90f, currentRotation, 0f);
            currentRangePreview.transform.rotation = Quaternion.Euler(0f, currentBuilding.transform.eulerAngles.y, 0f);
        }
        else if (selectedPrefab == slidePrefab)
        {
            Slide slideComp = currentBuilding.GetComponent<Slide>();
            Vector3 offset = currentBuilding.transform.forward * (slideComp.range / 2f);
            currentRangePreview.transform.position = currentBuilding.transform.position;// + offset;
            currentRangePreview.transform.rotation = Quaternion.Euler(0f, currentBuilding.transform.eulerAngles.y, 0f);
        }
    }


    void DestroyRangePreview()
    {
        if (currentRangePreview != null)
        {
            Destroy(currentRangePreview);
            currentRangePreview = null;
        }
    }
    // Selecteer prefab via UI-kaart
    public void SelectBuildingFromUI(int index, int cost)
    {
        currentBuildingCost = cost;

        switch (index)
        {
            case 0: SelectPrefab(sandboxPrefab); break;
            case 1: SelectPrefab(wallPrefab); break;
            case 2: SelectPrefab(swingPrefab); break;
            case 3: SelectPrefab(slidePrefab); break;
        }

        if (!isPlacing) TogglePlacement();
    }


    #endregion
}
