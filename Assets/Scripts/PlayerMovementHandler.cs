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
    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = speed;
	    //force = maxSpeed * 2;
    }

    // Update is called once per frame
	void FixedUpdate()
    {
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
            if (Input.GetKey(JumpKey))
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                IsGrounded = false;
            }
        }
        if (Input.GetKey("c"))
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 20, zoomSpeed * Time.deltaTime);
        }
        else
        {
	        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80, zoomSpeed * Time.deltaTime);
        }
        //rb.velocity = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0, rb.velocity.z), maxSpeed);
    }

    private void OnCollisionStay(Collision collision)
    {
        IsGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        IsGrounded = false;
    }
}
