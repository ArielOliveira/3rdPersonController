using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MovementInput : MonoBehaviour {
    #region Variables

    //PlayerInput
    [SerializeField]
    private Vector2 input;
    private Vector3 desiredMoveDirection;
    public Animator animator;

    //Physics
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float acceleration = 3f;
    [SerializeField]
    private float turnSpeed = 0f;
    [SerializeField]
    private float turnSpeedLow = 5f;
    [SerializeField]
    private float turnSpeedHigh = 20;
    private float verticalSpeed;
    [SerializeField]
    private float currentSpeed;

    //Gravity
    public float gravity = 10;
    bool grounded = false;
    public LayerMask layer;
    

    //Camera
    public Camera camera;
    Vector3 forward, right;

    States currentState;

    //LookAt Input Response
    [SerializeField]
    Vector3 torsoDirectionTarget, currentTorsoDirection;

    public enum States {
        Free,
        Aiming
    }

    #endregion

    #region Initialization
    // Start is called before the first frame update
    void Start() {
        animator = this.GetComponent<Animator>();
        camera = Camera.main;
        currentState = States.Free;
    }

    void FixedUpdate() {
        AdjustLookAtTarget(ref currentTorsoDirection, ref torsoDirectionTarget, HumanBodyBones.Head);
        Debug.DrawRay(currentTorsoDirection, desiredMoveDirection, Color.green);
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetAxis("Aim") < -0.1f) {
            currentState = States.Aiming;
            animator.SetBool("IsAiming", true);
        } else {
            currentState = States.Free;
            animator.SetBool("IsAiming", false);
        }

        CalculateCamera();
        PlayerInput();
        CalculateMovement(); 
    }

    void PlayerInput() {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        animator.SetFloat("InputX", input.x, 0.3f, Time.deltaTime);
        animator.SetFloat("InputY", input.y, 0.3f, Time.deltaTime);
        input = Vector2.ClampMagnitude(input, 1);
        animator.SetFloat("InputMagnitude", input.magnitude);
    }

    void CalculateMovement() {
        desiredMoveDirection = forward * input.y + right * input.x;
        currentSpeed = Mathf.Lerp(currentSpeed, input.magnitude*speed, acceleration*Time.deltaTime);
        if (currentSpeed < 0.01f)
            currentSpeed = 0f;
        float tS = currentSpeed/speed;
        turnSpeed = Mathf.Lerp(turnSpeedHigh, turnSpeedLow, tS);

        animator.SetFloat("Speed", currentSpeed);

        if (input.magnitude > 0f && currentState != States.Aiming) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), turnSpeed*Time.deltaTime);
        }
    }

    void OnAnimatorIK(int layerIndex) {
        if (currentState == States.Free) {
            animator.SetLookAtWeight(input.magnitude, input.magnitude);
            animator.SetLookAtPosition(torsoDirectionTarget);
        }
    }

    void CalculateCamera() {
        forward = camera.transform.forward;
        right = camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();
    }

    void AdjustLookAtTarget(ref Vector3 torsoPosition, ref Vector3 torsoTarget, HumanBodyBones torso) {
        torsoPosition = animator.GetBoneTransform(torso).position;
        torsoTarget = torsoPosition + transform.forward + desiredMoveDirection;

    }

    public float CharacterFacing {
        get {   
                float facingAngle = Vector3.Dot(desiredMoveDirection.normalized, forward);
                return facingAngle < 0 ? -facingAngle : 0f;
            }
                }

    #endregion
}

   
