using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {  
   [SerializeField, Range(-89f, 89f)]
    private float minVerticalAngle = -40f, maxVerticalAngle = 50f;
     [SerializeField, Range(1.2f, 1.8f)]
    float cameraHeight = 1.5f;
    [SerializeField, Min(0f)]
    private float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)]
    private float focusCentering = 0.75f;
    [SerializeField, Range(0.7f, 0.9f)]
    private float distance = 0.8f;

    [SerializeField]
    private float inputSensitivity = 150.0f;  
    [SerializeField]
    private float cameraSmoothSpeed = 0.3f;
    [SerializeField]
    private Vector3 cameraOffset = new Vector3(0.45f, 0f, 1f);
    [SerializeField]
    private float facingDistance = 0f;
    private Transform Parent;
    private Transform playerAnchor;
    private MovementInput characterInput;
    private Vector3 cameraVelocitySmoothDamp = Vector3.zero;
    private Vector3 focusPoint, previousFocusPoint;
    private Vector2 orbitAngles = new Vector2(0f, 0f);
    private float finalInputX;
    private float finalInputZ;
    private float mouseX = 0;
    private float mouseY = 0;

    void OnValidate() {
        if (maxVerticalAngle < minVerticalAngle)
            maxVerticalAngle = minVerticalAngle;    
    }

    // Start is called before the first frame update
    void Start() {
        characterInput = GameObject.FindWithTag("Player").GetComponent<MovementInput>();
        playerAnchor = GameObject.FindWithTag("PlayerCameraAnchor").GetComponent<Transform>();

        Parent = this.transform.parent;
    }


    // Update is called once per frame
    void Update() {
         if (ManualRotation()) {
            ConstrainAngles();
        }
    }

    void LateUpdate() {
        if (facingDistance < 0.01f)
            facingDistance = 0f;
        
        facingDistance = Mathf.Lerp(facingDistance, characterInput.CharacterFacing, Time.deltaTime);
        UpdateFocusPoint();
        Vector3 targetPosition = Vector3.zero;        
        Quaternion rotation = Quaternion.Euler(orbitAngles.x, orbitAngles.y, 0f);
        Vector3 lookDirection = rotation * cameraOffset;
        targetPosition = (focusPoint) - lookDirection * (distance + facingDistance);
        
        SmoothPosition(Parent.position, targetPosition, rotation);
    }

    void SmoothPosition(Vector3 fromPos, Vector3 toPos, Quaternion rotation) {
        Vector3 newPos = Vector3.SmoothDamp(fromPos, toPos, ref cameraVelocitySmoothDamp, cameraSmoothSpeed, 9f, Time.deltaTime);
        Parent.SetPositionAndRotation(newPos, rotation);
    }
    
    void UpdateFocusPoint() {
        Vector3 targetPoint = playerAnchor.position + (playerAnchor.up * cameraHeight);
        if (focusRadius > 0f) {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            if (distance > focusRadius) {
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, (focusRadius/distance));
            }
            if (distance > 0.01f && focusCentering > 0f) {
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, 
                                Mathf.Pow(1f - focusCentering, Time.deltaTime));
            } 
        } else {
                focusPoint = targetPoint;
        }
    }

    bool ManualRotation() {
        float inputX = Input.GetAxis("RightStickHorizontal");
        float inputZ = Input.GetAxis("RightStickVertical");

        //mouseX = Input.GetAxis("Mouse X");
        //mouseY = Input.GetAxis("Mouse Y");

        finalInputX = inputX + mouseX;
        finalInputZ = inputZ + mouseY;
       
        const float e = 0.1f;
        if (inputX < -e || inputX > e || inputZ < -e || inputZ > e) {
            orbitAngles.y += finalInputX * inputSensitivity * Time.deltaTime;
            orbitAngles.x += finalInputZ * inputSensitivity * Time.deltaTime; 
            return true;
        }
        return false;
    }
    
     void ConstrainAngles() {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
            orbitAngles.y += 360f;
        else if (orbitAngles.y > 360f)
            orbitAngles.y -= 360f;
    }

    public Quaternion LookRotation {
        get {
            return Quaternion.Euler(orbitAngles.x, orbitAngles.y, 0.0f);
        }
    }

    private Vector3 CameraHalfExtends {
        get {
            Vector3 halfExtends;
            halfExtends.y = Camera.main.nearClipPlane * 
                            Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView);
            halfExtends.x = halfExtends.y * Camera.main.aspect;
            halfExtends.z = 0f;

            return halfExtends;
    }}
    /*
    private Vector3 CheckCollision(ref Vector3 lookDir, ref Vector3 lookPos) {
        Vector3 rectOffset = lookDir * Camera.main.nearClipPlane;
        Vector3 rectPosition = lookPos + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        return rectPosition;
    }
    */

}
