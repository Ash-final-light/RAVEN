using Common.Constant;
using UnityEngine;
using UnityEngine.InputSystem;
using GamePlayerInputManager = Tools.InputHandle.PlayerInputManager;

/// <summary>
/// Display-only driver for the movement joystick handle.
/// </summary>
public class MoveJoystickUIDriver : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform handle;

    [Header("Motion")]
    [SerializeField] private float handleMoveRadius = 42f;
    [SerializeField] private float smoothSpeed = 18f;
    [SerializeField] private bool smoothMove = true;
    [SerializeField] private bool readPlayerMoveInput = true;

    private Vector2 input;
    private GamePlayerInputManager inputManager;

    public Vector2 Input => input;
    public Vector2 TargetPosition => Vector2.ClampMagnitude(input, 1f) * handleMoveRadius;

    private void Reset()
    {
        AutoBindUi();
    }

    private void Awake()
    {
        AutoBindUi();
        ResolveInputManager();
    }

    private void OnEnable()
    {
        AutoBindUi();
        ResolveInputManager();
        ApplyImmediate();
    }

    private void Update()
    {
        if (readPlayerMoveInput)
        {
            SetMoveInput(ReadMoveInput());
        }
    }

    private void LateUpdate()
    {
        ApplyView(Time.unscaledDeltaTime);
    }

    public void SetMoveInput(Vector2 value)
    {
        input = Vector2.ClampMagnitude(value, 1f);
    }

    public void ApplyImmediate()
    {
        if (handle != null)
        {
            handle.anchoredPosition = TargetPosition;
        }
    }

    private void ApplyView(float deltaTime)
    {
        if (handle == null)
        {
            return;
        }

        Vector2 targetPosition = TargetPosition;
        if (!smoothMove)
        {
            handle.anchoredPosition = targetPosition;
            return;
        }

        float t = 1f - Mathf.Exp(-Mathf.Max(0f, smoothSpeed) * deltaTime);
        handle.anchoredPosition = Vector2.Lerp(handle.anchoredPosition, targetPosition, t);
    }

    private Vector2 ReadMoveInput()
    {
        ResolveInputManager();
        InputAction moveAction = inputManager != null ? inputManager.GetInputAction(InputConstants.PlayerMove) : null;
        return moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    }

    private void ResolveInputManager()
    {
        if (inputManager == null)
        {
            inputManager = FindFirstObjectByType<GamePlayerInputManager>();
        }
    }

    private void AutoBindUi()
    {
        if (handle != null)
        {
            return;
        }

        Transform handleTransform = transform.Find("Handle");
        if (handleTransform != null)
        {
            handle = handleTransform.GetComponent<RectTransform>();
        }
    }
}
