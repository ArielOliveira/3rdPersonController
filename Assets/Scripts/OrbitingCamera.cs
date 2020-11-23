using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitingCamera : MonoBehaviour
{
    [SerializeField]
    private Transform focus = default;
    [SerializeField, Range(1f, 20f)]
    private float distance = 5f;
    [SerializeField, Range(0f, 2f)]
    private float heightOffset = 0.8f;
    [SerializeField, Min(0f)]
    private float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)]
    private float focusCentering = 0.75f;
    [SerializeField, Range(1f, 360f)]
    private float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)]
    private float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    [SerializeField, Min(0f)]
    private float alignDelay = 5f;
    [SerializeField, Range(0f, 90f)]
    private float alignSmoothRange = 45f;
    [SerializeField]
    LayerMask obstructionMask = -1;
    [SerializeField, Range(0f, 2f)]
    private Vector3 focusPoint, previousFocusPoint;
    private Vector2 orbitAngles = new Vector2(10f, 0f);
    private Vector2 rotationInput = Vector2.zero;
    private Vector2 rotationVelocity = Vector2.zero;
    private float lastManualRotationTime;

    private Camera regularCamera;

    private Vector3 CameraHalfExtends {
        get {
            Vector3 halfExtends;
            halfExtends.y = regularCamera.nearClipPlane * 
                            Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;

            return halfExtends;
    }}

    void OnValidate() {
        if (maxVerticalAngle < minVerticalAngle)
            maxVerticalAngle = minVerticalAngle;    
    }
    void Awake() {
        focus.position = focus.position + Vector3.up * heightOffset;
        focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
        regularCamera = GetComponent<Camera>();
    }

    void LateUpdate() {
        UpdateFocusPoint();
        ManualRotation();
        Quaternion lookRotation;
        if (ManualRotation()) {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        } else {
            lookRotation = transform.localRotation;
        }

        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;

        Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        if (Physics.BoxCast(
            castFrom, CameraHalfExtends, -lookDirection, out RaycastHit hit,
            lookRotation, castDistance, obstructionMask
        )) {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }
            
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint() {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f) {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            if (distance > focusRadius) {
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, (focusRadius/distance));
            }
            if (distance > 0.01f && focusCentering > 0f) {
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, 
                                Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime));
            } 
        } else {
                focusPoint = targetPoint;
            }
    }

    bool ManualRotation() {
        rotationInput = Vector2.SmoothDamp(rotationInput, new Vector2(Input.GetAxis("RightStickVertical"), Input.GetAxis("RightStickHorizontal")), ref rotationVelocity, 0.4f, rotationSpeed);
        const float e = 0.1f;
        if (rotationInput.x < -e || rotationInput.x > e || rotationInput.y < -e || rotationInput.y > e) {
            orbitAngles += rotationInput;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    bool AutomaticRotation() {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay) {
            return false;
            }
            Vector2 movement = new Vector2(
                focusPoint.x - previousFocusPoint.x,
                focusPoint.y - previousFocusPoint.y
                );
            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.000001f)
                return false;
            
            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
            float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
            if (deltaAbs < alignSmoothRange) 
                rotationChange *= deltaAbs / alignSmoothRange;
            else if (180f - deltaAbs < alignSmoothRange) 
                rotationChange *= (180f - deltaAbs) / alignSmoothRange;

            orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);

        return true;
    }

    void ConstrainAngles() {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
            orbitAngles.y += 360f;
        else if (orbitAngles.y > 360f)
            orbitAngles.y -= 360f;
    }

    static float GetAngle (Vector2 direction) {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }

 }
