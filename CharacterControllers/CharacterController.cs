using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
using OmnicatLabs.StatefulObject;
using UnityEngine.UI;
using Unity.Cinemachine;

namespace OmnicatLabs.CharacterControllers
{
    public class CharacterStates : State<CharacterState>
    {
        private static OmnicatLabs.StatefulObject.AnimationTriggers crouchTriggers = new OmnicatLabs.StatefulObject.AnimationTriggers("Crouch", null, "Uncrouch");

        public static readonly State<CharacterState> Moving = new CharacterStateLibrary.MoveState();
        [DefaultState] public static readonly State<CharacterState> Idle = new CharacterStateLibrary.IdleState();
        public static readonly State<CharacterState> Falling = new CharacterStateLibrary.FallingState();
        public static readonly State<CharacterState> Sprinting = new CharacterStateLibrary.SprintState();
        public static readonly State<CharacterState> Jumping = new CharacterStateLibrary.JumpState();
        public static readonly State<CharacterState> OnSlope = new CharacterStateLibrary.SlopeState();
        public static readonly State<CharacterState> AirJump = new CharacterStateLibrary.AirJumpingState();
        public static readonly State<CharacterState> Crouching = new CharacterStateLibrary.CrouchState(crouchTriggers);
        public static readonly State<CharacterState> CrouchWalk = new CharacterStateLibrary.CrouchWalkState(crouchTriggers);
        public static readonly State<CharacterState> Slide = new CharacterStateLibrary.SlideState();
        public static readonly State<CharacterState> WallRun = new CharacterStateLibrary.WallRunState();
        public static readonly State<CharacterState> Grapple = new CharacterStateLibrary.GrappleState();
    }

    /// <summary>
    /// In the case of Both, the ray and sphere must return 
    /// </summary>
    public enum GroundCheckType
    {
        Box,
        Sphere,
        Raycast,
        Both,
        Either,
    }

    public class CharacterController : StatefulObject<CharacterState>
    {
        public static CharacterController Instance;

        public Camera mainCam;
        public CinemachineCamera vCam;
        public CapsuleCollider modelCollider;

        [Header("General")]
        public float moveSpeed = 100f;
        public float extendedJumpForce = 20f;
        [Tooltip("The maximum in air horizontal velocity the player can ever move at.")]
        public float maxInAirSpeed = 5f;
        [Tooltip("Multiplier of moveSpeed when sprinting")]
        public float sprintMultiplier = 1.2f;
        [Tooltip("Allows sprinting in any direction")]
        public bool multiDirSprint = false;
        [Tooltip("How far in front of the player the controller checks for walls in order to prevent sticking. Generally this be slightly longer than the width of the character")]
        public float wallCheckDistance = .6f;
        public float maxStamina = 100f;
        [Tooltip("The rate at which stamina decreases. Treat as value loss per second. 1 reduction is 1 stamina lost every second of a stamina action")]
        public float staminaReductionRate = 1f;
        public bool sprintUsesStamina = false;
        public float footstepInterval = 5f;
        public float sprintStepInterval = .3f;

        [Header("Ground Checks")]
        public GroundCheckType groundCheckType;
        public Transform groundPoint;
        public Vector3 boxBounds = Vector3.one;
        public float checkRadius = 1f;
        public float checkDistance = 1f;
        public LayerMask groundLayer;
        public UnityEvent onGrounded = new UnityEvent();

        [Header("In Air and Jumps")]
        [Tooltip("Whether extra jumps should enabled by default. Useful for when this is an unlocked ability")]
        public bool extraJumpUnlocked = false;
        [Tooltip("The amount of time the player can jump after having been grounded")]
        public float coyoteTime = .2f;
        public float coyoteModifier = 1.2f;
        [Tooltip("Whether the player can hold jump to increase jump height")]
        public bool extendJumps = true;
        public bool multipleJumps = true;
        public int jumpAmount = 2;
        public float inAirMoveSpeed = 100f;
        public float jumpDuration = .3f;
        public float multiJumpDuration = .3f;
        public float fallForce = 100f;
        public float baseJumpForce = 5f;
        public bool instantAirStop = false;
        [Range(0.001f, 0.999f)]
        [Tooltip("Used to slow down in air movement towards zero when there is no input. The smaller the number, the faster velocity will approach zero")]
        public float slowDown = .2f;
        [Tooltip("The force applied to jumps past the normal first jump. If this is 0 the minJumpForce will be applied instead" +
            "Typically, this should be higher than the standard jump force to counteract your falling speed.")]
        public float multiJumpForce = 5f;
        [Tooltip("Whether the player can hold the jump button to jump longer on multi jumps.")]
        public bool extendMultiJumps = false;
        public float extendedMultiJumpForce = 20f;
        [Tooltip("Resets player's y velocity on landing so that they don't bounce when hitting the ground")]
        public bool lockOnLanding = false;

        [Header("Slopes")]
        [Tooltip("You can find a representation of this in the cyan line drawn from the character")]
        public float slopeCheckDistance = 1f;
        [Tooltip("If the angle of the slope is higher than this the player will simply slide off")]
        public float maxSlopeAngle = 60f;
        public float minSlopeAngle = 10f;
        [Tooltip("Only things included in this mask will elegible for detection")]
        public LayerMask slopeCheckFilter;
        [Tooltip("A position from which the controller will check for slopes. Ideally position this close to front of the character or else there can be jitters when entering a slope")]
        public Transform slopeCheckPoint;
        [Tooltip("Whether the character will be able to slide while on steep angles")]
        public bool useGravity = true;
        [Tooltip("When enabled the character will move at the same speed on a slope as they do on the ground. " +
            "If disabled the character will have to fight against the natural physics that govern slopes. " +
            "useGravity and this setting function independently.")]
        public bool maintainVelocity = true;
        public float slopeSpeed = 130f;

        [Header("Crouching/Sliding")]
        public LayerMask testMask;
        public float crouchHeight = 0.5f;
        public float originalHeight = 1.5f;
        [Tooltip("The time in seconds it takes to go from standing to crouching")]
        public float toCrouchSpeed = .2f;
        [Tooltip("Toggle crouch on/off on button press instead of hold to crouch")]
        public bool useToggle = false;
        [Tooltip("Modifier on the movement speed when crouched.")]
        public float crouchSpeedModifier = 0.5f;
        public float slideSpeed = 10f;
        public float slideSpeedReduction = .999f;
        [Tooltip("The threshold that controls when a slide is forced to end. The higher the number, the quicker the slide will stop")]
        public float slideStopThreshold = 1.5f;
        public float slideTransitionSpeed = .2f;
        public bool slideUsesStamina = true;

        [Header("Wall Running")]
        public bool wallRunningUnlocked;
        public LayerMask runnableLayers;
        public float wallRunSpeed = 500f;
        public float maxWallRunTime = 3f;
        public float wallJumpForce = 10f;
        public float wallJumpSideForce = 10f;
        public float wallFallForce = 10f;
        public float wallRunFOV = 80f;
        [Tooltip("Degrees of rotation. +/- depending on orientation")]
        public float wallRunCameraTilt = 15f;
        [Tooltip("Minimum time required to be on the wall before you can jump")]
        public float minTimeToWallJump = .5f;

        [Header("Grappling")]
        public Transform barrelPoint;
        public LayerMask grappleableLayers;
        public float maxGrappleDistance = 10f;
        public float grappleDelayTime = .5f;
        public float grapplingCooldown = .5f;
        public LineRenderer cableRenderer;
        public float overShootYAxis;
        public int ropeQuality = 500;
        public float ropeDamper = 14f;
        public float strength = 800f;
        public float velocity = 15f;
        public float waveCount = 3f;
        public float waveHeight = 1f;
        public float grapplingFOV = 120f;
        public AnimationCurve effectCurve;
        public bool grappleUnlocked = false;


        [Header("UI")]
        //public Slider staminaSlider;

        internal Vector3 movementDir;
        internal bool isGrounded = true;
        private RaycastHit groundHit;
        private bool wasGrounded;
        internal bool jumpKeyDown = false;
        internal bool canJump = true;
        internal int currentJumpAmount = 0;
        internal UnityEvent onAirJump = new UnityEvent();
        internal bool sprinting = false;
        internal bool onSlope;
        internal RaycastHit slopeHit;
        internal float groundAngle;
        internal bool shouldCrouch = false;
        internal bool isCrouching = false;
        internal bool slideKeyDown = false;
        [HideInInspector]
        public Rigidbody rb;
        internal RaycastHit leftWallHit;
        internal RaycastHit rightWallHit;
        internal bool wallLeft;
        internal bool wallRight;
        internal bool wallRunning = false;
        internal Vector3 fixedGroundPoint;
        internal bool isLocked = false;
        internal float currentStamina;
        internal Transform camHolder;
        [HideInInspector]
        public bool playerIsHidden = false;
        [HideInInspector]
        public float startingCamHeight;
        [HideInInspector]
        public float savedStamina = 0f;
        internal bool canWallRun = true;
        internal bool grappling = false;
        internal bool canGrapple = true;
        [HideInInspector]
        public MouseLook mouseControls;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == null)
            {
                Instance = this;
            }
            rb = GetComponent<Rigidbody>();
            mouseControls = GetComponentInChildren<MouseLook>();
        }

        private void Start()
        {
            camHolder = mainCam.transform.parent;
            startingCamHeight = camHolder.transform.localPosition.y;
            //if (staminaSlider == null)
            //{
            //    Debug.Log("The slider for stamina has not been set in the inspector");
            //}
            //else
            //{
            //    staminaSlider.maxValue = maxStamina;
            //    staminaSlider.value = maxStamina;
            //}
            //SaveManager.Instance.onReset.AddListener(ResetStamina);
            currentStamina = maxStamina;
            fixedGroundPoint = groundPoint.localPosition;
        }

        private void ResetStamina()
        {
            currentStamina = savedStamina;
            //staminaSlider.value = currentStamina;
        }

        protected override void Update()
        {
            base.Update();
            GroundCheck();
            SlopeCheck();
            WallRunCheck();
            WallRunCheck();
            Debug.Log(state);
            //Debug.Log(isGrounded);
            //Debug.Log(state.ToString() + isCrouching.ToString());
            //Debug.Log(isCrouching);
            //Debug.Log(onSlope);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void ChangeStamina(float value)
        {
            currentStamina = value;
            if (currentStamina <= 0f)
            {
                sprinting = false;
                currentStamina = 0f;
            }
            //if (staminaSlider != null)
            //    staminaSlider.value = currentStamina;
        }

        private void WallRunCheck()
        {
            wallRight = Physics.Raycast(transform.position + modelCollider.center, transform.right, out rightWallHit, wallCheckDistance, runnableLayers);
            wallLeft = Physics.Raycast(transform.position + modelCollider.center, -transform.right, out leftWallHit, wallCheckDistance, runnableLayers);
        }

        private void WallCheck()
        {
            UnityEngine.Debug.DrawRay(transform.position, transform.TransformVector(movementDir) * wallCheckDistance, Color.red);
            if (Physics.Raycast(transform.position, transform.TransformVector(movementDir), wallCheckDistance))
            {
                GetComponent<Rigidbody>().linearVelocity = new Vector3(0f, GetComponent<Rigidbody>().linearVelocity.y, 0f);
            }
        }

        private void SlopeCheck()
        {
            if (Physics.Raycast(slopeCheckPoint.position, Vector3.down, out slopeHit, slopeCheckDistance, slopeCheckFilter))
            {
                groundAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                onSlope = groundAngle < maxSlopeAngle && groundAngle != 0 && groundAngle > minSlopeAngle;
            }
            else onSlope = false;

            //if (!useGravity)
            //{
            //    GetComponent<Rigidbody>().useGravity = !onSlope;
            //}
        }

        private void GroundCheck()
        {
            switch (groundCheckType)
            {
                case GroundCheckType.Box:
                    isGrounded = Physics.CheckBox(groundPoint.position, boxBounds / 2f, Quaternion.identity, groundLayer, QueryTriggerInteraction.Ignore);
                    //if (isGrounded)
                    //{
                    //    Physics.Raycast(groundPoint.position, Vector3.down, out groundHit, boxBounds.y, groundLayer);
                    //}
                    break;
                case GroundCheckType.Raycast:
                    isGrounded = Physics.Raycast(groundPoint.position, Vector3.down, out groundHit, checkDistance, groundLayer);
                    break;
                case GroundCheckType.Sphere:
                    isGrounded = Physics.CheckSphere(groundPoint.position, checkRadius, groundLayer);
                    break;
                case GroundCheckType.Both:
                    isGrounded = Physics.Raycast(groundPoint.position, Vector3.down, out groundHit, checkDistance, groundLayer) &
                        Physics.CheckSphere(groundPoint.position, checkRadius, groundLayer);
                    break;
                case GroundCheckType.Either:
                    isGrounded = Physics.Raycast(groundPoint.position, Vector3.down, out groundHit, checkDistance, groundLayer) |
                        Physics.CheckSphere(groundPoint.position, checkRadius, groundLayer);
                    break;
            }

            //checks if we are grounded this frame after we were not last frame
            if (isGrounded && !wasGrounded)
            {
                onGrounded.Invoke();
                currentJumpAmount = 0;
                canWallRun = true;
            }

            if (!isGrounded && !onSlope && !wallRunning && !grappling)
            {
                ChangeState(CharacterStates.Falling);
            }

            wasGrounded = isGrounded;
        }


        #region Input Callbacks
        public void OnGrapple(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && grappleUnlocked)
            {
                canGrapple = false;
                ChangeState(CharacterStates.Grapple);
            }
        }


        public void OnMove(InputAction.CallbackContext context)
        {
            movementDir = context.ReadValue<Vector3>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            //if (context.performed && !sprinting && !Audio.AudioManager.Instance.IsPlaying("PlayerBreath") && currentStamina > 0f)
            //{
            //    Audio.AudioManager.Instance.Play("PlayerBreath", Audio.SoundMode.Instant);
            //}

            if ((context.performed && movementDir.z > 0) || (context.performed && multiDirSprint) && currentStamina > 0f)
            {
                sprinting = !sprinting;
            }

            if (context.canceled && slideKeyDown)
            {
                sprinting = false;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (isPaused)
            {
                return;
            }

            if (context.performed && !isGrounded && currentJumpAmount < jumpAmount)
            {
                jumpKeyDown = true;
                onAirJump.Invoke();
            }

            if (context.performed && isGrounded && !isLocked && !isCrouching)
            {
                jumpKeyDown = true;
                ChangeState(CharacterStates.Jumping);
            }

            if (context.canceled)
            {
                jumpKeyDown = false;
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed && isGrounded)
            {
                slideKeyDown = true;
            }
            if (context.canceled)
            {
                slideKeyDown = false;
            }

            if (useToggle)
            {
                if (context.performed && isGrounded)
                {
                    shouldCrouch = !shouldCrouch;
                }
            }
            else
            {
                if (context.performed && isGrounded)
                {
                    shouldCrouch = true;
                }

                if (context.canceled)
                {
                    shouldCrouch = false;

                }
            }
        }
        #endregion

        #region Locks and Unlocks
        public void TogglePause()
        {
            SetPause(!isPaused);
        }

        public void SetLockedNoDisable(bool value, bool hidePlayer, bool unlockCursor)
        {
            ChangeState(CharacterStates.Idle);
            SetPause(value);
            mouseControls.enabled = !value;
            if (unlockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = Cursor.visible = false;
            }

            if (hidePlayer)
            {
                foreach (Transform child in mainCam.GetComponentsInChildren<Transform>())
                {
                    if (child.gameObject.layer == LayerMask.NameToLayer("Weapon"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                playerIsHidden = true;
            }
            else
            {
                foreach (Transform child in mainCam.GetComponentsInChildren<Transform>(true))
                {
                    if (child.gameObject.layer == LayerMask.NameToLayer("Weapon"))
                    {
                        child.gameObject.SetActive(true);
                    }
                }

                playerIsHidden = false;
            }
        }

        public void SetControllerLocked(bool value, bool hidePlayer, bool unlockCursor)
        {
            SetPause(value);
            GetComponent<PlayerInput>().enabled = !value;
            isLocked = value;
            GetComponentInChildren<MouseLook>().enabled = !value;

            if (hidePlayer)
            {
                foreach (Transform child in mainCam.GetComponentsInChildren<Transform>())
                {
                    if (child.gameObject.layer == LayerMask.NameToLayer("Weapon"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                playerIsHidden = true;
            }
            else
            {
                foreach (Transform child in mainCam.GetComponentsInChildren<Transform>(true))
                {
                    if (child.gameObject.layer == LayerMask.NameToLayer("Weapon"))
                    {
                        child.gameObject.SetActive(true);
                    }
                }

                playerIsHidden = false;
            }

            if (unlockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = Cursor.visible = false;
            }
        }


        /// <summary>
        /// Unlocks extra jumps with the amount of jumps being set in the editor
        /// </summary>
        public void UnlockExtraJumps()
        {
            extraJumpUnlocked = true;
        }
        /// <summary>
        /// Unlocks extra jumps with the amount passed in, bypassing the current amount in the editor
        /// </summary>
        /// <param name="amount">The new amount of jumps you want the player to have</param>
        public void UnlockExtraJumps(int amount)
        {
            extraJumpUnlocked = true;
            jumpAmount = amount;
        }

        /// <summary>
        /// Locks extra jumps prevent the player from using them
        /// </summary>
        public void LockExtraJumps()
        {
            extraJumpUnlocked = false;
        }
        #endregion

        private void OnDrawGizmosSelected()
        {
            if (groundPoint != null)
            {
                Gizmos.color = Color.green;
                switch (groundCheckType)
                {
                    case GroundCheckType.Box:
                        //isGrounded = Physics.CheckBox(groundPoint.position, boxBounds / 2f, Quaternion.identity, groundLayer, QueryTriggerInteraction.Ignore);
                        Gizmos.DrawWireCube(groundPoint.position, boxBounds);
                        break;
                    case GroundCheckType.Raycast:
                        Gizmos.DrawRay(groundPoint.position, Vector3.down * checkDistance);
                        break;
                    case GroundCheckType.Sphere:
                        Gizmos.DrawWireSphere(groundPoint.position, checkRadius);
                        break;
                    case GroundCheckType.Both:
                    case GroundCheckType.Either:
                        Gizmos.DrawWireSphere(groundPoint.position, checkRadius);
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(groundPoint.position, Vector3.down * checkDistance);
                        break;
                }
            }

            if (slopeCheckPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(slopeCheckPoint.position, Vector3.down * slopeCheckDistance);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + modelCollider.center, transform.right * wallCheckDistance);
            Gizmos.DrawRay(transform.position + modelCollider.center, -transform.right * wallCheckDistance);
        }
    }
}

