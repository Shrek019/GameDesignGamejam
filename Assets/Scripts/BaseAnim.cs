using UnityEngine;

public class PlayLoopAnimation : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator animator;
    public string animationStateName;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Geen Animator gevonden op " + gameObject.name);
            return;
        }

        if (!string.IsNullOrEmpty(animationStateName))
        {
            animator.Play(animationStateName);
        }
    }
}