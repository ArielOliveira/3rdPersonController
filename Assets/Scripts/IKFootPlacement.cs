using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKFootPlacement : MonoBehaviour {
    [Range(0, 1)]
    public float distanceToGround;

    public string LeftFootCurve = "LeftFootCurve";
    public string RightFootCurve = "RightFootCurve";

    public LayerMask layerMask;

    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnAnimatorIK(int layerIndex) {
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(LeftFootCurve));
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, animator.GetFloat(RightFootCurve));
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(LeftFootCurve));
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(RightFootCurve));

        IKFootPositionSolver(AvatarIKGoal.LeftFoot);
        IKFootPositionSolver(AvatarIKGoal.RightFoot);
    }

    void IKFootPositionSolver(AvatarIKGoal foot) {
        RaycastHit hit;

        Ray ray = new Ray(animator.GetIKPosition(foot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out hit, distanceToGround + 1.5f, layerMask)) {
            Vector3 footPosition = hit.point;
            footPosition.y += distanceToGround;
            animator.SetIKPosition(foot, footPosition);
            animator.SetIKRotation(foot, Quaternion.LookRotation(transform.forward, hit.normal));
        }
    }
}
