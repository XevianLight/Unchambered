using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

public class CubeScript : MonoBehaviour
{
    Rigidbody rb;
    public Vector3 targetRotation = Vector3.zero;
    public Vector3 targetPosition = Vector3.zero;
    public Vector3 defaultScale;
    public float targetScale;
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public Vector3 initialScale;
    public Vector3 endScale;
    public float scaleFactor = 1;
    public float scale;
    public float snapAngle = 90f;
    public bool held = false;
    public bool snappingEnabled = false;
    public bool placeInArea = false;
    public bool insideSub = false;
    public GameObject areaObject;
    //public PostProcessVolume postProcessVolume;
    //private ChromaticAberration ca;
    //float dampVel;
    public Light lt;
    public float lightModifier = 1f;
    //Vector3 scale;
    public int occupants = 0;
    public int initialColliderCount = 1;
    public int colliderCount = 1;
    public bool childColliders = false;
    public float snapSpeed = 0.7f;
    public bool isLightParent = false;
    public GameObject colliderObject;
    public bool isParentHeld = false;
    public bool ignoreFlares = false;
    public bool hasPlayedSound = false;
    public AudioClip expand;
    AudioSource audioSource;
    public Collider[] connectedAreas;
    public GameObject VFXScale;
    public float expandTime;
    public float contractTime;
    bool saveVectors = true;
    public AnimationCurve expandCurve;
    public int pickUpLayer = 8;
    public int insideSubLayer = 12;
    public int insideSubCubeLayer = 1;
    public bool scaling = false;
    public AnimationCurve test;



    // Start is called before the first frame update
    void Start()
    {
        //postProcessVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        //postProcessVolume.profile.TryGetSettings(out ca);
        defaultScale = transform.localScale;
        endScale = defaultScale;
        rb = gameObject.GetComponent<Rigidbody>();
        if (!childColliders)
            colliderObject = gameObject;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (VFXScale)
        {
            VFXScale.transform.localScale = transform.lossyScale;
        }
        scale = transform.lossyScale.magnitude / defaultScale.magnitude;
        if (lt != null)
        {
            // lt.range = transform.lossyScale.magnitude * lightModifier;
            lt.intensity = transform.lossyScale.magnitude * lightModifier;
        }
        //scale = transform.localScale;
        if (areaObject)
        {
            if (childColliders)
            { //If the object has a child which contains its compound collider
                foreach (Transform t in transform) //Set the layer of the collider object respectively
                {
                    if (t.gameObject.name == "Collider" && t.gameObject.name != "Portal")
                    {
                        t.tag = "Surface";
                        t.gameObject.layer = isParentHeld ? 13 : 12;
                        insideSub = true;
                    }
                }

                gameObject.layer = 8;
                gameObject.tag = "Surface";
            }
            else
            {
                tag = "Surface";
                gameObject.layer = isParentHeld ? 13 : 12;
            }
            if (areaObject.transform.parent)
            {
                CubeScript cs = areaObject.transform.parent.GetComponent<CubeScript>();
                isParentHeld = cs.held && !held ? true : cs.isParentHeld;
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
            if (areaObject)
            {
                insideSub = false;
                if (!childColliders)
                {
                    gameObject.layer = 8;
                    gameObject.tag = "Untagged";
                }
                else
                {
                    foreach (Transform t in transform)
                    {
                        if (t.gameObject.name == "Collider" && t.gameObject.name != "Portal")
                        {
                            t.tag = "Untagged";
                            t.gameObject.layer = 8;
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
        if (!held && snappingEnabled && placeInArea && areaObject != null)
        {
            contractTime = 0f;
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
            if (saveVectors)
            {
                initialPosition = transform.localPosition;
                initialRotation = transform.localRotation;
                initialScale = transform.localScale;
                saveVectors = false;
            }
            rb.isKinematic = true;
            expandTime += Time.deltaTime / snapSpeed;
            float curveTime = expandCurve.Evaluate(expandTime);
            transform.localPosition = Vector3.Slerp(initialPosition, Vector3.zero, curveTime);
            transform.localRotation = Quaternion.Slerp(initialRotation, Quaternion.Euler(Mathf.Round(transform.localEulerAngles.x / snapAngle) * snapAngle, Mathf.Round(transform.localEulerAngles.y / snapAngle) * snapAngle, Mathf.Round(transform.localEulerAngles.z / snapAngle) * snapAngle), curveTime);
            transform.localScale = Vector3.Slerp(initialScale, Vector3.one * targetScale, curveTime);

            //Debug.Log(transform.localScale.magnitude);
            BoxScript bs = areaObject.GetComponent<BoxScript>();
            bs.forceOut = Mathf.Round(transform.localScale.magnitude) == 2 ? false : true;
            scaling = (expandTime <= 1) ? true : false;
            test.AddKey(Mathf.Clamp(expandTime, 0, 1), scaling ? 1 : 0);
        }
        else
        {
            //rb.isKinematic = false;
            if (!saveVectors)
            {
                endScale = transform.lossyScale;
                saveVectors = true;
            }

            expandTime = 0f;
            contractTime += Time.deltaTime / snapSpeed;
            //Debug.Log(Vector3.Slerp(endScale, defaultScale, 0));
            transform.localScale = Vector3.Slerp(endScale, defaultScale, contractTime);
            hasPlayedSound = false;
            scaling = (contractTime <= 1) ? true : false;
            test.AddKey(Mathf.Clamp(contractTime, 0, 1), scaling ? 1 : 0);
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

    private Vector3 RoundVector(Vector3 vect)
    {
        return new Vector3(Mathf.Round(vect.x), Mathf.Round(vect.y), Mathf.Round(vect.z));
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

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Rigidbody>())
        {
            if (!areaObject || held)
                other.GetComponent<Rigidbody>().AddExplosionForce(100f, transform.position, transform.localScale.magnitude);
        }
    }
}
