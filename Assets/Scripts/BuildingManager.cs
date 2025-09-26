using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject sandboxPrefab; // vertraagt enemies
    public GameObject wallPrefab;    // blokkeert pad
    public GameObject swingPrefab;   // knockback
    public GameObject slidePrefab;   // schiet ballen

    [Header("Placement Settings")]
    public Material placeholderMaterial;
    public LayerMask groundLayer;
    public float gridSize = 1f;

    [Header("Grid Settings")]
    public GameObject linePrefab;
    public Color gridColor = new Color(0f, 1f, 0f, 0.3f);

    private GameObject currentBuilding;
    private Material[] originalMaterials;
    private bool isPlacing = false;

    private GameObject gridParent;
    private GameObject selectedPrefab;

    private float currentRotation = 0f;

    void Update()
    {
        HandleBuildingSelection();
        HandleRotation();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TogglePlacement();
        }

        if (isPlacing && currentBuilding != null)
        {
            MoveBuildingWithMouse();
            UpdateGrid(currentBuilding.transform.position);

            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding();
                HideGrid();
            }
        }
    }

    #region Building Selection

    void HandleBuildingSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // &
            SelectPrefab(sandboxPrefab);
        if (Input.GetKeyDown(KeyCode.Alpha2)) // é
            SelectPrefab(wallPrefab);
        if (Input.GetKeyDown(KeyCode.Alpha3)) // "
            SelectPrefab(swingPrefab);
        if (Input.GetKeyDown(KeyCode.Alpha4)) // '
            SelectPrefab(slidePrefab);
    }

    void SelectPrefab(GameObject prefab)
    {
        if (isPlacing && currentBuilding != null)
            Destroy(currentBuilding);

        selectedPrefab = prefab;

        if (isPlacing)
        {
            currentBuilding = Instantiate(selectedPrefab);
            SetPlaceholderMaterial(currentBuilding);
            ShowGrid(currentBuilding.transform.position);
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
        }
        else
        {
            if (currentBuilding != null) Destroy(currentBuilding);
            HideGrid();
        }
    }

    #endregion

    #region Rotation

    void HandleRotation()
    {
        if (!isPlacing || currentBuilding == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            currentRotation += 90f;
        else if (scroll < 0f)
            currentRotation -= 90f;

        currentBuilding.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
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

        CreateLine(bl, br);
        CreateLine(br, tr);
        CreateLine(tr, tl);
        CreateLine(tl, bl);
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = Instantiate(linePrefab, gridParent.transform);
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startColor = gridColor;
        lr.endColor = gridColor;
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
        lines[0].SetPosition(0, bl); lines[0].SetPosition(1, br);
        lines[1].SetPosition(0, br); lines[1].SetPosition(1, tr);
        lines[2].SetPosition(0, tr); lines[2].SetPosition(1, tl);
        lines[3].SetPosition(0, tl); lines[3].SetPosition(1, bl);
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

    #region Building Placement

    void MoveBuildingWithMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 snappedPosition = SnapToGrid(hit.point);
            float objectHeight = GetObjectHeight(currentBuilding);
            snappedPosition.y += objectHeight / 2f;

            currentBuilding.transform.position = snappedPosition;
        }
    }

    float GetObjectHeight(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer rend in renderers)
            bounds.Encapsulate(rend.bounds);
        return bounds.size.y;
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, y, z);
    }

    void PlaceBuilding()
    {
        RestoreOriginalMaterials(currentBuilding);
        currentBuilding = null;
    }

    #endregion

    #region Materials

    void SetPlaceholderMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;

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
    }

    void RestoreOriginalMaterials(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = originalMaterials[i];
    }

    #endregion
}
    