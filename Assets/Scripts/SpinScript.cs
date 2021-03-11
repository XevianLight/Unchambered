using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinScript : MonoBehaviour
{
	
	public Vector3 speed = Vector3.one;
	LensFlare lf;
	Renderer r;
	public float flareBrightnessModifier = 1f;
	
    // Start is called before the first frame update
    void Start()
    {
	    lf = gameObject.GetComponent<LensFlare>();
	    r = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
	{
		//speed.x = Mathf.Lerp(speed.x, speed.x + Random.Range(-2f,2f) * 10f, Time.deltaTime);
		//if(speed.x < 100f){
		//	speed.x = speed.x + 0.1f;
		//}else{
		//	speed.x = -100f;
		//}
		transform.RotateAround(transform.position, transform.right, speed.x);
	    transform.RotateAround(transform.position, Vector3.up, speed.y);
		transform.RotateAround(transform.position, Vector3.forward, speed.z);
		//if (gameObject.GetComponent<Renderer>().isVisible){
		//	lf.brightness = transform.lossyScale.magnitude / Vector3.Distance(transform.position, Camera.main.transform.position) * 5;
		//}else{
		//	lf.brightness = 0;
		//}
		if(r.isVisible) //Check if Camera is turned towards the GameObject first
		{
		   RaycastHit hit;
		   // Calculate Ray direction
		   Vector3 direction = Camera.main.transform.position - transform.position; 
			if(Physics.Raycast(transform.position, direction, out hit, 20, ~((1 << 14))))
			{
				//Debug.Log(hit.transform.name);
				if(hit.collider.tag != "Player") //hit something else before the camera
			   {
					//lf.brightness = Mathf.Lerp(lf.brightness, 0, Time.deltaTime * lf.fadeSpeed);
					//Debug.Log ("visible");
				   //do something here
			   }else{
				//    lf.brightness = Mathf.Lerp(lf.brightness, (transform.lossyScale.magnitude / Vector3.Distance(transform.position, Camera.main.transform.position) * flareBrightnessModifier), Time.deltaTime * lf.fadeSpeed);
			   	
			   }
		   }
		}
		//transform.eulerAngles = new Vector3(Random.Range(-5f,5f), transform.eulerAngles.y, Random.Range(-5f,5f));
    }
}
