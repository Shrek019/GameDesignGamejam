using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Settings")]
    public GameObject buildingPrefab;
    public Material placeholderMaterial;
    public LayerMask groundLayer;
    public float gridSize = 1f;

    [Header("Grid Settings")]
    public GameObject linePrefab;
    public int gridWidth = 1;    // we tonen nu 1x1 rond het blokje
    public int gridHeight = 1;
    public Color gridColor = new Color(0f, 1f, 0f, 0.3f); // transparant

    private GameObject currentBuilding;
    private Material[] originalMaterials;
    private bool isPlacing = false;

    private GameObject gridParent;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isPlacing = !isPlacing;

            if (isPlacing)
            {
                currentBuilding = Instantiate(buildingPrefab);
                SetPlaceholderMaterial(currentBuilding);
                ShowGrid(currentBuilding.transform.position);
            }
            else
            {
                if (currentBuilding != null) Destroy(currentBuilding);
                HideGrid();
            }
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

    #region Grid

    void ShowGrid(Vector3 center)
    {
        if (gridParent != null) Destroy(gridParent);
        gridParent = new GameObject("GridLines");

        float halfSize = gridSize / 2f;

        // Teken 4 lijnen rond het blokje (vierkant)
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

        // Grid volgt het blokje
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