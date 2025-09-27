using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float minScale = 0.5f;
    public float maxScale = 1f;
    public float pulseDuration = 1f;

    private bool isActive = false;
    private float timer = 0f;

    private void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        float t = Mathf.PingPong(timer / pulseDuration, 1f); // loopen
        t = t * t * (3f - 2f * t); // ease-in-out

        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void ActivateArrow()
    {
        isActive = true;
        timer = 0f;
        transform.localScale = Vector3.one * minScale;
        gameObject.SetActive(true);
    }

    public void DeactivateArrow()
    {
        isActive = false;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }
}
