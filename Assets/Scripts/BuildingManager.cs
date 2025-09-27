using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject sandboxPrefab;
    public GameObject wallPrefab;
    public GameObject swingPrefab;
    public GameObject slidePrefab;

    [Header("References")]
    public MoneyManager moneyManager;

    [Header("Placement Settings")]
    public Material placeholderMaterial;
    public LayerMask groundLayer;
    public float gridSize = 1f;

    [Header("Grid Settings")]
    public GameObject linePrefab;
    public Color gridColor = new Color(0f, 1f, 0f, 0.3f);

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

    void Update()
    {
        HandleBuildingSelection();
        HandleRotation();

        if (Input.GetKeyDown(KeyCode.E)) TogglePlacement();

        if (isPlacing && currentBuilding != null)
        {
            MoveBuildingWithMouse();
            UpdateGrid(currentBuilding.transform.position);
            UpdateRangePreview();

            // Alleen plaatsen als de muis niet over UI is
            if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                PlaceBuilding();
                HideGrid();
            }
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
            ShowGrid(currentBuilding.transform.position);
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
            ShowGrid(currentBuilding.transform.position);
            ShowRangePreview();
        }
        else
        {
            if (currentBuilding != null) Destroy(currentBuilding);
            HideGrid();
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

    #region Grid
    void ShowGrid(Vector3 center)
    {
        if (gridParent != null) Destroy(gridParent);
        gridParent = new GameObject("GridLines");

        float halfSize = gridSize / 2f;
        Vector3 bl = SnapToGrid(center) + new Vector3(-halfSize, 0.01f, -halfSize);
        Vector3 br = SnapToGrid(center) + new Vector3(halfSize, 0.01f, -halfSize);
        Vector3 tr = SnapToGrid(center) + new Vector3(halfSize, 0.01f, halfSize);
        Vector3 tl = SnapToGrid(center) + new Vector3(-halfSize, 0.01f, halfSize);

        CreateLine(bl, br); CreateLine(br, tr);
        CreateLine(tr, tl); CreateLine(tl, bl);
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = Instantiate(linePrefab, gridParent.transform);
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start); lr.SetPosition(1, end);
        lr.startColor = gridColor; lr.endColor = gridColor;
    }

    void UpdateGrid(Vector3 center)
    {
        if (gridParent == null) return;

        float halfSize = gridSize / 2f;
        Vector3 bl = SnapToGrid(center) + new Vector3(-halfSize, 0.01f, -halfSize);
        Vector3 br = SnapToGrid(center) + new Vector3(halfSize, 0.01f, -halfSize);
        Vector3 tr = SnapToGrid(center) + new Vector3(halfSize, 0.01f, halfSize);
        Vector3 tl = SnapToGrid(center) + new Vector3(-halfSize, 0.01f, halfSize);

        LineRenderer[] lines = gridParent.GetComponentsInChildren<LineRenderer>();
        if (lines.Length >= 4)
        {
            lines[0].SetPosition(0, bl); lines[0].SetPosition(1, br);
            lines[1].SetPosition(0, br); lines[1].SetPosition(1, tr);
            lines[2].SetPosition(0, tr); lines[2].SetPosition(1, tl);
            lines[3].SetPosition(0, tl); lines[3].SetPosition(1, bl);
        }
    }

    void HideGrid()
    {
        if (gridParent != null)
        {
            Destroy(gridParent);
            gridParent = null;
        }
    }
    #endregion

    #region Placement
    void MoveBuildingWithMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 snappedPosition = SnapToGrid(hit.point);
            float objectHeight = GetObjectHeight(currentBuilding);
            snappedPosition.y += objectHeight / 2f;
            currentBuilding.transform.position = snappedPosition;

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

    Vector3 SnapToGrid(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
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
        currentBuilding = null;
        DestroyRangePreview();
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
            float range = currentBuilding.GetComponent<Swing>().knockbackForceRange;
            currentRangePreview.transform.localScale = new Vector3(1f, 0.1f, range * 2f);
            UpdateRangePreviewPosition();
        }
        else if (selectedPrefab == slidePrefab)
        {
            float range = currentBuilding.GetComponent<Slide>().detectionRange;
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
            Vector3 offset = currentBuilding.transform.forward * (slideComp.detectionRange / 2f);
            currentRangePreview.transform.position = currentBuilding.transform.position + offset;
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
