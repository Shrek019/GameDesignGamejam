using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;      // snelheid van de camera
    public float edgeSize = 10f;       // afstand van rand in pixels waar muis detectie begint

    [Header("Limits")]
    public Vector2 minBounds;          // minimum X/Z voor beweging
    public Vector2 maxBounds;          // maximum X/Z voor beweging

    public DayManagerTMP_Fade dayManager;

    private Vector3 startPosition;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        startPosition = transform.position;
    }

    void Update()
    {
        if (dayManager != null && dayManager.IsUICardActive())
        {
            ResetToStartPosition();
            return; // geen beweging
        }

        Vector3 pos = transform.position;

        // ---- Keyboard Input (Arrow keys) ----
        if (Input.GetKey(KeyCode.UpArrow)) pos.z += moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) pos.z -= moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) pos.x += moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow)) pos.x -= moveSpeed * Time.deltaTime;

        // ---- Mouse Edge Input ----
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x >= Screen.width - edgeSize) pos.x += moveSpeed * Time.deltaTime;
        if (mousePos.x <= edgeSize) pos.x -= moveSpeed * Time.deltaTime;
        if (mousePos.y >= Screen.height - edgeSize) pos.z += moveSpeed * Time.deltaTime;
        if (mousePos.y <= edgeSize) pos.z -= moveSpeed * Time.deltaTime;

        // ---- Clamp binnen grenzen ----
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);

        transform.position = pos;
    }
    public void ResetToStartPosition()
    {
        transform.position = startPosition;
    }
}
