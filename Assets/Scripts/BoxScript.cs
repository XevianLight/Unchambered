using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScript : MonoBehaviour
{
    public Mesh insideMesh;
    public Mesh outsideMesh;
    public Material insideMat;
    public Material outsideMat;
	public bool occupied = false;
    MeshFilter meshF;
    MeshRenderer meshR;
    public int occupants = 0;


    // Start is called before the first frame update
    void Start()
    {
        meshF = gameObject.GetComponent<MeshFilter>();
        meshR = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
	{
		if (transform.childCount <= 0){
			occupied = false;
		}
    	
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            meshF.mesh = insideMesh;
            meshR.material = insideMat;
            /*if (transform.parent != null)
            {
                transform.parent.GetComponent<CubeScript>().colliderCount = transform.parent.GetComponent<CubeScript>().initialColliderCount + other.GetComponent<CubeScript>().colliderCount;
            }*/
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            meshF.mesh = outsideMesh;
            meshR.material = outsideMat;
        }
    }
}
