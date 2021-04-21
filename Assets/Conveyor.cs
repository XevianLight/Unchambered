using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{

    Rigidbody rb;
    public float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos = rb.position;
        rb.position += transform.forward * -speed * Time.fixedDeltaTime;
        rb.MovePosition(pos);
    }
}
