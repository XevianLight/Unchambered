using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarEmmitter : MonoBehaviour
{

    public float speed = 1f;
    public GameObject origin;
    public float max = 10f;
    public float maxDestroy = 20f;
    bool canMakeNew = true;
    public GameObject sphere;
    float t = 0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        if (t >= max)
        {
            t = 0;
            Instantiate(sphere, origin.transform.position, Quaternion.identity);
        }
    }
}
