using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
using OmnicatLabs.StatefulObject;

namespace OmnicatLabs.CharacterControllers
{
    public class CharacterStates : State<CharacterState>
    {
        private static AnimationTriggers crouchTriggers = new AnimationTriggers("Crouch", null, "Uncrouch");

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
    }

    /// <summary>
    /// In the case of Both, the ray and sphere must return 
    /// </summary>
    public enum GroundCheckType
    {
        Sphere,
        Raycast,
        Both,
        Either,
    }

    public class CharacterController : StatefulObject<CharacterState>
    {
        public Camera mainCam;
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

        [Header("Ground Checks")]
        public GroundCheckType groundCheckType;
        public Transform groundPoint;
        public float checkRadius = 1f;
        public float checkDistance = 1f;
        public LayerMask groundLayer;
        public UnityEvent onGrounded = new UnityEvent();

        [Header("In Air and Jumps")]
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

        [Header("Crouching/Sliding")]
        public float crouchHeight = 0.5f;
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
        internal bool isCrouching = false;
        internal bool slideKeyDown = false;
        [HideInInspector]
        public Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        protected override void Update()
        {
            base.Update();
            GroundCheck();
            SlopeCheck();
            WallCheck();
            //Debug.Log(state);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        private void WallCheck()
        {
            UnityEngine.Debug.DrawRay(transform.position, transform.TransformVector(movementDir) * wallCheckDistance, Color.red);
            if (Physics.Raycast(transform.position, transform.TransformVector(movementDir), wallCheckDistance))
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0f, GetComponent<Rigidbody>().velocity.y, 0f);
            }
        }

        private void SlopeCheck()
        {
            if (Physics.Raycast(slopeCheckPoint.position, Vector3.down, out slopeHit, slopeCheckDistance, slopeCheckFilter))
            {
                groundAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                onSlope = groundAngle < maxSlopeAngle && groundAngle != 0;
            }
            else onSlope = false;

            if (!useGravity)
            {
                GetComponent<Rigidbody>().useGravity = !onSlope;
            }
        }

        private void GroundCheck()
        {
            switch (groundCheckType)
            {
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
            }

            if (!isGrounded && !onSlope)
            {
                ChangeState(CharacterStates.Falling);
            }

            wasGrounded = isGrounded;
        }


        #region Input Callbacks
        public void OnMove(InputAction.CallbackContext context)
        {
            movementDir = context.ReadValue<Vector3>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if ((context.performed && movementDir.z > 0) || (context.performed && multiDirSprint))
            {
                sprinting = true;
            }

            if (context.canceled && slideKeyDown)
            {
                sprinting = false;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && !isGrounded && currentJumpAmount < jumpAmount)
            {
                jumpKeyDown = true;
                onAirJump.Invoke();
            }

            if (context.performed && isGrounded)
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
                    isCrouching = !isCrouching;
                }
            }
            else
            {
                if (context.performed && isGrounded)
                {
                    isCrouching = true;
                }

                if (context.canceled)
                {
                    isCrouching = false;

                }
            }
        }
        #endregion

        private void OnDrawGizmosSelected()
        {
            if (groundPoint != null)
            {
                Gizmos.color = Color.green;
                switch (groundCheckType)
                {
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
        }
    }
}

