using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandler : MonoBehaviour
{
    public GameObject LeftFootNode;
    public GameObject RightFootNode;
    public GameObject LeftHandNode;
    public GameObject RightHandNode;
    public Vector3 HandPositionOnSwingR;
    public Vector3 HandPositionOnSwingL;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool isSwinging = false;


    private void OnAnimatorIK(int layerIndex)
    {
        if (isSwinging)
        {
            if (HandPositionOnSwingR != null)
            {
                this.GetComponent<Animator>().SetIKPosition(AvatarIKGoal.RightHand, HandPositionOnSwingR);
                this.GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            }

            if (HandPositionOnSwingL != null)
            {
                this.GetComponent<Animator>().SetIKPosition(AvatarIKGoal.LeftHand, HandPositionOnSwingL);
                this.GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);

            }

        }
        else
        {
           // HandleWalkingFootIK();
        }
        //if (HandPositionOnSwingR != null)
        //{
        //    this.GetComponent<Animator>().SetIKPosition(AvatarIKGoal.RightHand, HandPositionOnSwingR.transform.position);
        //    this.GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.RightHand, 1);

        //}

        //if (HandPositionOnSwingL != null)
        //{
        //    this.GetComponent<Animator>().SetIKPosition(AvatarIKGoal.LeftHand, HandPositionOnSwingL.transform.position);
        //    this.GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        //}

    }


    private void HandleWalkingFootIK()
    {
        RaycastHit hitLeft;
        RaycastHit hitRight;
        float threshold = .1f;

        if (Physics.Raycast((LeftFootNode.transform.position), -Vector3.up, out hitLeft, 1f))
        {
            if (hitLeft.distance < threshold)
            {
                this.GetComponent<Animator>().SetIKPosition(AvatarIKGoal.LeftFoot, hitLeft.point);
                this.GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                this.GetComponent<Animator>().SetIKRotation(AvatarIKGoal.LeftFoot, hitLeft.transform.rotation);
                this.GetComponent<Animator>().SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            }
        }

        if (Physics.Raycast((RightFootNode.transform.position), -Vector3.up, out hitRight, 1f))
        {
            if (hitRight.distance < threshold)
            {
                this.GetComponent<Animator>().SetIKPosition(AvatarIKGoal.RightFoot, hitRight.point);
                this.GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            }
        }

        Debug.DrawRay((RightFootNode.transform.position), -Vector3.up);
        Debug.DrawRay((LeftFootNode.transform.position), -Vector3.up);

    }
}
