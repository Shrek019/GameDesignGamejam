using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    private Vector3 targetScale = Vector3.one;
    private Vector3 initialScale = Vector3.zero;
    private float timer = 0f;
    private bool isShowing = false;

    private void Start()
    {
        transform.localScale = Vector3.zero; // start onzichtbaar
    }

    public void ShowArrow()
    {
        isShowing = true;
        timer = 0f;
    }

    public void HideArrow()
    {
        isShowing = false;
        timer = 0f;
    }

    private void Update()
    {
        if (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / animationDuration);

            // Ease-in-out (smoothstep)
            t = t * t * (3f - 2f * t);

            if (isShowing)
                transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            else
                transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, t);
        }
    }
}
