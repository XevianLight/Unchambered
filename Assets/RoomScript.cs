using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScript : MonoBehaviour
{

    public GameObject cube;
    public float ratio = 1f;
    public float portalSize = 1f;
    public Portal roomPortal;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.localScale = cube.transform.lossyScale * ratio / portalSize;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<CubeScript>())
        {
            if (roomPortal)
            {
                other.GetComponent<CubeScript>().roomPortal = roomPortal;
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CubeScript>())
        {
            if (roomPortal)
            {
                other.GetComponent<CubeScript>().roomPortal = null;
            }
        }
    }
}
