using System;
using UnityEngine;
using NaughtyAttributes;

namespace HPlayer
{
    public class PlayerController : MonoBehaviour
    {
        public enum Mods { None, Default, Sprint }

        #region Camera

        [Header("Camera controll")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 8f;
        [SerializeField] private float minPitch = -90f;
        [SerializeField] private float maxPitch = 90f;

        private float yaw;
        private float pitch;

        [field: SerializeField, ReadOnly] public bool FreezeCamera { get; set; }

        #endregion

        #region Movement

        [Header("Default")]
        [SerializeField] private float defaultSpeed = 5f;

        // ⚠ tetap ada kalau nanti kepakai (biar tidak warning)
        // [SerializeField] private float defaultHeight = 1.9f;

        [SerializeField] private float smoothMoveTime = 0.1f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private bool canJump = true;

        [SerializeField, ShowIf(nameof(canJump))]
        private float defaultJumpHeight = 1.2f;

        [Header("Sprint")]
        [SerializeField] private bool canSprint = true;

        [SerializeField, ShowIf(nameof(canSprint))]
        private float sprintSpeed = 9f;

        [SerializeField, ShowIf(EConditionOperator.And, nameof(canSprint), nameof(canJump))]
        private float sprintJumpHeight = 1.4f;

        [Header("Inputs")]
        [SerializeField, ReadOnly] private bool inputJump;
        [SerializeField, ReadOnly] private bool inputSprint;
        [SerializeField, ReadOnly] private Vector2 inputMove;
        [SerializeField, ReadOnly] private Vector2 inputMouse;

        [Header("States")]
        [SerializeField, ReadOnly] private bool isGrounded;
        [SerializeField, ReadOnly] private Mods currentMod;
        [SerializeField, ReadOnly] private float currentSpeed;
        [SerializeField, ReadOnly] private float currentJumpHeight;

        private Vector3 smoothV;
        private Vector3 velocity;
        private float verticalVelocity;

        [field: SerializeField, ReadOnly] public bool FreezeMovement { get; set; }

        #endregion

        private CharacterController controller;

        public static Action OnPlayerEnterPortal;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            SetMode(Mods.Default);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yaw = transform.eulerAngles.y;
            pitch = playerCamera.transform.localEulerAngles.x;
        }

        private void Update()
        {
            UpdateGroundCheck();
            GetInput();
            MoveCamera();
            ChooseMode();
            MovePlayer();
        }

        #region Core

        private void UpdateGroundCheck()
        {
            float radius = controller.radius * 0.9f;
            Vector3 groundCheck = controller.bounds.center - Vector3.up * (controller.bounds.extents.y - radius + 0.2f);

            isGrounded = Physics.CheckSphere(groundCheck, radius, groundMask);
        }

        private void SetMode(Mods mod)
        {
            if (currentMod == mod) return;

            switch (mod)
            {
                case Mods.Default:
                    currentSpeed = defaultSpeed;
                    currentJumpHeight = defaultJumpHeight;
                    break;

                case Mods.Sprint:
                    currentSpeed = sprintSpeed;
                    currentJumpHeight = sprintJumpHeight;
                    break;

                default:
                    currentSpeed = 0;
                    currentJumpHeight = 0;
                    break;
            }

            currentMod = mod;
        }

        private void ChooseMode()
        {
            if (canSprint && inputSprint)
                SetMode(Mods.Sprint);
            else
                SetMode(Mods.Default);
        }

        private void GetInput()
        {
            inputMove = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            inputMouse = new Vector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            );

            inputSprint = Input.GetKey(KeyCode.LeftShift);

            // FIX: jump lebih natural
            inputJump = Input.GetKeyDown(KeyCode.Space);
        }

        private void MoveCamera()
        {
            if (FreezeCamera || playerCamera == null) return;

            yaw += inputMouse.x * mouseSensitivity;
            pitch -= inputMouse.y * mouseSensitivity;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.eulerAngles = Vector3.up * yaw;
            playerCamera.transform.localEulerAngles = Vector3.right * pitch;
        }

        private void MovePlayer()
        {
            if (FreezeMovement) return;

            Vector3 inputDir = new Vector3(inputMove.x, 0, inputMove.y).normalized;
            Vector3 worldDir = transform.TransformDirection(inputDir);

            Vector3 targetVelocity = worldDir * currentSpeed;
            velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothV, smoothMoveTime);

            if (isGrounded)
            {
                if (verticalVelocity < 0)
                    verticalVelocity = -2f;

                if (canJump && inputJump)
                    verticalVelocity = Mathf.Sqrt(currentJumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 finalVelocity = new Vector3(velocity.x, verticalVelocity, velocity.z);
            controller.Move(finalVelocity * Time.deltaTime);
        }

        #endregion

        #region Utilities

        public void SetPosition(Vector3 position)
        {
            controller.enabled = false;

            transform.position = position;
            velocity = Vector3.zero;
            smoothV = Vector3.zero;
            verticalVelocity = 0;

            controller.enabled = true;
        }

        public void SetRotation(float rotX, float rotY)
        {
            pitch = rotX;
            yaw = rotY;

            playerCamera.transform.localEulerAngles = Vector3.right * pitch;
            transform.eulerAngles = Vector3.up * yaw;
        }

        public void SetEnable(bool enableState)
        {
            FreezeCamera = !enableState;
            FreezeMovement = !enableState;
            enabled = enableState;
        }

        #endregion
    }
}