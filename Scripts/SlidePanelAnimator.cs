using UnityEngine;
using UnityEngine.EventSystems;

public class SlidePanelAnimator : MonoBehaviour
{
    private Animator anim;
    private bool isOpen;

    [SerializeField] private bool startOpen = false;
    [SerializeField] private float clickCooldown = 0.15f;
    private float nextAllowedTime;

    void Awake()
    {
        anim = GetComponent<Animator>();
        isOpen = startOpen;
        anim.SetBool("IsOpen", isOpen);
    }

    public void TogglePanel()
    {
        if (Time.unscaledTime < nextAllowedTime) return;
        nextAllowedTime = Time.unscaledTime + clickCooldown;

        isOpen = !isOpen;
        anim.SetBool("IsOpen", isOpen);

        // prevents “selected button” weirdness
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
