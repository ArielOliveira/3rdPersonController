using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private Transform playerFollow;
    private CameraFollow cam;
    [SerializeField]
    private Vector3 offSet = new Vector3(0f, 0f, -1f);

    [SerializeField]   
    float aimHeight = 1.5f;
    [SerializeField, Range(0.5f, 1f)]
    float aimDistance = 0.8f;
    // Start is called before the first frame update
    void Start()
    {
        playerFollow = GameObject.FindWithTag("Player").GetComponent<Transform>();
        cam = GameObject.FindWithTag("MainCamera").GetComponent<CameraFollow>();
    }

    // Update is called once per frame
    void LateUpdate() {
        Vector3 lookDirection = cam.LookRotation * offSet;
        transform.position = playerFollow.position + (Vector3.up * aimHeight) - lookDirection * aimDistance; //- new Vector3(0f, 0f, transform.position.y);
    }
}
