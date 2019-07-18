using System.Collections;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class TombiCharacterController : SerializableMonoBehaviour
{
    public static TombiCharacterController instance;

    public override string FileExtension { get => ".smtc"; protected set => base.FileExtension = value; }
    public override string FileExtensionName { get => "SMTC"; protected set => base.FileExtensionName = value; }

    #region CUSTOM CLASSES
    [System.Serializable]
    public class LGWCController
    {
        //[ShowOnly]
        public bool canAerialSnap = false;

        //[ShowOnly]
        public bool canWallSnap = false;

        //[ShowOnly]
        public bool isWallSnapping = false;

        //[ShowOnly]
        public bool hasWallSnapped = false;

        //[ShowOnly]
        public bool isClimbing = false;

        //[ShowOnly]
        public bool canLedgeClimb = false;

        //[ShowOnly]
        public bool isLedgeClimbing = false;

        //[ShowOnly]
        public bool canLedgeSnap = false;

        //[ShowOnly]
        public bool isLedgeSnapping = false;

        //[ShowOnly]
        public bool hasLedgeSnapped = false;

        [EditorRename("Enable Contact Wall Snap")]
        public bool contactWallSnap = true;
        public float rayDistanceFromPlayerPosition = 0.3f;
        public float snapLerpFactor = 4f;

        public float ledgeClimbProgress = 0f;
        public float ledgeYPosAdjustment = 0f;
        public float pushBackMultiplierOnJumpOff = 1f;
        public float pushUpMultiplierOnJumpOff = 1f;
        public float distanceFromWall = 0.04f;
        public float distanceFromWallWhenLedgeSnapped = 0.04f;
        public float distanceFromWallWhenLedgeSnappedSlope = 0.3f;

        public float distanceFromLedgeZ = 0.04f;
        public float distanceFromLedgeY = 0.04f;
        public float climbUpSpeed = 1.2f;
        public float climbFallSpeed = 2f;
        public LayerMask climbableSurfaces;
        public BezierSpline ledgeClimbSpline;

        [HideInInspector]
        public Vector3[] climbDetectionRays = new Vector3[5];

        [HideInInspector]
        public Vector3[] ledgeDetectionRays = new Vector3[2];

        [HideInInspector]
        public Vector3[] ledgeDetectionRaysClimbOffset = new Vector3[2];

        [HideInInspector]
        public Vector3[] climbDetectionRaysHits = new Vector3[5];

        [Header("Wall Snap Settings")]
        public float wallSnapRayDistance = 0.2f;
        public float hasWallSnappedThresholdDistance = 0.1f;
        public float hasWallSnappedResetThresholdDistance = 0.15f;

        [Space(5)]
        public float climbDetectionRayUpOffset = 0.3f;
        public float climbDetectionRayDownOffset = 0.2f;
        public float climbDetectionRayRightOffset = 0.2f;
        public float climbDetectionRayLeftOffset = 0.2f;
        public float climbDetectionRayOverheadOffset = 0.1f;

        [Header("Ledge Snap Settings")]
        public float ledgeSnapRayDistance = 0.2f;

        [EditorRename("Has Ledge Snapped Threshold Distance")]
        public float hasLedgeSnappedThresholdDistance = 0.1f;

        [EditorRename("Has Ledge Snapped Threshold Distance (Reset)")]
        public float hasLedgeSnappedResetThresholdDistance = 26.2f;

        [Space(5)]
        public float ledgeSnapFirstRayOffsetRight = 0.2f;
        public float ledgeSnapFirstRayOffsetUp = 0.85f;
        public float ledgeSnapFirstRayOffsetForward = 0.2f;
        public float ledgeSnapSecondRayOffsetRight = 0.2f;
        public float ledgeSnapSecondRayOffsetUp = 0.85f;
        public float ledgeSnapSecondRayOffsetForward = 0.2f;
        public float ledgeDetectionRayClimbRayUpOffset = 0.2f;
        [Header("Wall Climb Settings")]


        [Header("Ledge Climb Settings")]
        public float canLedgeClimbDistance = 0.4f;
        public float ledgeClimbSlerpFactor = 20f;

        public LGWCController()
        {
            //ledgeClimbSpline = new BezierSpline();
        }

        public void ResetFlags()
        {
            canWallSnap = false;
            canAerialSnap = false;
            isWallSnapping = false;
            isClimbing = false;
        }
    }

    [System.Serializable]
    public class PlayerStateController
    {
        [ShowOnly] public bool isJumping = false;
        [ShowOnly] public bool isGrounded;
        [ShowOnly] public bool isFalling;
        [ShowOnly] public bool isStrafing = false;
        [ShowOnly] public bool isRolling = false;
        [ShowOnly] public bool isDead = false;
        [ShowOnly] public bool isBlocking = false;
        [ShowOnly] public bool isKnockback;
        [ShowOnly] public bool canAction = true;
        [ShowOnly] public bool canMove = true;
        [ShowOnly] public bool canJump;
        [ShowOnly] public bool onBridge = false;
        [ShowOnly] public bool onPumpRock = false;
    }

    [System.Serializable]
    public class MovementSettings
    {
        [Header("General Settings")]
        public bool lockZMovement = true;
        public bool moveOnly = false;

        [Header("Speed Settings")]
        public float jumpSpeed = 5;
        public float wallJumpSpeed = 5f;
        public float swingJumpSpeed = 5f;
        public float runSpeed = 3f;
        public float animalDashSpeed = 5f;
        public float inAirSpeed = 0.6f;
        public float walkSpeed = 2f;
        public float rotationSpeed = 40f;

        [Header("Particles")]
        public GameObject animalDashParticles;

    }

    [System.Serializable]
    public class PhysicsSettings
    {
        public LayerMask collidingMask;

        [Header("Gravity Settings")]
        public float groundGravity = -9.8f;
        public float jumpingGravity = -9.8f;
        public float bridgeGravity = -9.8f;

        [Header("Rigidbody Mass Settings")]
        public float onBridgeMass = 0f;
        public float onGroundMass = 5f;

        [Header("Ground Check Settings")]
        [EditorRename("Rigidbody Y velocity threshold")]
        public float rigidBodyThresholdYVel = -1f;
        public float maxGroundedRadius = 0.2f - 0.0075f;
        public Vector3 maxGroundedHeight;
        public Vector3 maxGroundedDistanceDown = Vector3.down * 0.2f;
    }

    [System.Serializable]
    public class WeaponController
    {
        public GameObject equippedWeapon;
    }

    [System.Serializable]
    public class BaseGrabController
    {
        [Header("Grab Flags")]
        [ShowOnly] public bool canGrab = false;
        [ShowOnly] public bool isGrabbing = false;
        [ShowOnly] public bool hasGrabbed = false;

        public GameObject grabbedObj = null;

        public float onGrabColliderHeightOffset = 0f;
        public Vector3 onGrabColliderOffset = Vector3.zero;

        [Header("Grab Tags List")]
        public System.Collections.Generic.List<string> grabbableTags;

    }

    [System.Serializable]
    public class ObjectGrabController : BaseGrabController
    {
        [Header("Throw Flags")]
        [ShowOnly] public bool waitForThrow = false;
        [ShowOnly] public bool isThrowing = false;

        public void PlayObjectGrabSound(GrabObject.eType type)
        {
            FindObjectOfType<AudioManager>().Play("Land_Grab");

            switch (type)
            {
                case GrabObject.eType.Chest:
                    FindObjectOfType<AudioManager>().Play("Chest_Grab");
                    break;

                case GrabObject.eType.Pig:
                    FindObjectOfType<AudioManager>().Play("Pig_Grab");
                    break;

                case GrabObject.eType.Bird:
                    break;

                default:
                    break;
            }
        }
    }

    [System.Serializable]
    public class EnvironmentGrabController : BaseGrabController
    {

    }
    #endregion

    #region Variables

    /* COMPONENTS */
    [HideInInspector] public AudioManager audioManager;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody rb;

    /* TOMBA SETTINGS */
    public MovementSettings movementSettings = new MovementSettings();
    public PhysicsSettings physicsSettings = new PhysicsSettings();

    /* TOMBA CONTROLLERS */
    public LGWCController climbController = new LGWCController();
    public PlayerStateController stateController = new PlayerStateController();
    public WeaponController weaponController = new WeaponController();
    public ObjectGrabController objGrabController = new ObjectGrabController();
    public EnvironmentGrabController envGrabController = new EnvironmentGrabController();

    /* IDLE VARIABLES */
    private float idleTime = 5f;
    private float currentIdleTime = 0f;

    /* MOVEMENT VARIABLES */
    private float moveSpeed;
    private float x;
    private float z;
    private float dv;
    private float dh;
    private float inputHorizontal;
    private Vector3 newVelocity;

    [HideInInspector] public float lastInputHorizontal;
    [HideInInspector] public Vector3 inputVec;
    [HideInInspector] public Vector3 railFirstPoint;
    [HideInInspector] public Vector3 railLastPoint;
    [HideInInspector] public Vector3 playerRail;
    [HideInInspector] public Vector3 climbPosition;
    [HideInInspector] public Vector3 ledgeClimbPosition;
    [HideInInspector] public Vector3 lastInputVec;
    [HideInInspector] public Vector3 lastCompleteInputVector;
    [HideInInspector] public Vector3 completeInputVector;

    /* SLIDING VARIABLES */
    [ShowOnly] public bool canCheckForSlopes = false;
    [ShowOnly] public bool isSliding;
    [ShowOnly] public bool startedSliding = false;
    [ShowOnly] public bool waitingForSlopeEnd = false;
    [ShowOnly] public float slopeAngle = 0f;
    [HideInInspector] public float slidingTime = 1f;
    [HideInInspector] public float slidingTimeElapsed = 0f;
    [HideInInspector] public Vector3 groundSlopeNormal;

    [HideInInspector] public bool onSwitchCollider = false;
    [HideInInspector] public float knockbackMultiplier = 1f;

    public Camera sceneCamera;
    public GameObject stomachRigJoint;
    public float ledgepos = 0.6f;
    public float ledgeposX = 0.6f;
    public float wallSnapAngleError = 0f;

    private float distanceFromWall;
    private bool startFall;
    private bool hasLedgeClimbTransitioned = false;
    private bool oldTriggerHeld = false;
    private Vector3 targetDashDirection;
    private GameObject ledgeSnappedObj;
    private RaycastHit hitLedgeClimb;
    private AdvancedUtilities.Cameras.BasicCameraController camScript;
    private Vector3 ledgeclimbhitNormal = Vector3.zero;
    public PumpRock lastHittedPump;
    private Vector3 groundNormal = Vector3.zero;
    private Vector3 groundHit = Vector3.zero;

    private const float fallingVelocity = -0.1f;
    private const float checkRaycastGroundedOffset = 0.75f;
    // Used for continuing momentum while in air
    private const float maxVelocity = 5f;
    private const float minVelocity = -5f;

    private bool hasInitialized = false;

    #endregion


    bool Initialize()
    {
        // Get the animator component
        animator = GetComponentInChildren<Animator>();

        // Get the audiomanager component
        audioManager = FindObjectOfType<AudioManager>();

        // Get the rigidbody component
        rb = GetComponent<Rigidbody>();

        // Get the camera controller component
        camScript = FindObjectOfType<AdvancedUtilities.Cameras.BasicCameraController>();

        /* 
         * Check if audiomanager and camera script were found
         * Animator and RigidBody are required components. 
         */
        if(audioManager == null)
        {
            Debug.LogWarning("'AudioManager' script was not found by 'TombiCharacterController'.");
            return false;
        }

        if (audioManager == null)
        {
            Debug.LogError("'BasicCameraController' script was not found by 'TombiCharacterController'.");
            return false;
        }

        // Check if animator has controller and avatar
        if (animator.avatar == null)
        {
            Debug.LogError("'Animator' component of 'TombiCharacterController' has no 'Avatar' attached to it.");
            return false;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("'Animator' component of 'TombiCharacterController' has no 'Controller' attached to it.");
            return false;
        }

        // Check if weapon has been attached
        if (weaponController.equippedWeapon == null)
        {
            Debug.LogError("'Weapon' variable of 'TombiCharacterController' has no object attached to it.");
            return false;
        }
        else
        {
            weaponController.equippedWeapon.SetActive(false);
        }

        //Initialize climb and ledge detection rays
        climbController.climbDetectionRays = new Vector3[5];
        climbController.ledgeDetectionRays = new Vector3[2];

        //Force last input to RIGHT
        lastInputHorizontal = 1;

        if(stomachRigJoint != null && movementSettings.animalDashParticles != null)
        {
            movementSettings.animalDashParticles.transform.parent = stomachRigJoint.transform;
        }

        return true;
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        hasInitialized = Initialize();

        if(!hasInitialized)
        {
            Debug.LogError("TombiCharacterController script wasn't initialized properly, the script will not execute any function.");
        }
        else
        {
            Debug.Log("TombiCharacterController script has been initialized.");
        }
    }

    void Update()
    {
        //Make sure that the script has been initialized properly.
        if (hasInitialized)
        {
            if (movementSettings.lockZMovement)
            {
                CheckForGrabbableObjects();

                if (GetRaycastBottomClearance(physicsSettings.collidingMask, new Vector3(0f, -0.6f, 0f)))
                {
                    if (objGrabController.hasGrabbed && objGrabController.waitForThrow && !stateController.isJumping && !objGrabController.isThrowing)
                    {
                        ThrowGrabbedObject();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.R) && !movementSettings.moveOnly)
            {
                if (!weaponController.equippedWeapon.activeSelf && (stateController.isGrounded || stateController.isFalling || stateController.isJumping) && !climbController.hasLedgeSnapped && !climbController.hasWallSnapped && !climbController.isLedgeClimbing)
                {
                    weaponController.equippedWeapon.GetComponent<Blackjack>().Throw();
                }
            }

            if (stateController.onBridge)
            {
                rb.mass = physicsSettings.onBridgeMass;
            }
            else
            {
                rb.mass = physicsSettings.onGroundMass;
            }

            Jumping();
            HandleIdle();
            HandlePumpRockPress();

            if (!movementSettings.moveOnly) //Check if Tomba can perform actions
            {
                if (stateController.isJumping || stateController.isFalling || !stateController.isGrounded)
                {
                    climbController.canAerialSnap = true;
                }
                else
                {
                    climbController.ResetFlags();
                }

                climbController.canWallSnap = CheckForClimbableSurface(2f);
                climbController.canLedgeSnap = CheckLedgeSurface() && !stateController.isGrounded;

                if (!climbController.canLedgeSnap)
                {
                    climbController.hasLedgeSnapped = false;
                }
                if (!climbController.canLedgeSnap && climbController.hasWallSnapped && !climbController.hasLedgeSnapped && !climbController.isLedgeClimbing && !climbController.canLedgeClimb)
                {
                    AdjustWallDistance(climbController.distanceFromWall);
                    AdjustRotationWhenSnapped();
                }

                if (climbController.ledgeClimbProgress > 0 && !climbController.isLedgeClimbing)
                {
                    climbController.ledgeClimbProgress = 0f;
                }

                if (!climbController.isLedgeClimbing)
                {
                    climbController.ledgeClimbSpline.points[0] = climbController.ledgeClimbSpline.transform.InverseTransformPoint(this.transform.position);

                    if (lastInputHorizontal == 1)
                    {
                        climbController.ledgeClimbSpline.points[1] = new Vector3(climbController.ledgeClimbSpline.points[0].x - 1f, climbController.ledgeClimbSpline.points[0].y + 2f, climbController.ledgeClimbSpline.points[0].z);
                    }
                    else if (lastInputHorizontal == -1)
                    {
                        climbController.ledgeClimbSpline.points[1] = new Vector3(climbController.ledgeClimbSpline.points[0].x + 1f, climbController.ledgeClimbSpline.points[0].y + 2f, climbController.ledgeClimbSpline.points[0].z);
                    }

                    if (ledgeClimbPosition != null && ledgeClimbPosition != Vector3.zero)
                    {
                        climbController.ledgeClimbSpline.points[3] = climbController.ledgeClimbSpline.transform.InverseTransformPoint(ledgeClimbPosition);
                    }
                    else
                    {
                        climbController.ledgeClimbSpline.points[3] = climbController.ledgeClimbSpline.transform.InverseTransformPoint(climbController.climbDetectionRays[0] + (transform.forward * 0.2f));
                    }
                    climbController.ledgeClimbSpline.points[2] = new Vector3(climbController.ledgeClimbSpline.points[3].x, climbController.ledgeClimbSpline.points[3].y + 0.3f, climbController.ledgeClimbSpline.points[3].z);
                    climbController.ledgeClimbSpline.transform.position = this.transform.position;

                }

                if ((climbController.isWallSnapping || climbController.hasWallSnapped) && !climbController.isLedgeClimbing && !stateController.isJumping && !stateController.isFalling && !climbController.canLedgeClimb && !stateController.isGrounded)
                {
                    AdjustRotationWhenSnapped();
                    AdjustWallDistance(climbController.distanceFromWall);
                }

                if (climbController.canWallSnap && !climbController.hasWallSnapped && !climbController.isLedgeClimbing && !climbController.hasLedgeSnapped && !climbController.isLedgeSnapping)
                {
                    WallSnap();
                }
                else
                {
                    ResetWallSnap();
                }
            }

            if (!stateController.isBlocking && !stateController.isDead && !climbController.isWallSnapping && !climbController.hasWallSnapped && !climbController.hasLedgeSnapped && !climbController.isLedgeSnapping && !climbController.isLedgeClimbing)
            {
                CameraRelativeMovement();
            }

            if (climbController.hasWallSnapped && !climbController.canLedgeClimb)
            {
                WallRelativeMovement();
            }

            LedgeClimb();

            if (climbController.canLedgeClimb && !climbController.isLedgeClimbing)
            {
                //animator.SetTrigger("LedgeSnapTrg");

                this.GetComponent<IKHandler>().isSwinging = true;
                this.GetComponent<IKHandler>().HandPositionOnSwingL = new Vector3(ledgeSnappedObj.transform.position.x + 0.01f * lastInputHorizontal, ledgeSnappedObj.transform.position.y, transform.position.z + 0.2f * lastInputHorizontal);
                this.GetComponent<IKHandler>().HandPositionOnSwingR = new Vector3(ledgeSnappedObj.transform.position.x + 0.01f * lastInputHorizontal, ledgeSnappedObj.transform.position.y, transform.position.z - 0.2f * lastInputHorizontal);

                this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(this.transform.position.x, ledgeSnappedObj.transform.position.y - ledgepos, transform.position.z), Time.deltaTime * 6f);
                this.transform.position = Vector3.Slerp(this.transform.position, new Vector3(ledgeSnappedObj.transform.position.x - ledgeposX * lastInputHorizontal, this.transform.position.y, this.transform.position.z), Time.deltaTime * 10f);

            }
            else
            {
                animator.ResetTrigger("LedgeSnapTrg");
                this.GetComponent<IKHandler>().isSwinging = false;
            }
        }
    }

    void FixedUpdate()
    {
        //Make sure that the script has been initialized properly.
        if (hasInitialized)
        {
            if (stateController.isJumping && !stateController.isFalling && Input.GetButton("PS4_X"))
            {
                rb.AddForce(new Vector3(0f, 50f * Time.deltaTime, 0f), ForceMode.Impulse);

            }
            SlopeSlide();

            if (stateController.onPumpRock && !stateController.isJumping)
            {
                //Instantly check for grounded state so that grounded flag updates in this frame shard.
                if (Vector3.Distance(this.transform.position, new Vector3(this.transform.position.x, lastHittedPump.topObject.transform.position.y, this.transform.position.z)) <= 0.05)
                {
                    CheckForGrounded(0.5f);
                }

                //Stick Tomba to the pump rock.
                rb.AddForce(0, -100, 0, ForceMode.Force);

                if (!lastHittedPump.pressed)
                {
                    this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(this.transform.position.x, lastHittedPump.topObject.transform.position.y, this.transform.position.z), Time.deltaTime * 2f);
                }

            }
            else
            {
                if (!climbController.hasWallSnapped && !climbController.isLedgeSnapping && !climbController.hasLedgeSnapped)
                {
                    HandlePlayerGravity();

                    CheckForGrounded(0.5f);
                }
            }

            if (climbController.hasWallSnapped || climbController.isWallSnapping || climbController.isLedgeClimbing || climbController.isLedgeSnapping || climbController.hasLedgeSnapped)
            {
                //RaycastHit hit;

                ////Da cambiare
                //if (Physics.Raycast(climbController.climbDetectionRays[4], transform.forward, out hit, 2f, climbController.climbableSurfaces))
                //{
                //    distanceFromWall = hit.distance;

                //    if(distanceFromWall < 0.06f)
                //    {
                //        transform.position = transform.position + (-transform.forward * distanceFromWall);
                //    }
                //}
            }

            else
            {
                AirControl();

                //check if character can move
                if (stateController.canMove && !stateController.isBlocking && !stateController.isDead)
                {
                    moveSpeed = UpdateMovement();
                }

                //check if falling
                if (rb.velocity.y < fallingVelocity)
                {
                    if (Physics.Raycast(this.transform.position - (this.transform.up * 0.2f), -this.transform.up, 0.25f, physicsSettings.collidingMask) || stateController.onPumpRock)
                    {
                        stateController.isFalling = false;
                    }
                    else
                    {
                        if (!stateController.isGrounded && !isSliding)
                        {
                            stateController.isFalling = true;

                            animator.applyRootMotion = false;

                            if (stateController.canJump)
                            {
                                stateController.canJump = false;
                            }
                        }
                        else
                        {
                            stateController.isFalling = false;
                        }
                    }
                }
                else
                {
                    stateController.isFalling = false;
                }
            }

            /* 
             * Before next actions are performed
             * snap back the position of Tomba avoiding him to loose
             * the accurate rail positioning 
             */
            SnapZPosition();
        }
    }

    void LateUpdate()
    {
        //Make sure that the script has been initialized properly.
        if (hasInitialized)
        {
            //Get local velocity of charcter
            float velocityXel = transform.InverseTransformDirection(rb.velocity).x * 5f;
            float velocityZel = transform.InverseTransformDirection(rb.velocity).z * 1f;

            //Update animator with movement values
            animator.SetFloat("Velocity X", velocityXel / movementSettings.runSpeed, 1f, Time.deltaTime * 5f);

            if (!Input.GetKey(KeyCode.LeftControl))
            {
                animator.SetFloat("Velocity Z", velocityZel / movementSettings.runSpeed, 1f, Time.deltaTime * 5f);
            }
            else
            {
                animator.SetFloat("Velocity Z", (velocityZel / movementSettings.runSpeed) + 1, 1f, Time.deltaTime * 5f);
            }
            //if character is alive and can move, set our animator
            if (!stateController.isDead && stateController.canMove)
            {
                if (moveSpeed > 0)
                {
                    animator.SetBool("Moving", true);
                }
                else
                {
                    animator.SetBool("Moving", false);
                }
            }

            if (lastInputHorizontal == 1)
            {
                animator.SetBool("FacingLeft", false);

            }
            else if (lastInputHorizontal == -1)
            {
                animator.SetBool("FacingLeft", true);
            }
        }
    }

    private void HandlePlayerGravity()
    {
        if (stateController.isJumping || stateController.isFalling || !stateController.isGrounded && !objGrabController.isThrowing) //Tomba is jumping, change gravity to make him fly a little
        {
            rb.useGravity = true;
            rb.AddForce(0, physicsSettings.jumpingGravity, 0, ForceMode.Acceleration); 
        }
        else if (!isSliding && !stateController.onBridge && !objGrabController.isThrowing) //Tomba is on ground, use grounded gravity
        {
            rb.useGravity = true;
            rb.AddForce(0, physicsSettings.groundGravity, 0, ForceMode.Acceleration);
        }
        else if (!isSliding && stateController.onBridge && !objGrabController.isThrowing) //Tomba is on bridge, use custom gravity
        {
            rb.useGravity = true;
            rb.AddForce(0, physicsSettings.bridgeGravity, 0, ForceMode.Acceleration);
        }
        else if (objGrabController.isThrowing) //Tomba is throwing object, freeze it's position while throwing
        {
            rb.useGravity = false;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
    }

    private void HandlePumpRockPress()
    {
        RaycastHit hittone;

        if (GetRaycastBottomClearance(physicsSettings.collidingMask, new Vector3(0,-0.5f,0), out hittone))
        {
            if (hittone.transform.tag == "PumpRock" && !stateController.isJumping)
            {
                lastHittedPump = hittone.transform.GetComponent<PumpRock>();
                Debug.Log(hittone.distance);
                if (hittone.distance < 1.2f)
                {
                    hittone.transform.GetComponent<PumpRock>().canCollapse = true;
                }

                else if (hittone.distance < 1.2f)
                {
                    if (!stateController.isJumping)
                    {
                        stateController.onPumpRock = true;
                        CheckForGrounded(1f);
                    }
                    else
                    {
                        stateController.onPumpRock = false;
                    }
                }
            }
            else
            {
                if (lastHittedPump != null)
                {
                    lastHittedPump.cancollapse2 = false;
                    stateController.onPumpRock = false;
                }
            }
        }
    }

    private void HandleIdle()
    {
        if (Time.time - currentIdleTime >= idleTime)
        {
            animator.SetBool("Idling", true);
        }
        else
        {
            animator.SetBool("Idling", false);
        }
    }

    private void HandleGrabObjectCollision()
    {
        /* 
           Make sure that in the frame shard when the collision occurs 
           we are still over a grabbable object with the main collider
           and if so retrieve the hit informations.
        */
        RaycastHit hit;
            
        CheckForGrabbableObjects(out hit);

        //Check if updated variable is true
        if (objGrabController.canGrab)
        {
            //Filter bouncing collisions by using animation state and grabFlag
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Land_Grab") && !objGrabController.hasGrabbed)
            {
                if (hit.transform.gameObject != null)
                {
                    objGrabController.hasGrabbed = true;
                    objGrabController.grabbedObj = hit.transform.gameObject;
                    animator.SetBool("hasGrabbedObject", objGrabController.hasGrabbed);

                    //Plays vertical grab global sound and then picks the object-specific sound to play.
                    objGrabController.PlayObjectGrabSound(objGrabController.grabbedObj.transform.GetComponent<GrabObject>().type);

                    if (objGrabController.grabbedObj.transform.GetComponent<GrabObject>().movesWithPlayer)
                    {
                        objGrabController.grabbedObj.transform.GetComponent<Rigidbody>().isKinematic = true;
                        objGrabController.grabbedObj.transform.GetComponent<Rigidbody>().useGravity = false;

                        objGrabController.grabbedObj.transform.GetComponent<Collider>().enabled = false;
                        objGrabController.grabbedObj.transform.parent = stomachRigJoint.transform;
                        objGrabController.grabbedObj.transform.localPosition = new Vector3(0f,0.002f,0.0031f);
                        objGrabController.grabbedObj.transform.localRotation = Quaternion.Euler(0f, 90f, -20f);

                        GameObject obj = Resources.Load("Prefabs/PigGrabPoof_2", typeof(GameObject)) as GameObject;

                        GameObject newObj = Instantiate(obj, objGrabController.grabbedObj.transform.position, Quaternion.identity);

                    }
                }
            }
        }
        animator.SetTrigger("LandGrabTrg");
    }

    public void ThrowGrabbedObject()
    {
        objGrabController.waitForThrow = false;
        objGrabController.isThrowing = true;

        animator.SetTrigger("ThrowObjectTrg");
        audioManager.Play("Throw_Object");
        StartCoroutine(_ThrowObject());
    }

    IEnumerator _ThrowObject()
    {
        while (objGrabController.isThrowing)
        {
            yield return new WaitForSeconds(0.1f);

            if(objGrabController.hasGrabbed)
            {
                objGrabController.grabbedObj.transform.position = new Vector3(objGrabController.grabbedObj.transform.position.x, this.transform.position.y + 0.4f, objGrabController.grabbedObj.transform.position.z);
                objGrabController.grabbedObj.transform.parent = null;
                objGrabController.hasGrabbed = false;

                objGrabController.grabbedObj.layer = LayerMask.NameToLayer("NoPlayerCollision");
                objGrabController.grabbedObj.GetComponent<Collider>().enabled = true;
                objGrabController.grabbedObj.GetComponent<Rigidbody>().useGravity = false;
                objGrabController.grabbedObj.GetComponent<Rigidbody>().isKinematic = false;
                objGrabController.grabbedObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
                objGrabController.grabbedObj.GetComponent<Rigidbody>().AddForce(this.transform.forward.normalized * 5f, ForceMode.Impulse);
                objGrabController.grabbedObj.GetComponent<Rigidbody>().AddTorque(this.transform.right.normalized * 10f, ForceMode.Impulse);
                objGrabController.grabbedObj.GetComponent<GrabObject>().throwDirection = lastInputHorizontal;
                objGrabController.grabbedObj.GetComponent<GrabObject>().hasBeenThrown = true;

            }
            animator.SetBool("isFallingAfterGrab", true);

            rb.isKinematic = false;
            objGrabController.isThrowing = false;
            animator.SetBool("hasGrabbedObject", false);
            objGrabController.grabbedObj = null;
        }
    }

    public void ResetIdle()
    {
        currentIdleTime = Time.time;
    }

    public void SlopeSlide()
    {
        RaycastHit hit;
        Vector3 slopeParallel = Vector3.zero;
        float currentSlope;

        // Raycast with infinite distance to check the slope directly under Tomba no matter where he is
        if (Physics.Raycast(this.transform.position, Vector3.down, out hit, 0.3f) && canCheckForSlopes && !objGrabController.hasGrabbed)
        {
            // Saving the normal
            Vector3 n = hit.normal;
            groundSlopeNormal = n;

            // Crossing normal with the Tomba's up vector
            Vector3 groundParallel = Vector3.Cross(transform.up, n);

            // Crossing the vector made before with the initial normal gives a vector that is parallel to the slope and always pointing down
            slopeParallel = Vector3.Cross(groundParallel, n);
            Debug.DrawRay(hit.point, slopeParallel * 10, Color.green);

            // Just the current angle Tomba's standing on
            currentSlope = Mathf.Round(Vector3.Angle(hit.normal, transform.up));
            slopeAngle = currentSlope;

            if (currentSlope >= 20f && !Physics.Raycast(this.transform.position, this.transform.forward, 0.3f, physicsSettings.collidingMask) && stateController.isGrounded)// && !stateController.isGrounded)
            {
                Vector3 hVel = rb.velocity; // new Vector3(rb.velocity.x, 0, rb.velocity.z);

                if (hVel.magnitude > 2f)
                {
                    if (!stateController.isJumping && !stateController.onPumpRock)
                    {
                        if (!isSliding)
                        {
                            animator.SetTrigger("SlideTrg");
                            isSliding = true;

                            startedSliding = true;
                            slidingTimeElapsed = Time.time;
                        }

                        if (isSliding)
                        {
                            rb.useGravity = false;
                            float slideSpeed = currentSlope;
                            this.transform.position = hit.point;

                            if (currentSlope < 45)
                            {
                                if (hVel.magnitude < 3f)
                                {
                                    //rb.AddForce(-n * 15f * Time.deltaTime, ForceMode.Impulse);

                                    //Add slope slide force
                                    rb.AddForce(Vector3.down * (currentSlope * Time.deltaTime * 1.2f), ForceMode.VelocityChange);
                                }
                            }

                            else
                            {
                                rb.velocity = rb.velocity * 0.2f;

                                Debug.Log(currentSlope);
                            }
                        }
                    }
                    else
                    {
                        animator.ResetTrigger("SlideTrg");
                        isSliding = false;
                    }
                }
                else
                {
                    ResetSlopeSlide();
                }
            }

            // If the player is standing on a slope that isn't too steep, is grounded, as is not sliding anymore we start a function to count time
            else if (currentSlope < 20f && stateController.isGrounded && isSliding)
            {
                ResetSlopeSlide();
            }
            else
            {
                ResetSlopeSlide();
            }
        }
        else
        {
            isSliding = false;
        }

        if (!isSliding)
        {
            animator.ResetTrigger("SlideTrg");
            waitingForSlopeEnd = false;
            slidingTimeElapsed = 0;
        }

        animator.SetBool("IsSliding", isSliding);

        if (waitingForSlopeEnd)
        {
            rb.AddForce(new Vector3(0f, -2f, 0f) + lastInputVec, ForceMode.VelocityChange);
        }
    }

    public void ResetSlopeSlide()
    {
        if (isSliding)
        {
            waitingForSlopeEnd = true;

        }
        if (isSliding && (Time.time - slidingTimeElapsed) >= slidingTime)
        {
            waitingForSlopeEnd = false;

            isSliding = false;
            slidingTimeElapsed = 0f;
        }
    }

    public void LedgeGrab(GameObject ledgeSnapped)
    {
        //if ((!animator.GetCurrentAnimatorStateInfo(0).IsName("Fall-Ledge-Grab") || 
        //            !animator.GetCurrentAnimatorStateInfo(0).IsName("Ledge_Snap") || 
        //            !animator.GetCurrentAnimatorStateInfo(0).IsName("Ledge_Hold")) && !climbController.isLedgeClimbing)
        //{
        //}
        ResetIdle();

        if (!climbController.canLedgeClimb)
        {
            ledgeSnappedObj = ledgeSnapped;
            climbController.canLedgeClimb = true;
        }

        if (climbController.canLedgeClimb && !climbController.isLedgeClimbing)
        {
            if (Physics.Linecast(climbController.climbDetectionRays[0] + (transform.forward * 0.3f) + (transform.up * (climbController.climbDetectionRayUpOffset + 0.1f)), climbController.climbDetectionRays[0] + (transform.forward * 0.3f) + (-transform.up * climbController.climbDetectionRayUpOffset), out hitLedgeClimb,physicsSettings.collidingMask))
            {
                if (hitLedgeClimb.distance < 6f)
                {
                    Vector3 ledgeClimbPositionWithOffset = new Vector3(hitLedgeClimb.point.x, hitLedgeClimb.point.y, hitLedgeClimb.point.z);
                    RaycastHit hit2;


                    if ((!Physics.Raycast(hitLedgeClimb.point, hitLedgeClimb.normal, out hit2, 1f, physicsSettings.collidingMask)))
                    {
                        ledgeClimbPosition = hitLedgeClimb.point;

                    }
                    else
                    {
                        RaycastHit hit3;
                        if ((Physics.Raycast(hitLedgeClimb.point - hitLedgeClimb.normal, hitLedgeClimb.normal, out hit3, 2f, physicsSettings.collidingMask)))
                        {
                            ledgeClimbPosition = hit3.point;
                        }
                    }
                    transform.rotation = Quaternion.Euler(new Vector3(0, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z));

                    //RotateTowardsMovementDir(lastInputVec, true);

                    rb.isKinematic = true;
                    climbController.isLedgeSnapping = true;
                    stateController.isGrounded = false;
                    stateController.isFalling = false;

                    if (!climbController.hasLedgeSnapped && !climbController.canWallSnap)
                    {
                        climbController.hasLedgeSnapped = true;
                        audioManager.Play("Snap");
                        animator.SetTrigger("LedgeSnapTrg");
                        animator.SetBool("HasLedgeSnapped", true);

                    }
                }
            }
        }
    }

    public void LedgeGrabReset()
    {
        climbController.canLedgeClimb = false;
        animator.SetBool("HasLedgeSnapped", false);
        animator.ResetTrigger("LedgeSnapTrg");

    }

    public void WallSnap()
    {
        if (!stateController.isGrounded)
        {
            RaycastHit hit;

            bool snap = false;

            if (Physics.Raycast(transform.position, transform.forward, out hit, climbController.wallSnapRayDistance, climbController.climbableSurfaces))
            {
                if (climbController.contactWallSnap)
                {
                    snap = true;
                }
                else
                {
                    if (Input.GetButton("Horizontal"))
                    {
                        snap = true;
                    }
                    else
                    {
                        snap = false;
                    }
                }

                if (snap)
                {
                    climbPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z) - transform.forward * climbController.distanceFromWall;
                    rb.isKinematic = true;
                    climbController.isWallSnapping = true;
                    stateController.isJumping = false;
                    animator.SetBool("Wall_Climbing", true);
                    animator.SetTrigger("WallClimbTrg");
                    stateController.isFalling = false;
                }
            }

            if (climbController.isWallSnapping)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, climbPosition, Time.deltaTime * climbController.snapLerpFactor);
                AdjustRotationWhenSnapped();

                if (Vector3.Distance(this.transform.position, climbPosition) < climbController.hasWallSnappedThresholdDistance)
                {
                    climbController.isWallSnapping = false;
                    climbController.hasWallSnapped = true;
                    climbPosition = Vector3.zero;
                    audioManager.Play("Snap");
                }
            }
        }
    }

    public void ResetWallSnap()
    {
        if (!Physics.Raycast(transform.position + (transform.up * climbController.rayDistanceFromPlayerPosition), transform.forward, climbController.hasWallSnappedResetThresholdDistance, climbController.climbableSurfaces))
        {
            climbController.hasWallSnapped = false;
            if (rb.isKinematic && !climbController.isLedgeSnapping && !climbController.isLedgeClimbing && !climbController.hasLedgeSnapped && !objGrabController.isThrowing)
            {
                rb.isKinematic = false;
            }
            animator.SetBool("Wall_Climbing", false);
            animator.ResetTrigger("WallClimbTrg");
        }
    }

    #region Positioning Helpers

    public void AdjustRotationWhenSnapped()
    {
        wallSnapAngleError = Vector3.SignedAngle(this.transform.up, climbController.climbDetectionRaysHits[4] - climbController.climbDetectionRaysHits[1], this.transform.right);

        if (wallSnapAngleError > 0.1 || wallSnapAngleError < -0.1)
        {
            transform.Rotate(wallSnapAngleError, 0, 0);
        }
    }

    public void AdjustWallDistance(float distanceFromWall)
    {
        RaycastHit hit;
        Vector3 pos = Vector3.zero;

        if (Physics.Raycast(transform.position - transform.forward, transform.forward, out hit, 4f, climbController.climbableSurfaces))
        {
            pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) - (transform.forward * distanceFromWall);
        }
        else
        {
            pos = Vector3.zero;
        }

        if (pos != Vector3.zero)
        {
            this.transform.position = pos;//Vector3.Lerp(this.transform.position, pos, Time.deltaTime * 10f);

            //if (climbController.hasLedgeSnapped)
            //{
            //    if (yOffset > 0.001f)
            //    {
            //        this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position - (transform.up * yOffset), Time.deltaTime * 15f);

            //    }
            //}
        }

        //if(yOffset > 0)
        //{
        //    this.transform.position = this.transform.position - (this.transform.up * yOffset);
        //}
    }

    /// <summary>
    /// Function used to keep Tomba snapped to the railing system.
    /// </summary>
    public void SnapZPosition()
    {
        if (railFirstPoint != null && railLastPoint != null && movementSettings.lockZMovement)
        {
            //Calculate projection between rail points
            Vector3 AB = railLastPoint - railFirstPoint;
            Vector3 AC = rb.position - railFirstPoint;
            Vector3 AX = Vector3.Project(AC, AB);
            Vector3 X = AX + railFirstPoint;

            //Instantly set Tomba position to new calculated one
            transform.position = new Vector3(X.x, transform.position.y, X.z);
        }
    }

    #endregion

    void WallRelativeMovement()
    {
        if (rb.velocity.y > -1f)
        {
            if (Input.GetButton("PS4_DPAD_UP") && !climbController.canLedgeClimb)
            {
                this.transform.Translate(this.transform.up * Time.deltaTime * climbController.climbUpSpeed);
                animator.SetFloat("Wall_Climb_Speed", Input.GetAxis("PS4_DPAD_UP"));

            }

            else   if (Input.GetButton("PS4_DPAD_DOWN"))
            {
                this.transform.Translate(-this.transform.up * Time.deltaTime * climbController.climbFallSpeed);
                animator.SetFloat("Wall_Climb_Speed", -Input.GetAxis("PS4_DPAD_DOWN"));

            }
            else
            {
                animator.SetFloat("Wall_Climb_Speed", 0);

            }
        }
    }

    public void LedgeClimb()
    {
        if (!climbController.isLedgeClimbing)
        {
            if (Input.GetButtonDown("PS4_DPAD_UP") && (climbController.canWallSnap || climbController.hasLedgeSnapped) && (animator.GetCurrentAnimatorStateInfo(0).IsName("Ledge_Snap") || animator.GetCurrentAnimatorStateInfo(0).IsName("Ledge_Hold")))
            {
                if(ledgeClimbPosition != Vector3.zero)
                {
                    climbController.isLedgeClimbing = true;
                    animator.SetTrigger("LedgeClimbTrg");
                }

            }

            if (Input.GetButton("PS4_DPAD_DOWN") && climbController.canWallSnap)
            {
                this.transform.Translate(-this.transform.up * Time.deltaTime * climbController.climbFallSpeed);
            }
        }
        else
        {
            climbController.ledgeClimbProgress += Time.deltaTime / climbController.ledgeClimbSlerpFactor;

            if (climbController.ledgeClimbProgress > 1f)
            {
                climbController.ledgeClimbProgress = 1f;
            }
            this.transform.position = climbController.ledgeClimbSpline.GetPoint(climbController.ledgeClimbProgress);

            if (Vector3.Distance(transform.position, ledgeClimbPosition) < 0.01f || stateController.isGrounded)
            {
                this.transform.position = ledgeClimbPosition;
                climbController.isLedgeClimbing = false;
                climbController.canLedgeClimb = false;
                climbController.canLedgeSnap = false;
                climbController.isLedgeSnapping = false;
                //physicsSettings.groundGravity = -9.8f;
                rb.isKinematic = false;

                climbController.ledgeClimbProgress = 0f;
                ledgeClimbPosition = Vector3.zero;

                if(!stateController.isGrounded)
                {
                    audioManager.Play("GroundHit");

                }
                animator.SetInteger("Jumping", 0);
                ledgeClimbPosition = Vector3.zero;
                animator.applyRootMotion = false;

            }
        }
    }

    /// <summary>
    /// This function adjusts Tomba collider and animator flags to grab a ground object before actually landing on it.
    /// </summary>
    public bool CheckForGrabbableObjects()
    {
        RaycastHit hit;

        return CheckForGrabbableObjects(out hit);
    }

    /// <summary>
    /// This function adjusts Tomba collider and animator flags to grab a ground object before actually landing on it.
    /// </summary>
    public bool CheckForGrabbableObjects(out RaycastHit hit)
    {
        //Get bottom clearance and return hitted object
        hit = GetRaycastBottomClearance(physicsSettings.collidingMask);

        if (hit.transform != null)
        {
            if (objGrabController.grabbableTags.Contains(hit.transform.tag))
            {
                objGrabController.canGrab = true;
                this.GetComponent<CapsuleCollider>().height = 0.51f;
                this.GetComponent<CapsuleCollider>().center = new Vector3(this.GetComponent<CapsuleCollider>().center.x, 0.79f, this.GetComponent<CapsuleCollider>().center.z);
            }
            else
            {
                objGrabController.canGrab = false;
                animator.ResetTrigger("LandGrabTrg");
                this.GetComponent<CapsuleCollider>().height = 1.081906f;
                this.GetComponent<CapsuleCollider>().center = new Vector3(this.GetComponent<CapsuleCollider>().center.x, 0.54f, this.GetComponent<CapsuleCollider>().center.z);
            }
        }
        else
        {
            objGrabController.canGrab = false;
            animator.ResetTrigger("LandGrabTrg");
            this.GetComponent<CapsuleCollider>().height = 1.081906f;
            this.GetComponent<CapsuleCollider>().center = new Vector3(this.GetComponent<CapsuleCollider>().center.x, 0.54f, this.GetComponent<CapsuleCollider>().center.z);
        }
        animator.SetBool("canLandGrab", objGrabController.canGrab);

        return objGrabController.canGrab;
    }

    public bool CheckLedgeSurface()
    {
        bool foundSurface = true;

        climbController.ledgeDetectionRays[0] = transform.position + (transform.right * climbController.ledgeSnapFirstRayOffsetRight) + (transform.up * climbController.ledgeSnapFirstRayOffsetUp) + (transform.forward * climbController.ledgeSnapFirstRayOffsetForward);
        climbController.ledgeDetectionRays[1] = transform.position - (transform.right * climbController.ledgeSnapSecondRayOffsetRight) + (transform.up * climbController.ledgeSnapSecondRayOffsetUp) + (transform.forward * climbController.ledgeSnapSecondRayOffsetForward);

        foreach (Vector3 r in climbController.ledgeDetectionRays)
        {
            if (Physics.Raycast(r, -transform.up, 1f, climbController.climbableSurfaces))
            {
                foundSurface &= true;
            }
            else
            {
                foundSurface &= false;
            }
        }

        foundSurface &= !CheckForClimbableSurface(0.2f);

        return foundSurface;
    }

    private float DegAgleBetweenVectors3(Vector3 vect1, Vector3 vect2)
    {
        float cosAngle = Vector3.Dot(vect1.normalized, vect2.normalized);
        float degAngle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;
        return degAngle;
    }

    public bool CheckForClimbableSurface(float distance)
    {
        bool foundSurface = true;
        int i = 0;

        climbController.climbDetectionRays[0] = transform.position + (transform.up * climbController.climbDetectionRayUpOffset) + (transform.up * climbController.rayDistanceFromPlayerPosition);
        climbController.climbDetectionRays[1] = transform.position - (transform.up * climbController.climbDetectionRayDownOffset) + (transform.up * climbController.rayDistanceFromPlayerPosition);
        climbController.climbDetectionRays[2] = Vector3.zero;//transform.position + (transform.right * climbController.climbDetectionRayRightOffset) + (transform.up * climbController.rayDistanceFromPlayerPosition);
        climbController.climbDetectionRays[3] = Vector3.zero;//transform.position - (transform.right * climbController.climbDetectionRayLeftOffset) + (transform.up * climbController.rayDistanceFromPlayerPosition);
        climbController.climbDetectionRays[4] = climbController.climbDetectionRays[0] + (transform.up * climbController.climbDetectionRayOverheadOffset);

        foreach (Vector3 r in climbController.climbDetectionRays)
        {
            if(r != Vector3.zero)
            {
                RaycastHit hit;

                if (Physics.Raycast(r - transform.forward, transform.forward, distance + 1f, climbController.climbableSurfaces))
                {
                    foundSurface &= true;
                }
                else
                {
                    foundSurface &= false;
                }
                Physics.Raycast(r - transform.forward, transform.forward, out hit, 2f, climbController.climbableSurfaces);
                climbController.climbDetectionRaysHits[i] = hit.point;
            }
            i += 1;
        }
        return foundSurface;
    }

    #region UpdateMovement

    void CameraRelativeMovement()
    {
        float inputDashVertical = Input.GetAxisRaw("DashVertical");
        float inputDashHorizontal = Input.GetAxisRaw("DashHorizontal");
        float inputVertical = 0;// Input.GetAxisRaw("PS4_PAD_LEFT_Y");

        if (onSwitchCollider)
        {
            if (Input.GetButton("PS4_DPAD_UP") && !Input.GetButton("PS4_X"))
            {
                if (inputVertical != 1)
                {
                    inputVertical = 1;
                    ResetIdle();
                }
            }
            else
            {
                inputVertical = 0;
            }

            if (Input.GetButton("PS4_DPAD_DOWN"))
            {
                if (inputVertical != -1)
                {
                    //inputVertical = -1;
                    //StartCoroutine(camScript.Rotation.SetIsoRotationDownFlag(false, Time.deltaTime * 18f));

                    //camScript.Rotation.SetIsoRotation(AdvancedUtilities.Cameras.Components.RotationComponent.eIsoRotation.DOWN, false);

                }
            }
        }
        else
        {
            camScript.Rotation.SetIsoRotation(AdvancedUtilities.Cameras.Components.RotationComponent.eIsoRotation.DOWN, false);
            StartCoroutine(camScript.Rotation.SetIsoRotationDownFlag(false, Time.deltaTime * 20f));
        }

        if(!isSliding && !objGrabController.hasGrabbed)
        {
            if (Input.GetButton("PS4_DPAD_RIGHT") && (!climbController.hasLedgeSnapped && !climbController.hasWallSnapped))
            {
                if (stateController.canMove)
                {
                    inputHorizontal = 1;
                    lastInputHorizontal = 1;
                }
                ResetIdle();
            }

            if (Input.GetButton("PS4_DPAD_LEFT") && (!climbController.hasLedgeSnapped && !climbController.hasWallSnapped))
            {
                if (stateController.canMove)
                {
                    inputHorizontal = -1;
                    lastInputHorizontal = -1;
                }
                ResetIdle();
            }
            inputHorizontal = Input.GetAxis("Horizontal") * 1f;

            //Check if we can acquire vertical inputs
            if(!movementSettings.lockZMovement)
            {
                inputVertical = Input.GetAxis("Vertical") * 1f;
            }
        }
        else if(isSliding && !objGrabController.hasGrabbed) //Tomba is sliding, freeze it's direction and reset startedSliding flag.
        {
            if (Physics.Raycast(this.transform.position + (this.transform.up * 0.3f), this.transform.forward, 0.3f, physicsSettings.collidingMask))
            {
                if(startedSliding)
                {
                    inputHorizontal = lastInputHorizontal * -1;

                    RotateTowardsMovementDir(lastInputVec * -1, true);
                    startedSliding = false;
                }
            }
        }
        else if((!isSliding && objGrabController.hasGrabbed && !objGrabController.waitForThrow ) || objGrabController.isThrowing)
        {
            inputHorizontal = 0;
            //lastInputHorizontal = 0;

        }
        else if(!isSliding && !objGrabController.isThrowing && objGrabController.hasGrabbed && objGrabController.waitForThrow && !GetRaycastBottomClearance(physicsSettings.collidingMask, new Vector3(0f,-0.1f,0f)))
        {
            if (Input.GetButton("PS4_DPAD_RIGHT"))
            {
                if (stateController.canMove)
                {
                    inputHorizontal = 1;
                    lastInputHorizontal = 1;
                }
                ResetIdle();
            }

            if (Input.GetButton("PS4_DPAD_LEFT"))
            {
                if (stateController.canMove)
                {
                    inputHorizontal = -1;
                    lastInputHorizontal = -1;
                }
                ResetIdle();
            }
            inputHorizontal = Input.GetAxis("Horizontal");
        }
        //converts control input vectors into camera facing vectors
        Transform cameraTransform = sceneCamera.transform;

        //Forward vector relative to the camera along the x-z plane   
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;

        //Right vector relative to the camera always orthogonal to the forward vector
        //Vector3 right = new Vector3(forward.z, 0, -forward.x);
        Vector3 right;

        if (movementSettings.lockZMovement)
        {
            right = playerRail.normalized;
        }
        else
        {
            right = new Vector3(forward.z, 0, -forward.x);
        }

        //directional inputs
        dv = inputDashVertical;
        dh = inputDashHorizontal;

        if (!stateController.isRolling)
        {
            targetDashDirection = dh * right + dv * -forward;
        }
        x = inputHorizontal;
        z = inputVertical;

        completeInputVector = x * right + z * forward;

        if (inputVertical != 0 && !movementSettings.lockZMovement)
        {
            RotateTowardsMovementDir(completeInputVector, false);
        }
        else
        {
            RotateTowardsMovementDir(lastInputVec, false);
        }

        if(!movementSettings.lockZMovement)
        {
            inputVec = completeInputVector;
        }
        else
        {
            inputVec = x * right;
        }

        if (inputVec != Vector3.zero)
        {
            lastInputVec = inputVec;

            if(stateController.isGrounded && !objGrabController.isThrowing)
            {
                rb.isKinematic = false;
            }
        }

        if (completeInputVector != Vector3.zero)
        {
            lastCompleteInputVector = completeInputVector;
        }
    }

    //Rotate character towards direction
    public void RotateTowardsMovementDir(Vector3 dirVector, bool instantRotation)
    {
        if (dirVector != Vector3.zero && !stateController.isStrafing && !stateController.isRolling && !stateController.isBlocking)
        {
            if (instantRotation)
            {
                transform.rotation = Quaternion.LookRotation(dirVector);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirVector), Time.deltaTime * movementSettings.rotationSpeed);
            }
        }
    }

    float UpdateMovement()
    {
        CameraRelativeMovement();

        Vector3 motion = inputVec;

        if (stateController.isGrounded)
        {
            //Reduce input for diagonal movement
            if (motion.magnitude > 1)
            {
                motion.Normalize();
            }
            if (stateController.canMove && !stateController.isBlocking)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    newVelocity = motion * movementSettings.animalDashSpeed;
                    if (movementSettings.animalDashParticles != null && !movementSettings.animalDashParticles.activeSelf)
                    {
                        ParticleSystem ps = movementSettings.animalDashParticles.GetComponent<ParticleSystem>();

                        if (ps != null)
                        {
                            ParticleSystem.MainModule m = ps.main;

                            m.loop = true;

                            movementSettings.animalDashParticles.SetActive(true);

                        }
                    }
                }
                else
                {
                    newVelocity = motion * movementSettings.runSpeed;

                    if (movementSettings.animalDashParticles != null && movementSettings.animalDashParticles.activeSelf)
                    {
                        ParticleSystem ps = movementSettings.animalDashParticles.GetComponent<ParticleSystem>();

                        if (ps != null)
                        {
                            ParticleSystem.MainModule m = ps.main;

                            m.loop = false;
                        }
                    }
                }
                if (stateController.isJumping)
                {
                    newVelocity = motion * movementSettings.inAirSpeed;
                    climbController.canAerialSnap = true;
                }
            }
        }
        else
        {
            //If we are falling use momentum
            newVelocity = (motion / 15f) + new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z); 
        }

        if (!stateController.isStrafing && !onSwitchCollider)
        {
            if (inputVec != Vector3.zero)
            {
                RotateTowardsMovementDir(inputVec, true);
            }
            else
            {
                RotateTowardsMovementDir(lastInputVec, false);
            }
        }
        //Commentato perchè troppo restrittivo con movimentazioni 2.5D e 3D
        //if (movementController.lockZMovement)
        //{
        //    newVelocity = new Vector3(newVelocity.x, rb.velocity.y, 0f);//Remove z component to ease rail snap
        //}
        //else
        //{
        //}

        newVelocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);

        rb.velocity = newVelocity;

        //return a movement value for the animator
        return inputVec.magnitude;
    }

    #endregion

    #region Jumping

    #region Trigger Handling
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "SwitchCollider")
        {
            onSwitchCollider = true;
        }

        switch (other.tag)
        {
            case "Slope":
                canCheckForSlopes = true;
                break;

            case "ISO_DoorTrigger":
                camScript.ShowCullingLayer("PerformIsometricCulling");
                camScript.HideCullingLayer("IgnoreIsometricCulling");
                sceneCamera.clearFlags = CameraClearFlags.SolidColor;
                break;

            default:
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case "Pumpkin":
                rb.isKinematic = true;
                break;

            default:
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "SwitchCollider")
        {
            onSwitchCollider = false;
        }

        switch (other.tag)
        {
            case "Slope":
                canCheckForSlopes = false;
                break;

            case "ISO_DoorTrigger":
                camScript.HideCullingLayer("PerformIsometricCulling");
                camScript.ShowCullingLayer("IgnoreIsometricCulling");
                sceneCamera.clearFlags = CameraClearFlags.Skybox;
                break;

            default:
                break;
        }
    }
    #endregion



    #region Collision Handling
    private void OnCollisionEnter(Collision collision)
    {
        if (objGrabController.grabbableTags.Contains(collision.gameObject.tag))
        {
            HandleGrabObjectCollision();
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Bridge":
                stateController.onBridge = true;
                break;

            default:
                break;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Bridge":
                stateController.onBridge = true;
                break;

            default:
                stateController.onBridge = false;
                break;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (objGrabController.grabbableTags.Contains(collision.gameObject.tag))
        {
            animator.ResetTrigger("LandGrabTrg");
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Bridge":
                stateController.onBridge = false;
                break;

            default:
                break;
        }
    }
    #endregion

    /// <summary>
    /// Handles air movement and RigidBody velocity clamping.
    /// </summary>
    void AirControl()
    {
        if (!stateController.isGrounded)
        {
            CameraRelativeMovement();

            Vector3 motion = Vector3.ClampMagnitude(inputVec, 2f);
            motion *= (Mathf.Abs(inputVec.x) == 1 && Mathf.Abs(inputVec.z) == 1) ? 0.7f : 1;
            rb.AddForce(motion * movementSettings.inAirSpeed, ForceMode.Acceleration);

            //limit the amount of velocity we can achieve
            float velocityX = 0;
            float velocityZ = 0;
            if (rb.velocity.x > maxVelocity)
            {
                velocityX = GetComponent<Rigidbody>().velocity.x - maxVelocity;
                if (velocityX < 0)
                {
                    velocityX = 0;
                }
                rb.AddForce(new Vector3(-velocityX, 0, 0), ForceMode.Acceleration);
            }
            if (rb.velocity.x < minVelocity)
            {
                velocityX = rb.velocity.x - minVelocity;
                if (velocityX > 0)
                {
                    velocityX = 0;
                }
                rb.AddForce(new Vector3(-velocityX, 0, 0), ForceMode.Acceleration);
            }
            if (rb.velocity.z > maxVelocity)
            {
                velocityZ = rb.velocity.z - maxVelocity;
                if (velocityZ < 0)
                {
                    velocityZ = 0;
                }
                rb.AddForce(new Vector3(0, 0, -velocityZ), ForceMode.Acceleration);
            }
            if (rb.velocity.z < minVelocity)
            {
                velocityZ = rb.velocity.z - minVelocity;
                if (velocityZ > 0)
                {
                    velocityZ = 0;
                }
                rb.AddForce(new Vector3(0, 0, -velocityZ), ForceMode.Acceleration);
            }
        }
    }

    /// <summary>
    /// Checks if there is a clearance downwards the player on the specified layermask.
    /// </summary>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    RaycastHit GetRaycastBottomClearance(LayerMask layerMask)
    {
        RaycastHit hit;
        Vector3 pos = this.transform.position;
        float radius = 0.16f;

        //determining if grounded
        if (Physics.Linecast(pos + physicsSettings.maxGroundedHeight, pos + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (radius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (radius / 2) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (radius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (radius / 2) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos - transform.right * (radius / 2) + physicsSettings.maxGroundedHeight, pos - transform.right * (radius / 2) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos + transform.right * (radius / 2) + physicsSettings.maxGroundedHeight, pos + transform.right * (radius / 2) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (radius / 2) - transform.right * (radius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (radius / 2) - transform.right * (radius / 2) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (radius / 2) + transform.right * (radius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (radius / 2) + transform.right * (radius / 2) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (radius / 2) + transform.right * (radius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (radius / 2) + transform.right * (radius / 2) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (radius / 2) - transform.right * (radius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (radius / 2) - transform.right * (radius / 2) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (radius) + physicsSettings.maxGroundedHeight, pos - transform.forward * (radius) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (radius) + physicsSettings.maxGroundedHeight, pos + transform.forward * (radius) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos - transform.right * (radius) + physicsSettings.maxGroundedHeight, pos - transform.right * (radius) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos + transform.right * (radius) + physicsSettings.maxGroundedHeight, pos + transform.right * (radius) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (radius * checkRaycastGroundedOffset) - transform.right * (radius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos - transform.forward * (radius * checkRaycastGroundedOffset) - transform.right * (radius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (radius * checkRaycastGroundedOffset) + transform.right * (radius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos + transform.forward * (radius * checkRaycastGroundedOffset) + transform.right * (radius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (radius * checkRaycastGroundedOffset) + transform.right * (radius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos - transform.forward * (radius * checkRaycastGroundedOffset) + transform.right * (radius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (radius * checkRaycastGroundedOffset) - transform.right * (radius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos + transform.forward * (radius * checkRaycastGroundedOffset) - transform.right * (radius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedDistanceDown, out hit, layerMask))
        {
            return hit;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Checks if there is a clearance downwards the player on the specified layermask.
    /// </summary>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    bool GetRaycastBottomClearance(LayerMask layerMask, Vector3 downOffset)
    {
        RaycastHit hit;
        Vector3 pos = this.transform.position;

        //determining if grounded
        if (Physics.Linecast(pos + physicsSettings.maxGroundedHeight, pos + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos - transform.right * (physicsSettings.maxGroundedRadius) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos + transform.right * (physicsSettings.maxGroundedRadius) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + downOffset, out hit, layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool GetRaycastBottomClearance(LayerMask layerMask, Vector3 downOffset, out RaycastHit hit)
    {
        Vector3 pos = this.transform.position;

        //determining if grounded
        if (Physics.Linecast(pos + physicsSettings.maxGroundedHeight, pos + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos - transform.right * (physicsSettings.maxGroundedRadius) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos + transform.right * (physicsSettings.maxGroundedRadius) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + downOffset, out hit, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + downOffset, out hit, layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Helper function for CheckForGrounded.
    /// </summary>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    bool CheckRaycastGrounded(LayerMask layerMask)
    {
        Vector3 pos = this.transform.position;

        return (Physics.Linecast(pos + physicsSettings.maxGroundedHeight, pos + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos - transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos - transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos + transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos + transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedDistanceDown, layerMask)
        || Physics.Linecast(pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) - transform.right * (physicsSettings.maxGroundedRadius * checkRaycastGroundedOffset) + physicsSettings.maxGroundedDistanceDown, layerMask));

    }

    /// <summary>
    /// Checks if character is within a certain distance from the ground, and markes it IsGrounded
    /// </summary>
    /// <param name="threshold"></param>
    /// <returns></returns>
    bool CheckForGrounded(float threshold)
    {
        float distanceToGround;
        RaycastHit hit;
        RaycastHit hit2;
        Vector3 offset = new Vector3(0, .45f, 0);
        Debug.DrawRay((transform.position + offset), -Vector3.up);

        if (Physics.Raycast((transform.position + offset), -Vector3.up, out hit, 100f, physicsSettings.collidingMask))
        {
            distanceToGround = hit.distance;

            if (distanceToGround < threshold)
            {
                if (rb.velocity.y > physicsSettings.rigidBodyThresholdYVel)
                {
                    if (!stateController.isGrounded)
                    {
                        if (stateController.isFalling)
                        {

                            if (!animator.GetBool("canLandGrab"))
                            {
                                audioManager.Play("GroundHit");
                            }
                            stateController.isFalling = false;
                            stateController.canJump = true;

                            if(!isSliding)
                            {
                                animator.SetInteger("Jumping", 0);
                            }
                            ledgeClimbPosition = Vector3.zero;
                            animator.applyRootMotion = false;

                        }
                        rb.velocity = Vector3.zero;

                    }
                    stateController.isGrounded = true;
                    animator.SetBool("isFallingAfterGrab", false);
                    //HandlePumpRockPress();

                    if (!objGrabController.isThrowing)
                    {
                        rb.isKinematic = false;
                    }
                    stateController.canJump = true;
                    startFall = false;
                    stateController.isFalling = false;

                    ////Reset hasGrabbed state if we are not grounded and no bottom grabbable objects are found.
                    //if (!CheckForGrabbableObjects())
                    //{
                    //    objGrabController.hasGrabbed = false;
                    //}

                    if (animator.GetInteger("Jumping") != 0 && !stateController.isJumping && !stateController.isFalling && stateController.isGrounded) 
                    {
                        animator.SetInteger("Jumping", 0);
                        if (!animator.GetBool("canLandGrab"))
                        {
                            audioManager.Play("GroundHit");
                        }
                    }
                }
            }
            else
            {
                if (!CheckRaycastGrounded(physicsSettings.collidingMask) || climbController.isLedgeClimbing || climbController.isLedgeSnapping || climbController.hasLedgeSnapped)
                {
                    stateController.isGrounded = false;
                }
                else
                {
                    if (!stateController.isGrounded)
                    {
                        if (!animator.GetBool("canLandGrab"))
                        {
                            audioManager.Play("GroundHit");

                        }
                        stateController.isFalling = false;

                        ////Reset hasGrabbed state if we are not grounded and no bottom grabbable objects are found.
                        //if (!CheckForGrabbableObjects())
                        //{
                        //    objGrabController.hasGrabbed = false;
                        //}

                        stateController.canJump = true;
                        animator.SetInteger("Jumping", 0);
                        ledgeClimbPosition = Vector3.zero;
                        animator.applyRootMotion = false;

                    }
                    if (animator.GetInteger("Jumping") != 0 && !stateController.isJumping && !stateController.isFalling && stateController.isGrounded)
                    {
                        animator.SetInteger("Jumping", 0);

                    }
                    stateController.isGrounded = true;
                    animator.SetBool("isFallingAfterGrab", false);
                    //HandlePumpRockPress();

                }
            }
        }
        return stateController.isGrounded;

    }

    /// <summary>
    /// Handles jumping action
    /// </summary>
    void Jumping()
    {
        if (stateController.isGrounded && !waitingForSlopeEnd && !objGrabController.hasGrabbed)
        {
            if (stateController.canJump && Input.GetButtonDown("PS4_X"))
            {
                if (!stateController.isJumping && !stateController.isFalling)
                {
                    animator.applyRootMotion = false;

                    ledgeClimbPosition = Vector3.zero;

                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        StartCoroutine(_Jump(new Vector3(0f, 1f, 0f), movementSettings.jumpSpeed + Mathf.Abs(rb.velocity.magnitude) / 3));

                    }
                    else
                    {
                        StartCoroutine(_Jump(new Vector3(0f, 1f, 0f), movementSettings.jumpSpeed));

                    }
                    ResetIdle();
                }
            }
        }

        else if (climbController.hasWallSnapped || climbController.hasLedgeSnapped)
        {
            //if (Input.GetButtonDown("PS4_X"))
            //{
            //    if (Physics.Raycast(climbController.climbDetectionRays[4], transform.forward, distanceFromWall, climbController.climbableSurfaces))
            //    {
            //        animator.SetBool("Wall_Climbing", false);

            //        rb.isKinematic = false;
            //        ledgeClimbPosition = Vector3.zero;

            //        StartCoroutine(_Jump((-transform.forward * climbController.pushBackMultiplierOnJumpOff) + (transform.up * climbController.pushUpMultiplierOnJumpOff), movementController.wallJumpSpeed));
            //        ResetIdle();
            //    }
            //}
        }
        else if (isSliding && !objGrabController.hasGrabbed)
        {
            if (Input.GetButtonDown("PS4_X"))
            {
                //float jumpSpeedAdjusted = rb.velocity.sqrMagnitude;

                //if (jumpSpeedAdjusted > 1f)
                //{
                //    jumpSpeedAdjusted = 1f;
                //}
                //else if(jumpSpeedAdjusted < 0f)
                //{
                //    jumpSpeedAdjusted = 1f;
                //}
                //StartCoroutine(_Jump(new Vector3(0f, 1f, 0f), (rb.velocity.sqrMagnitude * 0.1f) + movementController.jumpSpeed));

                //ResetIdle();

                if (!stateController.isJumping && !stateController.isFalling)
                {
                    animator.applyRootMotion = false;

                    ledgeClimbPosition = Vector3.zero;

                    rb.velocity = Vector3.zero;

                    StartCoroutine(_Jump(new Vector3(0f, 1f, 0f), movementSettings.jumpSpeed + 1f));

                    ResetIdle();
                }
            }
        }
        else if (objGrabController.hasGrabbed)
        {
            if (Input.GetButtonDown("PS4_X") && !objGrabController.waitForThrow)
            {
                objGrabController.waitForThrow = true;
                StartCoroutine(_Jump(new Vector3(0f, 1f, 0f), movementSettings.jumpSpeed -1f));
            }

            else if (Input.GetButtonDown("PS4_X") && objGrabController.waitForThrow && stateController.isFalling)
            {
                ThrowGrabbedObject();
            }
        }
        else
        {
            isSliding = false;
            waitingForSlopeEnd = false;
            slidingTimeElapsed = 0;
            animator.applyRootMotion = false;

            stateController.canJump = false;

            if (stateController.isFalling)
            {
                //set the animation back to falling
                animator.SetInteger("Jumping", 2);
                //prevent from going into land animation while in air
                if (!startFall)
                {
                    animator.SetTrigger("JumpTrigger");
                    startFall = true;
                }
            }
        }
    }

    public IEnumerator _Jump(Vector3 jumpingvector, float jumpSpeed)
    {
        stateController.isJumping = true;

        audioManager.Play("Player_Jump");
        animator.SetInteger("Jumping", 1);
        animator.SetTrigger("JumpTrigger");

        // Apply the current forward movement to launch force
        rb.AddForce(this.transform.forward.normalized * 2f, ForceMode.Acceleration);

        if (jumpSpeed == 0)
        {
            rb.AddForce(jumpingvector, ForceMode.Impulse);
        }
        else
        {
            rb.velocity += jumpSpeed * jumpingvector;

        }
        stateController.canJump = false;
        yield return new WaitForSeconds(.5f);
        stateController.isJumping = false;
    }

    #endregion

    void Hit()
    {

    }

    void FootL()
    {

    }

    void FootR()
    {

    }

    void Jump()
    {

    }

    void Land()
    {

    }
    private void OnDrawGizmos()
    {
        RaycastHit hit1;
        Vector3 offset = new Vector3(0, 5f, 0);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(hitLedgeClimb.point, 0.2f);

        Vector3 pos = this.transform.position;


        Debug.DrawLine(pos + physicsSettings.maxGroundedHeight, pos + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius / 2) + transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius / 2) - transform.right * (physicsSettings.maxGroundedRadius / 2) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos - transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos + transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos - transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos - transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos + transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedHeight, pos + transform.right * (physicsSettings.maxGroundedRadius) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos - transform.forward * (physicsSettings.maxGroundedRadius * 0.75f) - transform.right * (physicsSettings.maxGroundedRadius * 0.75f) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius * 0.75f) - transform.right * (physicsSettings.maxGroundedRadius * 0.75f) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos + transform.forward * (physicsSettings.maxGroundedRadius * 0.75f) + transform.right * (physicsSettings.maxGroundedRadius * 0.75f) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius * 0.75f) + transform.right * (physicsSettings.maxGroundedRadius * 0.75f) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos - transform.forward * (physicsSettings.maxGroundedRadius * 0.75f) + transform.right * (physicsSettings.maxGroundedRadius * 0.75f) + physicsSettings.maxGroundedHeight, pos - transform.forward * (physicsSettings.maxGroundedRadius * 0.75f) + transform.right * (physicsSettings.maxGroundedRadius * 0.75f) + physicsSettings.maxGroundedDistanceDown, Color.yellow);
        Debug.DrawLine(pos + transform.forward * (physicsSettings.maxGroundedRadius * 0.75f) - transform.right * (physicsSettings.maxGroundedRadius * 0.75f) + physicsSettings.maxGroundedHeight, pos + transform.forward * (physicsSettings.maxGroundedRadius * 0.75f) - transform.right * (physicsSettings.maxGroundedRadius * 0.75f) + physicsSettings.maxGroundedDistanceDown, Color.yellow);


        if (Physics.SphereCast((transform.position + offset), 0.2f, -Vector3.up, out hit1, 10f))
        {
            groundNormal = hit1.normal;
            Gizmos.color = Color.green;

            Gizmos.DrawRay(hit1.point, hit1.normal);

        }

        if (Physics.Raycast((transform.position + offset), -Vector3.up, out hit1, 10f))
        {
            groundNormal = hit1.normal;
            Gizmos.color = Color.red;

            Gizmos.DrawRay(hit1.point, hit1.normal);

        }


        if (ledgeClimbPosition != Vector3.zero)
        {
            if (climbController.canLedgeClimb)
            {
                Gizmos.color = Color.green;

            }
            else
            {
                Gizmos.color = Color.white;

            }
            Gizmos.DrawWireSphere(ledgeClimbPosition, 0.15f);
            Gizmos.DrawRay(ledgeClimbPosition, ledgeclimbhitNormal);
        }

        if (climbPosition != Vector3.zero)
        {
            Gizmos.DrawWireSphere(climbPosition, 0.15f);
        }

        if (Physics.Raycast(climbController.climbDetectionRays[4] - transform.forward, transform.forward, 2f, climbController.climbableSurfaces))
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.blue;
        }

        Gizmos.DrawRay(climbController.climbDetectionRays[4] - transform.forward, transform.forward);

        if (climbController.canLedgeClimb)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.blue;
        }

        Gizmos.DrawRay(climbController.climbDetectionRays[0] + (transform.up * 0.2f), transform.forward);

        Gizmos.color = Color.blue;

        RaycastHit hit;

        if (Physics.Linecast(climbController.climbDetectionRays[0] + (transform.forward * 0.2f) + (transform.up * 0.4f), climbController.climbDetectionRays[0] + (transform.forward * 0.2f), out hit, climbController.climbableSurfaces))
        {

            UnityEditor.Handles.Label(climbController.climbDetectionRays[0] + (transform.forward * 0.2f) + (transform.up * 0.5f), hit.distance.ToString());

            if (hit.distance < 5f && hit.distance < 4.5f)
            {
                Gizmos.color = Color.red;
            }
        }
        Gizmos.DrawLine(climbController.climbDetectionRays[0] + (transform.forward * 0.2f) + (transform.up * 0.4f), climbController.climbDetectionRays[0] + (transform.forward * 0.2f));

        DrawGizmosDetectionRays();

        if (climbController.canLedgeClimb)
        {
            Transform handleTransform;

            handleTransform = climbController.ledgeClimbSpline.transform;

            Vector3 p0 = handleTransform.TransformPoint(climbController.ledgeClimbSpline.GetControlPoint(0));
            for (int i = 1; i < climbController.ledgeClimbSpline.ControlPointCount; i += 3)
            {
                Vector3 p1 = handleTransform.TransformPoint(climbController.ledgeClimbSpline.GetControlPoint(i));
                Vector3 p2 = handleTransform.TransformPoint(climbController.ledgeClimbSpline.GetControlPoint(i + 1));
                Vector3 p3 = handleTransform.TransformPoint(climbController.ledgeClimbSpline.GetControlPoint(i + 2));

                Handles.DrawBezier(p0, p3, p1, p2, Color.blue, null, 2f);
                p0 = p3;
            }
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(climbController.climbDetectionRaysHits[1], climbController.climbDetectionRaysHits[4] - climbController.climbDetectionRaysHits[1]);
        Gizmos.DrawRay(climbController.climbDetectionRaysHits[1], this.transform.up);

        Gizmos.color = Color.blue;

        for (int i = 0; i < climbController.climbDetectionRaysHits.Length - 1; i++)
        {
            if (i > 0)
            {
                Gizmos.DrawLine(climbController.climbDetectionRaysHits[i - 1], climbController.climbDetectionRaysHits[i]);
            }
            Gizmos.DrawWireSphere(climbController.climbDetectionRaysHits[i], 0.02f);
        }
    }

    private void DrawGizmosDetectionRays()
    {
        foreach (Vector3 r in climbController.climbDetectionRays)
        {
            if (Physics.Raycast(r, transform.forward, 0.35f, climbController.climbableSurfaces))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(r, transform.forward);
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(r, transform.forward);
            }
        }

        foreach (Vector3 r in climbController.ledgeDetectionRays)
        {
            RaycastHit hitInfo;

            if (Physics.Raycast(r, -transform.up, out hitInfo, 1f, climbController.climbableSurfaces))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(r, -transform.up);
                UnityEditor.Handles.Label(r, hitInfo.distance.ToString());

            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(r, -transform.up);
                UnityEditor.Handles.Label(climbController.climbDetectionRays[0] + (transform.forward * 0.2f) + (transform.up * 0.5f), hitInfo.distance.ToString());
                UnityEditor.Handles.Label(r, hitInfo.distance.ToString());
            }
        }
    }
    #region MiscMethods

    void AttackKick(int kickSide)
    {
        if (stateController.isGrounded)
        {
            if (kickSide == 1)
            {
                animator.SetTrigger("AttackKick1Trigger");
            }
            else
            {
                animator.SetTrigger("AttackKick2Trigger");
            }
            StartCoroutine(LockMovement(0, .8f));
        }
    }

    void GetHit()
    {
        int hits = 5;
        int hitNumber = Random.Range(0, hits);
        animator.SetTrigger("GetHit" + (hitNumber + 1).ToString() + "Trigger");
        StartCoroutine(LockMovement(.1f, .4f));
        //apply directional knockback force
        if (hitNumber <= 1)
        {
            StartCoroutine(_Knockback(-transform.forward, 8, 4));
        }
        else if (hitNumber == 2)
        {
            StartCoroutine(_Knockback(transform.forward, 8, 4));
        }
        else if (hitNumber == 3)
        {
            StartCoroutine(_Knockback(transform.right, 8, 4));
        }
        else if (hitNumber == 4)
        {
            StartCoroutine(_Knockback(-transform.right, 8, 4));
        }
    }

    IEnumerator _Knockback(Vector3 knockDirection, float knockBackAmount, int variableAmount)
    {
        stateController.isKnockback = true;
        StartCoroutine(_KnockbackForce(knockDirection, knockBackAmount, variableAmount));
        yield return new WaitForSeconds(.1f);
        stateController.isKnockback = false;
    }

    IEnumerator _KnockbackForce(Vector3 knockDirection, float knockBackAmount, int variableAmount)
    {
        while (stateController.isKnockback)
        {
            rb.AddForce(knockDirection * ((knockBackAmount + Random.Range(-variableAmount, variableAmount)) * (knockbackMultiplier * 10)), ForceMode.Impulse);
            yield return null;
        }
    }

    IEnumerator _Death()
    {
        animator.SetTrigger("Death1Trigger");
        StartCoroutine(LockMovement(.1f, 1.5f));
        stateController.isDead = true;
        animator.SetBool("Moving", false);
        inputVec = new Vector3(0, 0, 0);
        yield return null;
    }

    IEnumerator _Revive()
    {
        animator.SetTrigger("Revive1Trigger");
        stateController.isDead = false;
        yield return null;
    }

    #endregion

    #region _Coroutines

    //method to keep character from moveing while attacking, etc
    public IEnumerator LockMovement(float delayTime, float lockTime)
    {
        yield return new WaitForSeconds(delayTime);
        stateController.canAction = false;
        stateController.canMove = false;
        animator.SetBool("Moving", false);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        inputVec = new Vector3(0, 0, 0);
        animator.applyRootMotion = true;
        yield return new WaitForSeconds(lockTime);
        stateController.canAction = true;
        stateController.canMove = true;
        animator.applyRootMotion = false;
    }

    public override void Save(string path)
    {
        throw new System.NotImplementedException();
    }

    public override void Load()
    {
        throw new System.NotImplementedException();
    }

    #endregion
}
