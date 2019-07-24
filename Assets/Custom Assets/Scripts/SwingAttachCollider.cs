using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SwingAttachCollider : MonoBehaviour
{
    private bool firstAttach = false;
    float oldAxis = 0;
    private PlayerMovementRail currentActiveRail;
    private Rigidbody rigidBody;
    public GameObject swinger;
    bool updateCameraPosition = false;
    Vector3 oldcampos;
    private bool startCheckDistance = false;
    private float distance = 1f;

    public float angle = 0f;
    public ForceMode forceMode = ForceMode.Impulse;
    public float swingForceMultiplier = 1f;
    public float swingForceMultiplierAuto = 750f;

    private bool attached = false;
    TombiCharacterController charScript;
    Collider swingObjColl = null;
    public HingeJoint hinge = null;
    //public IKHandler ikHandler;

    SphereCollider coll;

    AdvancedUtilities.Cameras.BasicCameraController camScript;
    // Start is called before the first frame update
    void Start()
    {
        charScript = FindObjectOfType<TombiCharacterController>();
        //ikHandler = charScript.GetComponent<IKHandler>();
        coll = this.GetComponent<SphereCollider>();
        if (charScript != null)
        {
            rigidBody = charScript.GetComponent<Rigidbody>();
        }
        camScript = FindObjectOfType<AdvancedUtilities.Cameras.BasicCameraController>();
        currentActiveRail = FindObjectOfType<PlayerMovementRail>();
        oldSwingForceMultiplier = swingForceMultiplier;
    }
    public float signedAngularVelZ = 0f;
    public float angularVelZ = 0f;
    public float maxAngularVelZ = 0f;
    public float hingeMotorActiveThreshold = 6.45f;
    public bool hingeMotorActiveForSpin = false;
    public float angle3 = 0f;
    public float angle4 = 0f;
    public SubCollider collone = null;
    public bool canFreeJumpOff = false;

    public float lastFrameAngularVelZ = 0f;
    // Update is called once per frame
    void FixedUpdate()
    {
        if(swingObjColl != null && swingObjColl.gameObject != null)
        {
            collone = swingObjColl.gameObject.GetComponentInChildren<SubCollider>(false);

        }

        if (collone != null)
        {
            if (collone.active && mustJump)
            {
                if (lastPlayerInputBeforeAttach == eInput.LEFT)
                {
                    if (rotatedRight)
                    {
                        DetachFromObject(Vector3.up, (0.1f * rigidBody.angularVelocity.z));

                    }
                    else if (rotatedLeft)
                    {
                        DetachFromObject(Vector3.up, (0.1f * rigidBody.angularVelocity.z));

                    }
                }
                else if (lastPlayerInputBeforeAttach == eInput.RIGHT)
                {
                    if (rotatedRight)
                    {
                        DetachFromObject(Vector3.up, (0.1f * rigidBody.angularVelocity.z));
                    }
                    else if (rotatedLeft)
                    {
                        DetachFromObject(Vector3.up, (0.1f * rigidBody.angularVelocity.z));

                    }
                }
                mustJump = false;
            }
        }

        if (canFreeJumpOff && mustJump)
        {
            if (lastPlayerInputBeforeAttach == eInput.LEFT)
            {
                if (rotatedRight)
                {
                    DetachFromObject(Vector3.up, (0.1f * rigidBody.angularVelocity.z));

                }
                else if (rotatedLeft)
                {
                    DetachFromObject(Vector3.up, (0.1f * rigidBody.angularVelocity.z));

                }
            }
            else if (lastPlayerInputBeforeAttach == eInput.RIGHT)
            {
                if (rotatedRight)
                {
                    DetachFromObject(Vector3.up, (0.1f * rigidBody.angularVelocity.z));
                }
                else if (rotatedLeft)
                {
                    DetachFromObject(Vector3.up, (0.1f * rigidBody.angularVelocity.z));

                }
            }
            mustJump = false;
        }

        if (firstAttach)
        {
            firstAttach = false;

        }
        if (attached)
        {
            if (Mathf.Abs(angularVelZ) < 6f && Input.GetButton("Horizontal") == false)
            {
                swingObjColl.gameObject.GetComponent<PlayerSwingObject>().subCollider1.SetActive(false);
                swingObjColl.gameObject.GetComponent<PlayerSwingObject>().subCollider2.SetActive(false);
                canFreeJumpOff = true;
                if (mustJump == true)
                {
                    mustJump = false;
                }
            }

            if (swinger != null)
            {
                if (hinge != null)
                {
                    if ((hinge.angle == hinge.limits.min + 10) || (hinge.angle == hinge.limits.max - 10))
                    {
                        // Do Something
                        ignoreInput = true;
                    }
                    else
                    {
                        ignoreInput = false;
                    }
                    angle1 = DegAgleBetweenVectors3(hinge.transform.forward, -charScript.gameObject.transform.up);
                    //{
                    //    if (angle1 < 100 && Mathf.Abs(lastFrameAngularVelZ) < Mathf.Abs(charScript.rb.angularVelocity.z))
                    //    {
                    //        if (!ignoreInput)
                    //        {
                    //            charScript.rb.angularVelocity *= -1;
                    //            ignoreInput = true;
                    //        }
                    //        //if ((lastPlayerInput == eInput.RIGHT && Input.GetButton("PS4_DPAD_RIGHT")) || (lastPlayerInput == eInput.LEFT && Input.GetButton("PS4_DPAD_LEFT")))
                    //        //{
                    //        //    charScript.rb.angularDrag = 100000;
                    //        //    ignoreInput = true;
                    //        //    Debug.Log("Dumping");

                    //        //}
                    //        //else
                    //        //{
                    //        //    charScript.rb.angularDrag = 10;
                    //        //    //ignoreInput = false;
                    //        //    Debug.Log("Reset Dumping");

                    //        //}
                    //    }
                    //    else if (angle1 > 100)
                    //    {
                    //        charScript.rb.angularDrag = 10;
                    //        ignoreInput = false;

                    //    }
                    //    else
                    //    {
                    //        ignoreInput = false;
                    //        charScript.rb.angularDrag = 10;
                    //    }

                    //    if(ignoreInput)
                    //    {
                    //        charScript.rb.angularDrag += 5;

                    //    }
                    //    lastFrameAngularVelZ = charScript.rb.angularVelocity.z;

                    //}
                }

                float spinSign = 1f;
                Debug.DrawRay(rigidBody.transform.position, rigidBody.velocity, Color.red);
                signedAngularVelZ = charScript.GetComponent<Rigidbody>().angularVelocity.z;
                angularVelZ = UnityEngine.Mathf.Abs(charScript.GetComponent<Rigidbody>().angularVelocity.z);
                angle4 = DegAgleBetweenVectors3(-hinge.transform.forward, -charScript.gameObject.transform.up);
                angle3 = DegAgleBetweenVectors3(hinge.transform.forward, -charScript.gameObject.transform.up);

                if (lastPlayerInputBeforeAttach == eInput.RIGHT)
                {
                    angle2 = DegAgleBetweenVectors3(-hinge.transform.right, -charScript.gameObject.transform.up);
                    spinSign = -1f;

                    if (Mathf.Abs(angularVelZ) > 8f)
                    {
                        swingObjColl.gameObject.GetComponent<PlayerSwingObject>().subCollider1.SetActive(false);
                        swingObjColl.gameObject.GetComponent<PlayerSwingObject>().subCollider2.SetActive(true);
                        canFreeJumpOff = false;
                    }

                }
                else if (lastPlayerInputBeforeAttach == eInput.LEFT)
                {
                    angle2 = DegAgleBetweenVectors3(hinge.transform.right, -charScript.gameObject.transform.up);
                    spinSign = 1f;

                    if (Mathf.Abs(angularVelZ) > 8f)
                    {
                        swingObjColl.gameObject.GetComponent<PlayerSwingObject>().subCollider1.SetActive(true);
                        swingObjColl.gameObject.GetComponent<PlayerSwingObject>().subCollider2.SetActive(false);
                        canFreeJumpOff = false;

                    }
               
                }


                if (!hingeMotorActiveForSpin)
                {
                    charScript.rb.angularDrag = 10;

                    if (maxAngularVelZ < angularVelZ)
                    {
                        maxAngularVelZ = angularVelZ;
                    }

                    if(maxAngularVelZ > 11.6)
                    {
                        charScript.rb.angularVelocity *= -1;
                        charScript.rb.angularDrag = 1000000000;
                        maxAngularVelZ = 0;
                    }
                    //if (maxAngularVelZ >= hingeMotorActiveThreshold)
                    //{
                    //    if (angle2 < 10f)
                    //    {

                    //        float angleDiff = angle4 - angle3;

                    //        if (angleDiff > 0)
                    //        {
                    //            if (angleDiff >= 12f)
                    //            {
                    //                if (Input.GetButton("Horizontal"))
                    //                {
                    //                    if (!hingeMotorActiveForSpin)
                    //                    {
                    //                        //var motor = hinge.motor;
                    //                        //motor.targetVelocity = spinSign * 700f;

                    //                        //hinge.motor = motor;
                    //                        //hingeMotorActiveForSpin = true;
                    //                        //hinge.useMotor = true;
                    //                        //rigidBody.useGravity = false;
                    //                        //rigidBody.angularDrag = 0f;

                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    //if (Input.GetButtonDown("Horizontal"))
                    //{
                    //    if (Input.GetAxisRaw("Horizontal") != oldAxis)
                    //    {
                    //        swinger.transform.position = new Vector3(swinger.transform.parent.position.x, swinger.transform.parent.position.y + 4.5f, swinger.transform.position.z);
                    //        oldAxis = Input.GetAxisRaw("Horizontal");
                    //    }

                    //}

                    CheckForDoublePress();

                    if (doubleTapped)
                    {
                        Debug.Log("Double-tapped");

                        if (!rotatedLeft && doubleTappedButton == -1)
                        {
                            if (angularVelZ < 1.5f)
                            {
                                rigidBody.velocity = Vector3.zero;
                                rigidBody.angularVelocity = Vector3.zero;
                                rigidBody.isKinematic = true;
                                
                                if (lastPlayerInputBeforeAttach == eInput.RIGHT)
                                {
                                    hinge.transform.localRotation = new Quaternion(hinge.transform.localRotation.x, hinge.transform.localRotation.y, 180, hinge.transform.localRotation.w);
                                    charScript.RotateTowardsMovementDir(-hinge.transform.right, true);

                                }
                                else
                                {
                                    hinge.transform.localRotation = new Quaternion(hinge.transform.localRotation.x, hinge.transform.localRotation.y, 0, hinge.transform.localRotation.w);
                                    charScript.RotateTowardsMovementDir(hinge.transform.right, true);

                                }
                                rotatedRight = false;
                                rotatedLeft = true;
                                Debug.Log("Rotated left");
                                rigidBody.isKinematic = false;

                            }

                        }
                        else if (doubleTappedButton == 1)
                        {
                            if (!rotatedRight && angularVelZ < 1.5f)
                            {
                                rigidBody.velocity = Vector3.zero;
                                rigidBody.angularVelocity = Vector3.zero;
                                rigidBody.isKinematic = true;

                                if (lastPlayerInputBeforeAttach == eInput.LEFT)
                                {
                                    hinge.transform.localRotation = new Quaternion(hinge.transform.localRotation.x, hinge.transform.localRotation.y, 180, hinge.transform.localRotation.w);
                                    charScript.RotateTowardsMovementDir(hinge.transform.right, true);

                                }
                                else
                                {
                                    hinge.transform.localRotation = new Quaternion(hinge.transform.localRotation.x, hinge.transform.localRotation.y, 0, hinge.transform.localRotation.w);
                                    charScript.RotateTowardsMovementDir(-hinge.transform.right, true);

                                }

                                rotatedRight = true;
                                rotatedLeft = false;
                                Debug.Log("Rotated right");
                                rigidBody.isKinematic = false;

                            }
                        }

                        doubleTapped = false;
                    }
                    else
                    {
                        if (enableSwinger)
                        {
                            //PerformSwingAction();

                        }
                        else
                        {
                            PerformSwingActionWithTorque();
                        }
                    }
                }
                else
                {
                    float angleDiff1 = angle4 - angle3;

                    if (angle2 >= 15f && playSwingSound)
                    {
                        FindObjectOfType<AudioManager>().Play("Swing");
                        playSwingSound = false;
                    }

                    if (angle2 >= 0 && angle2 <= 10f)
                    {
                        playSwingSound = true;

                    }
                }


                if (Input.GetButtonDown("PS4_X"))
                {
                    mustJump = true;

                    if (hingeMotorActiveForSpin)
                    {
                        jumpWhileSpinning = true;

                    }
                    else
                    {

                        //rigidBody.angularVelocity = rigidBody.angularVelocity / 4f;

                    }
                }

                if(jumpWhileSpinning)
                {
                    if(Input.GetButton("Vertical") && Input.GetAxisRaw("Vertical") == 1)
                    {
                        if (angle1 >= 0 && angle1 <= 5f)
                        {
                            hingeMotorActiveForSpin = false;
                            hinge.useMotor = false;
                            angularVelZ = 0f;
                            maxAngularVelZ = 0f;
                            jumpWhileSpinning = false;
                            rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y + 4f, rigidBody.velocity.z);
                            //FindObjectOfType<AudioManager>().SoundLoop("Swing", false);
                            DetachFromObject(Vector3.zero,0.2f);

                        }
                    }
                    else
                    {
                        //if (angle2 >= 90f)
                        //{
                        //    float angleDiff = angle4 - angle3;

                        //    if (angleDiff <= 90f)
                        //    {
                        //        if (angle2 <= 45)
                        //        {

                        //        }
                        //            hingeMotorActiveForSpin = false;
                        //            hinge.useMotor = false;
                        //            angularVelZ = 0f;
                        //            maxAngularVelZ = 0f;
                        //            jumpWhileSpinning = false;
                        //        //FindObjectOfType<AudioManager>().SoundLoop("Swing", false);

                        //        rigidBody.angularVelocity = Vector3.zero;
                        //        rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, rigidBody.velocity.z );
                        //        //rigidBody.velocity = rigidBody.velocity + -hinge.transform.right.normalized;

                        //        DetachFromObject(hinge.transform.right + hinge.transform.forward);
                        //    }
                        //}
                        if (angle2 >= 0 && angle2 <= 5f)
                        {
                            hingeMotorActiveForSpin = false;
                            hinge.useMotor = false;
                            angularVelZ = 0f;
                            maxAngularVelZ = 0f;
                            jumpWhileSpinning = false;
                            //FindObjectOfType<AudioManager>().SoundLoop("Swing", false);

                            //rigidBody.angularVelocity = Vector3.zero;

                            //Vector3 newVel = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, rigidBody.velocity.z) * 2f;
                            //if (newVel.y > 1f)
                            //{
                            //    newVel.y = 1f;
                            //}
                            //rigidBody.velocity = newVel;
                            //rigidBody.velocity = rigidBody.velocity + -hinge.transform.right.normalized;

                            DetachFromObject(rigidBody.transform.forward, 0.1f);
                        }
                    }
                }
            }
        }
        else
        {
            lastPlayerInputBeforeAttach = GetLPIBeforeAttach();

        }
        UpdateCameraPositionSmooth();

    }

    public bool mustJump = false;

    public float angle2 = 0f;
    float doubleTapTime = 0f;
    float doubleTappedButton = 1f;
    bool doubleTapped = false;
    bool waitDoubleTap = false;
    public bool rotatedRight = false;
    public bool rotatedLeft = false;
    bool playSwingSound = false;
    public float angle5 = 0f;
    public float oldAngle5 = 0f;
    public float oldSwingForceMultiplier = 0f;

    private void ActivateSpinMotor()
    {
        var motor = hinge.motor;

            if(rotatedRight)
            {
                motor.targetVelocity = -700f;

            }
            else if(rotatedLeft)
            {
                motor.targetVelocity = 700f;

            }

        hinge.motor = motor;
        hingeMotorActiveForSpin = true;
        hinge.useMotor = true;
        rigidBody.useGravity = false;
        rigidBody.angularDrag = 0f;

        //FindObjectOfType<AudioManager>().SoundLoop("Swing", true);

        //charScript.GetComponent<TorquePIDController>().enabled = false;
    }

    private void PerformSwingActionWithTorque()
    {
        Debug.DrawRay(rigidBody.transform.position, rigidBody.transform.forward * Input.GetAxisRaw("Horizontal"));
        Vector3 vector = Vector3.zero;

        if (charScript.GetComponent<Rigidbody>().freezeRotation == true)
        {
            charScript.GetComponent<Rigidbody>().freezeRotation = false;

        }

        if (lastPlayerInputBeforeAttach == eInput.LEFT)
        {
            if (rotatedRight)
            {
                vector = Quaternion.AngleAxis(-90, Vector3.up) * rigidBody.angularVelocity;
                if (!float.IsNaN(DegAgleBetweenVectors3(vector, -hinge.transform.right)))
                {
                    angle5 = DegAgleBetweenVectors3(vector, -hinge.transform.right);

                }
            }
            else if (rotatedLeft)
            {
                vector = Quaternion.AngleAxis(+90, Vector3.up) * rigidBody.angularVelocity;
                if (!float.IsNaN(DegAgleBetweenVectors3(vector, hinge.transform.right)))
                { 
                    angle5 = DegAgleBetweenVectors3(vector, hinge.transform.right);
                }
            }
        }
        else if (lastPlayerInputBeforeAttach == eInput.RIGHT)
        {
            if (rotatedRight)
            {
                vector = Quaternion.AngleAxis(+90, Vector3.up) * rigidBody.angularVelocity;

                if(!float.IsNaN(DegAgleBetweenVectors3(vector, -hinge.transform.right)))
                {
                    angle5 = DegAgleBetweenVectors3(vector, -hinge.transform.right);

                }

            }
            else if (rotatedLeft)
            {
                vector = Quaternion.AngleAxis(-90, Vector3.up) * rigidBody.angularVelocity;

                if (!float.IsNaN(DegAgleBetweenVectors3(vector, hinge.transform.right)))
                {
                    angle5 = DegAgleBetweenVectors3(vector, hinge.transform.right);
                }
            }
        }




        //if (oldAngle5 != angle5)
        //{
        //    countSpinsBeforeAutoSpin = 0;
        //    swingForceMultiplier = oldSwingForceMultiplier;
        //    oldAngle5 = angle5;
        //}
        //else
        //{
        //    if (angularVelZ > 0.4f && angle5 == 0)
        //    {
        //        if(Input.GetButton("Horizontal"))
        //        {
        //            if (angle4 < 90)
        //            {
        //                rigidBody.angularVelocity = Vector3.Lerp(rigidBody.angularVelocity, new Vector3(rigidBody.angularVelocity.x, rigidBody.angularVelocity.y, rigidBody.angularVelocity.z + 0.01f), 10f);

        //            }
        //            countSpinsBeforeAutoSpin += 1;

        //            if (angularVelZ > rigidBody.maxAngularVelocity -4f && angle5 == 0)
        //            {
        //                //ActivateSpinMotor();
        //            }
        //        }
        //        else
        //        {
        //            countSpinsBeforeAutoSpin = 0f;
        //        }

        //    }
        //}
        //swingForceMultiplier += countSpinsBeforeAutoSpin;
        rigidBody.maxAngularVelocity = 12f;

        Debug.DrawRay(rigidBody.transform.position, vector * -1, Color.blue);

        //if (charScript.GetComponent<TorquePIDController>().isActiveAndEnabled)
        //{
        //    charScript.GetComponent<TorquePIDController>().enabled = false;
        //}

        //rigidBody.AddTorque(-hinge.transform.up * 5f * Mathf.Sin(Time.time * swingForceMultiplier), ForceMode.VelocityChange);


        if (!ignoreInput)
        {
            if (Input.GetButton("Horizontal") && Input.GetAxisRaw("Horizontal") != 0)
            {

                    if (lastPlayerInputBeforeAttach == eInput.LEFT)
                    {
                        if (rotatedRight)
                        {
                            rigidBody.AddTorque(-hinge.transform.up * Time.deltaTime * swingForceMultiplier *  Input.GetAxis("Horizontal"), ForceMode.Force);

                        }

                        else if (rotatedLeft)
                        {
                            rigidBody.AddTorque(hinge.transform.up * Time.deltaTime * swingForceMultiplier * Input.GetAxis("Horizontal"), ForceMode.Force);

                        }
                    }
                    else if (lastPlayerInputBeforeAttach == eInput.RIGHT)
                    {
                        if (rotatedRight)
                        {
                            rigidBody.AddTorque(hinge.transform.up * Time.deltaTime * swingForceMultiplier *  Input.GetAxis("Horizontal"), ForceMode.Force);

                        }

                        else if (rotatedLeft)
                        {
                            rigidBody.AddTorque(-hinge.transform.up * Time.deltaTime * swingForceMultiplier *  Input.GetAxis("Horizontal"), ForceMode.Force);

                        }
                    }

            }
        }
    }

    public bool enableSwinger = false;
    public float countSpinsBeforeAutoSpin = 0f;
    private void PerformSwingAction()
    {
        if (charScript.GetComponent<Rigidbody>().freezeRotation == true)
        {
            charScript.GetComponent<Rigidbody>().freezeRotation = false;

        }

        if (Input.GetButton("Horizontal") && Input.GetAxisRaw("Horizontal") != 0)
        {

            if (Vector3.Distance(swinger.transform.position, swinger.transform.parent.position) < 10f)
            {
                float angleDiff = angle4 - angle3;

                if (angleDiff <= 0)
                {
                    //if (swinger.transform.parent.position.y - swinger.transform.position.y >= 3f)
                    //{
                    //    swinger.transform.position = new Vector3(swinger.transform.parent.position.x, swinger.transform.parent.position.y + 1.5f, swinger.transform.position.z);

                    //}
                    swinger.transform.Translate(swinger.transform.right * Input.GetAxisRaw("Horizontal") * Time.deltaTime * 35f, Space.World);
                    swinger.transform.Translate(-swinger.transform.forward * Time.deltaTime * 25f, Space.World);

                }
                else
                {
                    swinger.transform.position = new Vector3(swinger.transform.parent.position.x, swinger.transform.parent.position.y + 4.5f, swinger.transform.position.z);
                }

                //if (angle4 >= angle3)
                //{
                //    swinger.transform.position = new Vector3(swinger.transform.position.x, swinger.transform.parent.position.y + 2.5f, swinger.transform.position.z);
                //}
            }
            //else
            //{
            //    swinger.transform.position = new Vector3(swinger.transform.parent.position.x, swinger.transform.parent.position.y + 2.5f, swinger.transform.position.z);
            //}
        }
        else
        {
            swinger.transform.position = new Vector3(swinger.transform.parent.position.x, swinger.transform.parent.position.y + 4.5f, swinger.transform.position.z);
        }
    }

    private void CheckForDoublePress()
    {
        //if(angularVelZ < 1f)
        //{
        //    if (Input.GetButtonDown("Horizontal") && waitDoubleTap)
        //    {
        //        if (Time.time - doubleTapTime < 0.35f)
        //        {
        //            if (doubleTappedButton == Input.GetAxisRaw("Horizontal"))
        //            {
        //                doubleTapped = true;
        //                rigidBody.angularVelocity = Vector3.zero;
        //                doubleTapTime = 0f;
        //            }
        //        }
        //        waitDoubleTap = false;
        //    }

        //    if (Input.GetButtonDown("Horizontal") && !waitDoubleTap)
        //    {
        //        doubleTappedButton = Input.GetAxisRaw("Horizontal");
        //        waitDoubleTap = true;
        //        doubleTapTime = Time.time;
        //    }
        //}
        

        if (rigidBody.angularVelocity.z > 12f)
        {
            rigidBody.angularVelocity = new Vector3(rigidBody.angularVelocity.x, rigidBody.angularVelocity.y, 10f);
        }
    }

    public void Update()
    {
        lastPlayerInput = GetLPI();
    }
    public bool ignoreInput = false;

    public eInput lastPlayerInput = eInput.NONE;
    public eInput lastPlayerInput2 = eInput.NONE;
    public eInput lastPlayerInputBeforeAttach = eInput.NONE;

    public enum eInput
    {
        NONE = 0,
        RIGHT = 1,
        LEFT = 2
    }


    eInput GetLPI()
    {
        if (Input.GetButton("PS4_DPAD_RIGHT"))
        {
            return eInput.RIGHT;

        }
        else if (Input.GetButton("PS4_DPAD_LEFT"))
        {
            return eInput.LEFT;
        }
        return lastPlayerInput;
    }

    eInput GetLPIBeforeAttach()
    {
        if (Input.GetButton("PS4_DPAD_RIGHT"))
        {
            //if (lastPlayerInput != eInput.RIGHT)
            //{
            //    ignoreInput = false;
            //}
            return eInput.RIGHT;

        }
        else if (Input.GetButton("PS4_DPAD_LEFT"))
        {
            //if (lastPlayerInput != eInput.LEFT)
            //{
            //    ignoreInput = false;
            //}
            return eInput.LEFT;
        }

        return lastPlayerInputBeforeAttach;
    }

    public float angle1 = 0f;

    /// <summary>
    /// Calculate angle between two Vector3 using dot product.
    /// </summary>
    /// <param name="vect1">First vector</param>
    /// <param name="vect2">Second vector</param>
    /// <returns>Angle in degrees between the two vectors</returns>
    private float DegAgleBetweenVectors3(Vector3 vect1, Vector3 vect2)
    {
        float cosAngle = Vector3.Dot(vect1.normalized, vect2.normalized);
        float degAngle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;
        return degAngle;
    }

    public bool jumpWhileSpinning = false;

    private void DetachFromObject(Vector3 appliedForce, float jumpSpeed)
    {
        hinge.transform.localRotation = new Quaternion(hinge.transform.localRotation.x, hinge.transform.localRotation.y, 0, hinge.transform.localRotation.w);
        rotatedRight = false;
        rotatedLeft = false;
        hinge.gameObject.SetActive(false);
        //Time.timeScale = 0.01f;
        camScript.gameObject.SetActive(true);
        //rigidBody.angularDrag = Mathf.Infinity;
        //rigidBody.angularVelocity = Vector3.zero;

        rigidBody.useGravity = true;


        //ikHandler.isSwinging = false;
        //charScript.GetComponent<TorquePIDController>().target = null;

        distance = 1;

        mustSetCameraInactive = false;
        updateCameraPosition = true;

        rigidBody.freezeRotation = true;
        rigidBody.mass = 5;
        rigidBody.drag = 1f;
        //charScript.movementController.jumpSpeed = jumpSpeed;



        charScript.enabled = true;
        charScript.stateController.isGrounded = false;
        

        IEnumerator test = charScript._Jump(appliedForce, 0f);

        charScript.animator.SetInteger("Jumping", 2);
        charScript.animator.SetTrigger("JumpTrigger");

        //if (appliedForce != Vector3.zero)
        //{
        //    rigidBody.AddForce(appliedForce * 25f, ForceMode.Impulse);

        //}

        attached = false;
        //Physics.IgnoreCollision(charScript.GetComponent<CapsuleCollider>(), swingObjColl.GetComponent<SphereCollider>(), true);

        //StartCoroutine(test);

        hinge = null;
        angle1 = 0;
        angle2 = 0;
        angle3 = 0;
        angle4 = 0;

        //Time.timeScale = 0f;

       charScript.stateController.isFalling = true;


    }

    private void UpdateCameraPositionSmooth()
    {
        if (updateCameraPosition)
        {
            camScript.canUpdateCameraPosition = false;
            camScript.CameraTransform.Position = Vector3.MoveTowards(camScript.CameraTransform.Position, camScript.GetCameraUpdatedPosition(), Time.deltaTime * 3.5f); // No buffer if the buffer would zoom us in past 0.

            distance = Vector3.Distance(camScript.CameraTransform.Position, camScript.GetCameraUpdatedPosition());

            if (distance > 0.1f)
            {
                startCheckDistance = true;

            }
            if (startCheckDistance)
            {
                if (distance < 0.1f)
                {
                    updateCameraPosition = false;
                    camScript.canUpdateCameraPosition = true;
                    startCheckDistance = false;

                    if(mustSetCameraInactive)
                    {
                        camScript.gameObject.SetActive(false);
                        mustSetCameraInactive = false;
                    }
                }
            }
        }
    }

    public float rigidbodyMass = 5f;
    public float rigidbodyAngularDrag = 10f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerSwingObject swingObj = other.GetComponent<PlayerSwingObject>();

        if (swingObj != null)
        {
            hinge = swingObj.hinge;

            if (hinge != null)
            {

                hinge.connectedBody = rigidBody;
                swinger = swingObj.swinger;
                swinger.transform.position = new Vector3(swinger.transform.parent.position.x, swinger.transform.parent.position.y + 1.5f, swinger.transform.position.z);
                //charScript.GetComponent<TorquePIDController>().enabled = true;
                //charScript.GetComponent<TorquePIDController>().target = swinger.transform;

                if (!attached)
                {
                    FindObjectOfType<AudioManager>().Play("Snap");
                    charScript.animator.SetTrigger("JumpTrigger");
                    charScript.animator.SetInteger("Jumping", 2);
                    charScript.animator.SetTrigger("LedgeSnapTrg");
                    charScript.GetComponent<CapsuleCollider>().center = new Vector3(charScript.GetComponent<CapsuleCollider>().center.x, 0.54f, charScript.GetComponent<CapsuleCollider>().center.z);

                    swingObjColl = other;
                    RaycastHit hit;
                    Vector3 fromPosition = transform.position;
                    Vector3 toPosition = other.transform.position;
                    Vector3 direction = toPosition - fromPosition;

                    if (Physics.Raycast(coll.transform.position, direction, out hit, coll.radius))
                    {
                        Debug.DrawRay(transform.position, direction, Color.red);

                        attached = true;
                        firstAttach = true;

                        charScript.enabled = false;

                        //charScript.stateController.isFalling = false;
                        //ikHandler.isSwinging = true;
                        //ikHandler.HandPositionOnSwingR = swingObj.rightHandIK;
                        //ikHandler.HandPositionOnSwingL = swingObj.leftHandIK;

                        //charScript.GetComponent<Rigidbody>().angularDrag = 3.5f;
                        //charScript.GetComponent<Rigidbody>().mass = 0.9f;
                        charScript.GetComponent<Rigidbody>().angularDrag = rigidbodyAngularDrag;
                        charScript.GetComponent<Rigidbody>().mass = rigidbodyMass;
                        charScript.GetComponent<Rigidbody>().drag = 0f;
                        charScript.GetComponent<Rigidbody>().useGravity = true;

                        charScript.GetComponent<Rigidbody>().AddTorque(charScript.transform.forward * 100f, ForceMode.Impulse);

                        hinge.gameObject.SetActive(true);
                        if( lastPlayerInputBeforeAttach == eInput.LEFT)
                        {
                            rotatedLeft = true;
                            rotatedRight = false;
                        }
                        else if (lastPlayerInputBeforeAttach == eInput.RIGHT)
                        {
                            rotatedRight = true;
                            rotatedLeft = false;

                        }

                        //if (camScript != null)
                        //{
                        //camScript.Rotation.SetIsoRotation(AdvancedUtilities.Cameras.Components.RotationComponent.eIsoRotation.LEFT, true);
                        //StartCoroutine(camScript.Rotation.SetIsoRotationLeftFlag(true, Time.deltaTime * 18f));
                        //StartCoroutine(DeactivateCamera(Time.deltaTime * 18f));
                        //}
                        updateCameraPosition = true;
                        mustSetCameraInactive = true;


                    }
                    else
                    {
                        Debug.DrawRay(transform.position, direction, Color.blue);

                    }
                }
            }
        }
    }
    private bool mustSetCameraInactive = false;
    private bool waitingForCameraDeactivation = false;
    IEnumerator DeactivateCameraAutoRailRotation(float time)
    {
        currentActiveRail.canUpdateCameraRotation = false;
        yield return new WaitForSeconds(time);
        currentActiveRail.canUpdateCameraRotation = true;
    }

    IEnumerator ActivateCamera(float time)
    {
        yield return new WaitForSeconds(time);
        camScript.gameObject.SetActive(true);
    }

    IEnumerator DeactivateCamera(float time)
    {
        waitingForCameraDeactivation = true;
        yield return new WaitForSeconds(time);
        camScript.gameObject.SetActive(false);
        waitingForCameraDeactivation = false;
    }

    private void OnDrawGizmos()
    {
        if (swinger != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(swinger.transform.position, -swinger.transform.forward);
        }

    }
}
