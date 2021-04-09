using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ForceField : MonoBehaviour
{

    private SphereCollider col;
    public bool gravity = true;
    public bool time = false;
    public bool force = false;
    public float timeSpeed = 2f;
    public float targetTime = 0.1f;
    public bool scaleTime = false;
    public float timeProgress = 0f;
    public float timeScale = 1f;
    public float forceMagnitude = 10f;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (time)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            Debug.Log(Time.timeScale);
            timeProgress = Mathf.Clamp(timeProgress, 0, 1);
            if (scaleTime)
            {
                Debug.Log("slowdown");
                timeScale = Mathf.Lerp(1, targetTime, timeProgress);
                timeProgress += Time.unscaledDeltaTime;
                timeProgress = Mathf.Clamp(timeProgress, 0, 1);
            }
            else
            {
                Debug.Log("speedup");
                timeScale = Mathf.Lerp(1, targetTime, timeProgress);
                timeProgress -= Time.unscaledDeltaTime;
                timeProgress = Mathf.Clamp(timeProgress, 0, 1);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody)
        {
            if (Vector3.Distance(transform.InverseTransformPoint(transform.position), transform.InverseTransformPoint(other.transform.position)) <= col.radius)
            {
                if (gravity)
                {

                    if (other.GetComponent<ConstantForce>())
                    {
                        other.GetComponent<ConstantForce>().enabled = false; ;
                    }
                    else
                    {
                        if (other.name != "Capsule")
                            other.GetComponent<Rigidbody>().useGravity = false;
                    }

                }
                if (time)
                {

                    if (other.name == "Capsule")
                    {
                        scaleTime = true;
                    }
                }
                if (force)
                {
                    if (other.name != "Capsule")
                    {
                        if (other.GetComponent<Rigidbody>())
                        {
                            other.GetComponent<Rigidbody>().AddForce((transform.position - other.transform.position) * forceMagnitude / transform.lossyScale.x);
                        }
                    }
                }
            }
            else
            {
                if (gravity)
                {
                    if (other.attachedRigidbody != null)
                    {
                        if (other.GetComponent<ConstantForce>())
                        {
                            other.GetComponent<ConstantForce>().enabled = true;
                        }
                        else
                        {
                            if (other.name != "Capsule")
                                other.GetComponent<Rigidbody>().useGravity = true;
                        }
                    }
                }
                if (time)
                {

                    if (other.name == "Capsule")
                    {
                        scaleTime = false;
                    }
                }
                if (force)
                {
                    if (other.name != "Capsule")
                    {
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (gravity)
        {
            if (other.attachedRigidbody != null)
            {
                if (other.GetComponent<ConstantForce>())
                {
                    other.GetComponent<ConstantForce>().enabled = true;
                }
                else
                {
                    if (other.name != "Capsule")
                        other.GetComponent<Rigidbody>().useGravity = true;
                }
            }
        }
        if (time)
        {
            if (other.name == "Capsule")
            {
                scaleTime = false;
            }
        }
    }
}
