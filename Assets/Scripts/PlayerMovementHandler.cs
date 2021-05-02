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
    //public Vector3 gravity = Physics.gravity;
    public MouseLook ml;
    public ConstantForce cf;
    public GameObject axis;
    Quaternion upRotation = Quaternion.identity;
    bool enter;
    public Camera[] cameras;

    List<Vector4> conveyorVectors = new List<Vector4>();
    List<Vector3> conveyorNormals = new List<Vector3>();

    //public float speed = 10.0f;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;
    private bool grounded = false;
    public ForceMode forceMode;
    public ForceMode conveyorForceMode;

    public float conveyorStickForce = 10f;

    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = speed;
        //ml = GetComponent<MouseLook>();
        //force = maxSpeed * 2;
    }
    // Update is called once per frame
    //void Update()
    //{
    //    //cf.relativeForce = gravity;
    //    //axis.transform.rotation = Quaternion.Lerp(axis.transform.rotation, upRotation, Time.deltaTime * 10);
    //    //force = maxSpeed * 2;
    //    zVel = transform.InverseTransformDirection(rb.velocity).z;
    //    xVel = transform.InverseTransformDirection(rb.velocity).x;
    //    vel = new Vector3((Mathf.RoundToInt(xVel * 10000)) / 100, 0, (Mathf.RoundToInt(zVel * 10000)) / 100);
    //    if (Input.GetKey(SprintKey))
    //    {
    //        maxSpeed = speed * 2;
    //    }
    //    else
    //    {
    //        maxSpeed = speed;
    //    }
    //    if (Mathf.Abs(rb.velocity.x) <= minVel && Mathf.Abs(rb.velocity.z) <= minVel)
    //        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    //    if (/*Mathf.Abs(xVel) + Mathf.Abs(zVel) <= maxSpeed && */IsGrounded)
    //    {
    //        if (Input.GetKey(ForwardKey) && Mathf.Abs(zVel) <= maxSpeed)
    //        {
    //            rb.velocity = (rb.velocity + transform.forward * force);
    //        }
    //        else if (!Input.GetKey(BackpedalKey)/* && Mathf.Abs(zVel) > minVel*/)
    //        {
    //            //rb.AddRelativeForce(new Vector3(0, 0, -friction) * zVel);
    //        }

    //        if (Input.GetKey(LeftStrafeKey) && Mathf.Abs(xVel) <= maxSpeed)
    //        {
    //            rb.velocity = (rb.velocity - transform.right * force);
    //        }
    //        else if (!Input.GetKey(RightStrafeKey)/* && Mathf.Abs(xVel) > minVel*/)
    //        {
    //            //rb.AddRelativeForce(new Vector3(-friction, 0, 0) * xVel);
    //        }

    //        if (Input.GetKey(RightStrafeKey) && Mathf.Abs(xVel) <= maxSpeed)
    //        {
    //            rb.velocity = (rb.velocity + transform.right * force);
    //        }
    //        else if (!Input.GetKey(LeftStrafeKey)/* && Mathf.Abs(xVel) > minVel*/)
    //        {
    //            //rb.AddRelativeForce(new Vector3(-friction, 0, 0) * xVel);
    //        }

    //        if (Input.GetKey(BackpedalKey) && Mathf.Abs(zVel) <= maxSpeed)
    //        {
    //            rb.velocity = (rb.velocity - transform.forward * force);
    //        }
    //        else if (!Input.GetKey(ForwardKey)/* && Mathf.Abs(zVel) > minVel*/)
    //        {
    //            //rb.AddRelativeForce(new Vector3(0, 0, -friction) * zVel);
    //        }
    //        if (Input.GetKeyDown(JumpKey) && IsGrounded)
    //        {
    //            IsGrounded = false;
    //            rb.velocity = rb.velocity + (transform.up * jumpForce);
    //        }
    //    }
    //    if (Input.GetKey("c"))
    //    {
    //        foreach (Camera c in cameras)
    //        {
    //            c.fieldOfView = Mathf.Lerp(c.fieldOfView, 20, zoomSpeed * Time.deltaTime);
    //        }
    //    }
    //    else
    //    {
    //        foreach (Camera c in cameras)
    //        {
    //            c.fieldOfView = Mathf.Lerp(c.fieldOfView, 80, zoomSpeed * Time.deltaTime);
    //        }
    //    }
    //    //rb.velocity = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0, rb.velocity.z), maxSpeed);
    //}

    void FixedUpdate()
    {
        if (grounded)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")), 1);
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= speed;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            rb.AddForce(velocityChange, forceMode);

            // Jump
            if (canJump && Input.GetButton("Jump"))
            {
                rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            }

            foreach (Vector4 c in conveyorVectors)
            {
                rb.AddForce(c.ToVector3() * c.w / Time.deltaTime, conveyorForceMode);
            }

            if (canJump && !Input.GetButton("Jump"))
            {
                foreach (Vector3 c in conveyorNormals)
                {
                    rb.AddForce(c * conveyorStickForce);
                }
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

        // We apply gravity manually for more tuning control
        //rb.AddForce(new Vector3(0, -gravity * rb.mass, 0));

        grounded = false;


    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Conveyor") && !conveyorVectors.Contains(collision.transform.forward.AppendW(collision.transform.GetComponent<Conveyor>().speed)))
        {
            //conveyorVectors.Add(new Vector4(collision.transform.forward.x, collision.transform.forward.y, collision.transform.forward.z, collision.transform.GetComponent<Conveyor>().speed));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer != 6)
            grounded = true;
        if (collision.transform.CompareTag("Conveyor") && !conveyorVectors.Contains(collision.transform.forward.AppendW(collision.transform.GetComponent<Conveyor>().speed)))
        {
            conveyorVectors.Add(collision.transform.forward.AppendW(collision.transform.GetComponent<Conveyor>().speed));
        }
        if (collision.transform.CompareTag("Conveyor") && !conveyorNormals.Contains(-collision.transform.up))
        {
            conveyorNormals.Add(-collision.transform.up);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != 6)
            grounded = false;
        if (collision.transform.CompareTag("Conveyor") && conveyorVectors.Contains(collision.transform.forward.AppendW(collision.transform.GetComponent<Conveyor>().speed)))
        {
            conveyorVectors.Remove(collision.transform.forward.AppendW(collision.transform.GetComponent<Conveyor>().speed));
        }
        if (collision.transform.CompareTag("Conveyor") && conveyorNormals.Contains(-collision.transform.up))
        {
            conveyorNormals.Remove(-collision.transform.up);
        }
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
            //gravity = other.transform.up * -9.8f;
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
                    //gravity = Physics.gravity;
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

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

}

public static class ExtensionMethods
{
    public static Vector3 ToVector3(this Vector4 parent)
    {
        return new Vector3(parent.x, parent.y, parent.z);
    }

    public static Vector4 AppendW(this Vector3 parent, float w)
    {
        return new Vector4(parent.x, parent.y, parent.z, w);
    }
}
