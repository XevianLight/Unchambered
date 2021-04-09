using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementHandler : MonoBehaviour
{
    public Rigidbody rb;
    public string ForwardKey = "w";
    public string LeftStrafeKey = "a";
    public string RightStrafeKey = "d";
    public string BackpedalKey = "s";
    public string JumpKey = "space";
    public string SprintKey = "leftshift";
    public float jumpForce = 10f;
    public float friction = 10f;
    public bool IsGrounded = false;
    public float speed = 3f;
    float maxSpeed = 100.0f;
    public float force;
    public float minVel = 0.001f;
    float xVel;
    float yVel;
    float zVel;
    public Vector3 vel;
    public float zoomSpeed = 0.1f;
    public Vector3 gravity = Physics.gravity;
    public MouseLook ml;
    public ConstantForce cf;
    public GameObject axis;
    Quaternion upRotation = Quaternion.identity;
    bool enter;
    public Camera[] cameras;
    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = speed;
        //ml = GetComponent<MouseLook>();
        //force = maxSpeed * 2;
    }

    // Update is called once per frame
    void Update()
    {
        //cf.relativeForce = gravity;
        //axis.transform.rotation = Quaternion.Lerp(axis.transform.rotation, upRotation, Time.deltaTime * 10);
        //force = maxSpeed * 2;
        zVel = transform.InverseTransformDirection(rb.velocity).z;
        xVel = transform.InverseTransformDirection(rb.velocity).x;
        vel = new Vector3((Mathf.RoundToInt(xVel * 10000)) / 100, 0, (Mathf.RoundToInt(zVel * 10000)) / 100);
        if (Input.GetKey(SprintKey))
        {
            maxSpeed = speed * 2;
        }
        else
        {
            maxSpeed = speed;
        }
        if (Mathf.Abs(rb.velocity.x) <= minVel && Mathf.Abs(rb.velocity.z) <= minVel)
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        if (/*Mathf.Abs(xVel) + Mathf.Abs(zVel) <= maxSpeed && */IsGrounded)
        {
            if (Input.GetKey(ForwardKey) && Mathf.Abs(zVel) <= maxSpeed)
            {
                rb.AddRelativeForce(Vector3.forward * force);
            }
            else if (!Input.GetKey(BackpedalKey)/* && Mathf.Abs(zVel) > minVel*/)
            {
                rb.AddRelativeForce(new Vector3(0, 0, -friction) * zVel);
            }

            if (Input.GetKey(LeftStrafeKey) && Mathf.Abs(xVel) <= maxSpeed)
            {
                rb.AddRelativeForce(Vector3.left * force);
            }
            else if (!Input.GetKey(RightStrafeKey)/* && Mathf.Abs(xVel) > minVel*/)
            {
                rb.AddRelativeForce(new Vector3(-friction, 0, 0) * xVel);
            }

            if (Input.GetKey(RightStrafeKey) && Mathf.Abs(xVel) <= maxSpeed)
            {
                rb.AddRelativeForce(Vector3.right * force);
            }
            else if (!Input.GetKey(LeftStrafeKey)/* && Mathf.Abs(xVel) > minVel*/)
            {
                rb.AddRelativeForce(new Vector3(-friction, 0, 0) * xVel);
            }

            if (Input.GetKey(BackpedalKey) && Mathf.Abs(zVel) <= maxSpeed)
            {
                rb.AddRelativeForce(Vector3.back * force);
            }
            else if (!Input.GetKey(ForwardKey)/* && Mathf.Abs(zVel) > minVel*/)
            {
                rb.AddRelativeForce(new Vector3(0, 0, -friction) * zVel);
            }
            if (Input.GetKeyDown(JumpKey) && IsGrounded)
            {
                IsGrounded = false;
                rb.velocity = rb.velocity + (transform.up * jumpForce);
            }
        }
        if (Input.GetKey("c"))
        {
            foreach (Camera c in cameras)
            {
                c.fieldOfView = Mathf.Lerp(c.fieldOfView, 20, zoomSpeed * Time.deltaTime);
            }
        }
        else
        {
            foreach (Camera c in cameras)
            {
                c.fieldOfView = Mathf.Lerp(c.fieldOfView, 80, zoomSpeed * Time.deltaTime);
            }
        }
        //rb.velocity = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0, rb.velocity.z), maxSpeed);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer != 6)
            IsGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != 6)
            IsGrounded = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Surface")    
        {                              
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Surface")
        {
            gravity = other.transform.up * -9.8f;
            //ml.upRotation = other.transform.eulerAngles;
            //upRotation = other.transform.rotation;
            if (other.bounds.Contains(transform.position))
            {
                if (!enter)
                {
                    //var relVelocity = transform.TransformDirection(new Vector3(-rb.velocity.x, rb.velocity.y, -rb.velocity.z));
                    //rb.velocity = other.transform.TransformDirection(relVelocity);
                    transform.parent = other.transform;
                    ml.tempRotation = other.transform.rotation;
                    ml.rotation.y -= other.transform.eulerAngles.y;
                    enter = true;
                }
            }
            else
            {
                if (enter)
                {
                    //var relVelocity = transform.InverseTransformDirection(new Vector3(-rb.velocity.x, rb.velocity.y, -rb.velocity.z));
                    //rb.velocity = other.transform.TransformDirection(relVelocity);
                    gravity = Physics.gravity;
                    //ml.upRotation = Vector3.up;
                    //upRotation = Quaternion.identity;
                    ml.tempRotation = other.transform.rotation;
                    ml.rotation.y += other.transform.eulerAngles.y;
                    transform.parent = null;
                    enter = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Surface")
        {
        }
    }
}
