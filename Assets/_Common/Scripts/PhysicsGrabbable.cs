using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Copyright © Facebook Technologies, LLC and
//its affiliates. All rights reserved.
public class PhysicsGrabbable : OVRGrabbable
{
    [SerializeField]  bool releasable;
    [SerializeField]  float acceptableDistance = 0.1f;
    
    GameObject anchor,anchorL,anchorR; 
    GameObject grabHand,grabHandL,grabHandR;
    Rigidbody rb;
    Vector3 offsetPos; 
    Vector3 thisObjPos;

    Vector3 tmpGripTrans;

    bool isCollision;
    bool grabMomentKinematic;
    

    protected override void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        
        grabHandL = GameObject.Find("hands:Lhand");
        anchorL = GameObject.Find("LeftControllerAnchor");
        
        grabHandR = GameObject.Find("hands:Rhand");
        anchorR = GameObject.Find("RightControllerAnchor");
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        m_grabbedBy = hand;
        
        if (m_grabbedBy.name =="CustomHandLeft")
        {
            if(grabHandL!=null) grabHand = grabHandL;
            if(anchorL!=null) anchor = anchorL;
        }
        else if (m_grabbedBy.name =="CustomHandRight")
        {
            if(grabHandR!=null) grabHand = grabHandR;
            if(anchorR!=null) anchor = anchorR;
        }
        
        m_grabbedCollider = grabPoint;
        grabHand.SetActive(false);
        this.gameObject.transform.parent = anchor.transform;
        tmpGripTrans = this.gameObject.transform.localPosition;
        rb.useGravity = false;
        grabMomentKinematic = rb.isKinematic;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        grabHand.SetActive(true);
        this.gameObject.transform.parent = null;
        rb.useGravity = true;
        rb.isKinematic = grabMomentKinematic;
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = linearVelocity;
        rb.angularVelocity = angularVelocity;
        m_grabbedBy = null;
        m_grabbedCollider = null;
    }

    void OnCollisionEnter(Collision other)
    {
        isCollision = true;
    }

    void OnCollisionExit(Collision other)
    {
        isCollision = false;
    }
   
    void Update()
    {
        if (isGrabbed)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            offsetPos = anchor.transform.position;
            thisObjPos = this.gameObject.transform.position;
            
            if (isCollision == false && Vector3.Distance(thisObjPos,offsetPos)>acceptableDistance)
            {
                this.gameObject.transform.localPosition = tmpGripTrans;
            }
            
            if (releasable && Vector3.Distance(thisObjPos, offsetPos) > acceptableDistance)
            {
                m_grabbedBy.ForceRelease(this.gameObject.GetComponent<PhysicsGrabbable>());
            }
        }
    }
}
