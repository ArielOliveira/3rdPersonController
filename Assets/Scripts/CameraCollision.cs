using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour {
    public float minDistance = 1.0f;
    public float maxDistance = 4.0f;

    public float smooth = 10.0f;
    public float distance;

    Vector3 dollyDir;
    public Vector3 dollyDirAdjusted;

    void Awake() {
        dollyDir = transform.position.normalized;
        distance = transform.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update() {
        Vector3 desiredCameraPosition = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;
        if (Physics.Linecast(transform.parent.position, desiredCameraPosition, out hit)) {
            distance = Mathf.Clamp((hit.distance * 0.8f), minDistance, maxDistance);
        } else {
            distance = maxDistance;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
    }
}
