using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
                    private float playerSpeed = 5.0f;
                    private float jumpHeight = 1.5f;
                    private float gravityValue = -9.81f;

                    private CharacterController controller;
                    private Vector3 playerVelocity;
                    private bool groundedPlayer;

                    [Header("Input Actions")]
                    public InputActionReference moveAction; // expects Vector2
                    public InputActionReference jumpAction; // expects Button

                    public Transform orientation;
                    
                    public InputActionReference mouseLookAction;

                    [SerializeField] private Transform cameraTransform;
                    [SerializeField] private bool shouldFaceMoveDirection = false;

                    private void Awake()
                    {
                        controller = gameObject.AddComponent<CharacterController>();
                    }

                    private void Start()
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }

                    private void OnEnable()
                    {
                        moveAction.action.Enable();
                        jumpAction.action.Enable();
                        mouseLookAction.action.Enable();
                    }

                    private void OnDisable()
                    {
                        moveAction.action.Disable();
                        jumpAction.action.Disable();
                        mouseLookAction.action.Disable();
                    }

                    void Update()
                    {
                        // Read input
                        Vector2 input = moveAction.action.ReadValue<Vector2>();

// Camera-relative movement
                        Vector3 forward = cameraTransform.forward;
                        Vector3 right = cameraTransform.right;

                        forward.y = 0f;
                        right.y = 0f;

                        forward.Normalize();
                        right.Normalize();

                        Vector3 moveDirection = forward * input.y + right * input.x;

// Ground check
                        groundedPlayer = controller.isGrounded;
                        if (groundedPlayer && playerVelocity.y < 0)
                        {
                            playerVelocity.y = -2f;
                        }

// Jump
                        if (jumpAction.action.triggered && groundedPlayer)
                        {
                            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
                        }

// Apply gravity
                        playerVelocity.y += gravityValue * Time.deltaTime;

// Combine horizontal + vertical
                        Vector3 finalMove = moveDirection * playerSpeed;
                        finalMove.y = playerVelocity.y;

                        controller.Move(finalMove * Time.deltaTime);

// Rotate toward movement direction
                        if (moveDirection.sqrMagnitude > 0.001f)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                        }
}
                }
