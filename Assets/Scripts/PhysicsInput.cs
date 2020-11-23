using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsInput : MonoBehaviour
{
    private Vector3 forceInput;
    private Rigidbody characterBody;
    // Start is called before the first frame update
    void Start() {
        characterBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        characterBody.AddForce(forceInput, ForceMode.Force);
    }

    // Update is called once per frame
    void Update() {
        forceInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
    }
}
