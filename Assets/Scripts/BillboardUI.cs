using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;
    public float fixedScale = 1f;
    private Vector3 initialLocalScale;
    void Start()
    {
        mainCamera = Camera.main;
        initialLocalScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
            transform.localScale = initialLocalScale * fixedScale;
        }
    }
}