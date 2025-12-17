using UnityEngine;
using UnityEngine.InputSystem;

public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance { get; private set; }
    [SerializeField] private MobileJoystick joystick;
    
    [SerializeField] private UnityEngine.UI.Button interactButton;

    [HideInInspector] public bool interactPressed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

#if UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(true);
#else
        gameObject.SetActive(false);
#endif

        interactButton.onClick.AddListener(() => interactPressed = true);
    }

    public Vector2 GetJoystickDirection()
    {
#if UNITY_ANDROID || UNITY_IOS
        return joystick.Direction;
#else
        return Vector2.zero;
#endif
    }

    public void ResetInteract()
    {
        interactPressed = false;
    }
}
