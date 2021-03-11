using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

public class CubeScript : MonoBehaviour
{
    Rigidbody rb;
    public Vector3 targetRotation = Vector3.zero;
    public Vector3 targetPosition = Vector3.zero;
    public float targetScale;
    public Vector3 initialScale;
    public float inverseScaleFactor = 1;
    public float lossyScale;
    public float snapAngle = 90f;
    public bool held = false;
    public bool snappingEnabled = false;
    public bool placeInArea = false;
    public bool insideSub = false;
    public GameObject areaObject;
    //public PostProcessVolume postProcessVolume;
    //private ChromaticAberration ca;
    float dampVel;
    public Light lt;
    public float lightModifier = 1f;
    Vector3 scale;
    public int occupants = 0;
    public int initialColliderCount = 1;
    public int colliderCount = 1;
    public bool childColliders = false;
    public float snapSpeed = 10f;
    public bool isLightParent = false;
    public GameObject colliderObject;
    public bool isParentHeld = false;
    public bool ignoreFlares = false;
    public bool hasPlayedSound = false;
    public AudioClip expand;
    AudioSource audioSource;
    public Collider[] connectedAreas;



    // Start is called before the first frame update
    void Start()
    {
        //postProcessVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        //postProcessVolume.profile.TryGetSettings(out ca);
        initialScale = transform.localScale;
        rb = gameObject.GetComponent<Rigidbody>();
        if (!childColliders)
            colliderObject = gameObject;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        lossyScale = transform.lossyScale.x * inverseScaleFactor;
        if (lt != null)
        {
            // lt.range = transform.lossyScale.magnitude * lightModifier;
            lt.intensity = transform.lossyScale.magnitude * lightModifier;
        }
        scale = transform.localScale;
        if (!held && snappingEnabled && placeInArea && areaObject != null)
        {
            if (!hasPlayedSound)
            {
                if (audioSource)
                {
                    if (expand)
                    {
                        audioSource.clip = expand;
                        audioSource.Play();
                        hasPlayedSound = true;
                    }
                }
            }
            areaObject.GetComponent<BoxScript>().occupied = true;
            transform.parent = areaObject.transform;
            rb.isKinematic = true;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, (Time.deltaTime * snapSpeed) / Vector3.Distance(transform.localScale, Vector3.one));
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(Mathf.Round(transform.localEulerAngles.x / snapAngle) * snapAngle, Mathf.Round(transform.localEulerAngles.y / snapAngle) * snapAngle, Mathf.Round(transform.localEulerAngles.z / snapAngle) * snapAngle), (Time.deltaTime * snapSpeed) / Vector3.Distance(transform.localScale, Vector3.one));
            transform.localScale = Vector3.MoveTowards(transform.localScale, new Vector3((Mathf.Round(transform.parent.transform.localScale.x * 100) / 100) / transform.parent.transform.localScale.x, (Mathf.Round(transform.parent.transform.localScale.y * 100) / 100) / transform.parent.transform.localScale.y, (Mathf.Round(transform.parent.transform.localScale.z * 100) / 100) / transform.parent.transform.localScale.z) * targetScale, (Time.deltaTime * snapSpeed) / Vector3.Distance(transform.localScale, Vector3.one));
            //Debug.Log(transform.localScale.magnitude);
            if (Mathf.Round(transform.localScale.magnitude) != 2)
            {
                areaObject.GetComponent<BoxScript>().forceOut = true;
                //ca.intensity.value = Mathf.SmoothDamp(ca.intensity, 0.5f, ref dampVel, 0.05f);
            }
            else
            {
                areaObject.GetComponent<BoxScript>().forceOut = false;
                //ca.intensity.value = Mathf.SmoothDamp(ca.intensity, 0f, ref dampVel, 0.5f);
            }
            //if (!childColliders)
            //{
            //    if (areaObject.transform.parent.name == "SubCube")
            //    {
            //	    if (isParentHeld){
            //		    gameObject.layer = 13;
            //	    }else{
            //		    gameObject.layer = 12;
            //	    }
            //	    insideSub = true;
            //    }
            //    else
            //    {
            //    	insideSub = false;
            //	    gameObject.layer = 0;
            //	    gameObject.tag = "Surface";
            //    }
            //}
            //else
            //{
            //    foreach (Transform t in transform)
            //    {
            //	    if (t.gameObject.name == "Collider")
            //	    {
            //		    t.tag = "Surface";
            //		    if (isParentHeld){
            //			    t.gameObject.layer = 13;
            //		    }else{
            //		    	t.gameObject.layer = 12;
            //		    }
            //		    insideSub = true;
            //	    }
            //    }
            //    if (areaObject.transform.parent != null){
            //	    if (areaObject.transform.parent.name == "SubCube")
            //	    {
            //		    if (isParentHeld){
            //			    gameObject.layer = 13;
            //		    }else{
            //		    	gameObject.layer = 12;
            //		    }
            //		    insideSub = true;
            //	    }
            //	    else
            //	    {
            //		    gameObject.layer = 0;
            //		    insideSub = false;
            //	    }
            //    }
            //}
        }
        else
        {
            rb.isKinematic = false;
            transform.localScale = Vector3.Lerp(transform.localScale, initialScale, (Time.deltaTime * snapSpeed) * 5);
            hasPlayedSound = false;
        }

        if (areaObject)
        {
            if (childColliders)
            { //If the object has a child which contains its compound collider
                foreach (Transform t in transform) //Set the layer of the collider object respectively
                {
                    if (t.gameObject.name == "Collider" && t.gameObject.name != "Portal")
                    {
                        t.tag = "Surface";
                        if (isParentHeld)
                        {
                            t.gameObject.layer = 13;
                        }
                        else
                        {
                            t.gameObject.layer = 12;
                        }
                        insideSub = true;
                    }
                }
                if (ignoreFlares)
                {
                    gameObject.layer = 14;
                }
                else
                {
                    gameObject.layer = 8;
                }
                gameObject.tag = "Surface";
            }
            else
            {
                tag = "Surface";
                if (isParentHeld)
                {
                    gameObject.layer = 13;
                }
                else
                {
                    gameObject.layer = 12;
                }
            }
            //if (areaObject.transform.parent){	//If the areaObject is part of a larger object, disable collision between this object and the larger object.
            //    isParentHeld = areaObject.transform.parent.GetComponent<CubeScript>().held;
            //    foreach (Collider c in areaObject.transform.parent.GetComponent<CubeScript>().colliderObject.GetComponents<Collider>()){
            //	    foreach (Collider c1 in colliderObject.GetComponents<Collider>()){
            //		    Physics.IgnoreCollision(c1, c, true);
            //	    }	   
            //    }
            //    if (areaObject.transform.parent.parent){
            //	    if (areaObject.transform.parent.parent.parent){
            //	    	foreach (Collider c in areaObject.transform.parent.parent.parent.GetComponent<CubeScript>().colliderObject.GetComponents<Collider>()){
            //			    foreach (Collider c1 in colliderObject.GetComponents<Collider>()){
            //				    Physics.IgnoreCollision(c1, c, true);
            //		    	}	 
            //	    	}  
            //    	}
            //	}
            //}
            if (areaObject.transform.parent)
            {
                if (areaObject.transform.parent.GetComponent<CubeScript>().held && !held)
                {
                    isParentHeld = true;
                }
                else
                {
                    isParentHeld = areaObject.transform.parent.GetComponent<CubeScript>().isParentHeld;
                }
                //isParentHeld = areaObject.transform.parent.GetComponent<CubeScript>().isParentHeld;
                foreach (Collider c in getParentColliders())
                {
                    foreach (Collider c1 in colliderObject.GetComponents<Collider>())
                    {
                        Physics.IgnoreCollision(c, c1, true);
                    }
                }
            }

        }
        if (held)
        {
            rb.isKinematic = true;
            isParentHeld = false;
            //if (areaObject && areaObject.name == "SubArea") {
            //    if (areaObject.transform.parent){
            //	    foreach (Collider c in areaObject.transform.parent.GetComponent<CubeScript>().colliderObject.GetComponents<Collider>()){
            //		    foreach (Collider c1 in colliderObject.GetComponents<Collider>()){
            //			    Physics.IgnoreCollision(c1, c, false);
            //		    }	   
            //	    }
            //	    if (areaObject.transform.parent.parent){
            //		    if (areaObject.transform.parent.parent.parent){
            //			    foreach (Collider c in areaObject.transform.parent.parent.parent.GetComponent<CubeScript>().colliderObject.GetComponents<Collider>()){
            //				    foreach (Collider c1 in colliderObject.GetComponents<Collider>()){
            //					    Physics.IgnoreCollision(c1, c, false);
            //				    }	   
            //			    }
            //		    }
            //	    }
            //    }
            //}
            if (areaObject)
            {
                insideSub = false;
                if (!childColliders)
                {
                    if (ignoreFlares)
                    {
                        gameObject.layer = 14;
                    }
                    else
                    {
                        gameObject.layer = 8;
                    }
                    gameObject.tag = "Untagged";
                }
                else
                {
                    foreach (Transform t in transform)
                    {
                        if (t.gameObject.name == "Collider" && t.gameObject.name != "Portal")
                        {
                            t.tag = "Untagged";
                            if (ignoreFlares)
                            {
                                t.gameObject.layer = 14;
                            }
                            else
                            {
                                t.gameObject.layer = 8;
                            }
                        }
                    }
                    gameObject.layer = 8;
                    gameObject.tag = "Untagged";
                }
                if (areaObject.transform.parent)
                {
                    foreach (Collider c in getParentColliders())
                    {
                        foreach (Collider c1 in colliderObject.GetComponents<Collider>())
                        {
                            Physics.IgnoreCollision(c, c1, false);
                        }
                    }
                }
            }

            if (areaObject)
            {
                areaObject.GetComponent<BoxScript>().occupied = false;
                areaObject = null;
                placeInArea = false;
                transform.parent = null;
            }
            //ca.intensity.value = Mathf.SmoothDamp(ca.intensity, 0f, ref dampVel, 0.2f);
        }
        else
        {

        }

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "PlaceableArea" && !collision.GetComponent<BoxScript>().occupied && !insideSub && collision.transform.lossyScale.magnitude >= transform.lossyScale.magnitude && !areaObject)
        {
            //if (collision.transform.parent){
            //if (!collision.transform.parent.GetComponent<CubeScript>().held){
            areaObject = collision.gameObject;
            placeInArea = true;
            if (!held)
                collision.GetComponent<BoxScript>().occupied = true;
            //}
            //}else{
            //areaObject = collision.gameObject;
            //placeInArea = true;
            //if (!held)
            //collision.GetComponent<BoxScript>().occupied = true;
            //}
        }
    }

    public List<Collider> getParentColliders()
    {
        if (areaObject)
        {
            if (areaObject.transform.parent)
            {
                if (areaObject.transform.parent.parent)
                {
                    if (areaObject.transform.parent.parent.parent)
                    {
                        List<Collider> l = new List<Collider>(areaObject.transform.parent.GetComponent<CubeScript>().colliderObject.GetComponents<Collider>());
                        l.AddRange(new List<Collider>(areaObject.transform.parent.GetComponent<CubeScript>().getParentColliders()));
                        return l;
                    }
                }
                return new List<Collider>(areaObject.transform.parent.GetComponent<CubeScript>().colliderObject.GetComponents<Collider>());
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    //private void OnTriggerStay(Collider collision)
    //{
    //	if (collision.tag == "PlaceableArea"){
    //		if (!held && areaObject == collision.gameObject){
    //			collision.GetComponent<BoxScript>().occupied = true;
    //		}else{
    //			collision.GetComponent<BoxScript>().occupied = false;
    //		}
    //	}
    //}
}
