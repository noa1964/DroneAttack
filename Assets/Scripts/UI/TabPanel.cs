using UnityEngine;

public class TabPanel : MonoBehaviour
{
    [Header("Optional Animator for open/close animations")]
    [SerializeField] private Animator animator;

    public bool IsOpen { get; private set; } = false;

    /// <summary>
    /// Open the panel (play animation if available)
    /// </summary>
    public void Open()
    {
        if (IsOpen) return;

        gameObject.SetActive(true);

        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        IsOpen = true;
    }

    /// <summary>
    /// Close the panel (play animation if available)
    /// </summary>
    public void Close()
    {
        if (!IsOpen) return;

        if (animator != null)
        {
            animator.SetTrigger("Close");
        }
        else
        {
            // If no animator, just deactivate immediately
            gameObject.SetActive(false);
        }

        IsOpen = false;
    }

    /// <summary>
    /// Optional callback when the close animation ends
    /// Call this via Animation Event
    /// </summary>
    public void OnCloseAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}
