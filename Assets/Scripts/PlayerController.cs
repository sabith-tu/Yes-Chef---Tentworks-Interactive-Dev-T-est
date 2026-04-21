using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Rotation")]
    [Tooltip("Degrees per second the character rotates toward movement direction.")]
    public float rotationSpeed = 720f;

    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction interactAction;

    private PlayerInventory inventory;

    private readonly List<IInteractable> stationStack = new List<IInteractable>();
    private IInteractable CurrentStation =>
        stationStack.Count > 0 ? stationStack[stationStack.Count - 1] : null;

    private IInteractable lastPromptedStation;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        interactAction.Enable();
        interactAction.performed += OnInteractPressed;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        interactAction.Disable();
        interactAction.performed -= OnInteractPressed;
    }

    private void Update()
    {
        bool paused = GameManager.Instance != null && GameManager.Instance.IsPaused;
        bool gameActive = GameManager.Instance == null || GameManager.Instance.IsGameActive;

        if (paused || !gameActive)
            return;

        Move();
        UpdateInteractPrompt();
    }

    private void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, 0f, input.y);

        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        if (movement.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(movement.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateInteractPrompt()
    {
        IInteractable station = CurrentStation;

        if (station == null)
        {
            HidePrompt();
            return;
        }

        if (station.AutoInteract)
        {
            HidePrompt();
            return;
        }

        if (station != lastPromptedStation)
        {
            ShowPrompt(station);
            lastPromptedStation = station;
        }
    }

    private void ShowPrompt(IInteractable station)
    {
        InteractPromptUI.Instance?.Show();
    }

    private void HidePrompt()
    {
        if (lastPromptedStation != null)
        {
            InteractPromptUI.Instance?.Hide();
            lastPromptedStation = null;
        }
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (
            GameManager.Instance != null
            && (!GameManager.Instance.IsGameActive || GameManager.Instance.IsPaused)
        )
            return;

        IInteractable station = CurrentStation;
        if (station != null && !station.AutoInteract)
            station.OnInteract(inventory);
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable station = other.GetComponent<IInteractable>();
        if (station == null || stationStack.Contains(station))
            return;

        stationStack.Add(station);
        station.OnEnterTrigger(inventory);

        if (station.AutoInteract)
        {
            if (
                GameManager.Instance == null
                || (GameManager.Instance.IsGameActive && !GameManager.Instance.IsPaused)
            )
            {
                station.OnInteract(inventory);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable station = other.GetComponent<IInteractable>();
        if (station == null)
            return;

        stationStack.Remove(station);
        station.OnExitTrigger(inventory);

        if (station == lastPromptedStation)
        {
            HidePrompt();
        }
    }
}
