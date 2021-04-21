using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{

    public float maxDestroy = 20f;
    public float speed = 1f;
    float t = 0f;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        transform.localScale += new Vector3(speed, speed, speed) * Time.deltaTime;
        if (t > maxDestroy)
        {
            Destroy(gameObject);
        }
    }
}
