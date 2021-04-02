using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MouseLook : MonoBehaviour
{

    [Header("Control Settings")]
    [Tooltip("Sensitivity of camera rotation to mouse movement")]
    public float sensitivity = 3;
    [Tooltip("Sensitivity of rotations to mouse movement")]
    public float rotationSensitivity = 0.1f;
    [Tooltip("Sensitivity of z rotations to scroll wheel movement")]
    public float scrollWheelSensitivity = 10f;
    [Tooltip("Max camera azimuth angle")]
    public float clampUpper = 90f;
    [Tooltip("Min camera azimuth angle")]
    public float clampLower = -90f;
    [Tooltip("How fast mouse rotations slow to a halt")]
    public float mouseDamping = 1f;
    [Tooltip("How fast scroll wheel rotations slow to a halt")]
    public float scrollWheelDamping = 0.1f;
    [Tooltip("The speed at which cubes snap to the cursor when held")]
    public float snapSpeed = 0.5f;
    [Header("Misc Settings")]
    [Tooltip("Range to detect if held object hits surface")]
    public float range = 10f;
    [Tooltip("Distance at which to hold cube if surface is not hit")]
    public float rangeIfNotHit = 2f;
    [Tooltip("Should held objects rotate relative to the camera's perspective")]
    public bool rotateRelativeCamera = true;
    [Tooltip("The angle at which cubes should snap to when let go")]
    public float snapAngle = 90f;
    [Header("Debugging")]
    [Tooltip("The camera's current global rotation")]
    public Vector2 rotation;
    //[Tooltip("The held objects global rotation")]
    //public Vector3 heldRotation;
    [Tooltip("The global point where raycast hit an object")]
    public Vector3 hitPoint;
    //[Tooltip("Used for special effects")]
    //public PostProcessVolume postProcessVolume;
    //private DepthOfField dof;
    [Tooltip("The speed at which the DoF should focus")]
    public float focusSpeed = 5f;
    [Tooltip("The main camera gameObject")]
    public GameObject cam;
    [Tooltip("The main camera gameObject")]
    public GameObject camPitch;
    [Tooltip("The object currently hit by the raycast")]
    public GameObject objectHit;
    [Tooltip("The currently held object")]
    public GameObject heldObject;
    [Tooltip("Used if the held object has a child that should rotate instead of itself")]
    public GameObject heldChild;
    [Tooltip("Does the held object have a child that should instead rotate")]
    public bool rotateChild = false;
    [Tooltip("Stops the cameras response to mouse movement")]
    bool stopRotation = false;
    [Tooltip("Can the objectHit be picked up")]
    public bool canBePickedUp;
    [Tooltip("The layer that should be tested for pick ups")]
    public LayerMask pickupLayer;
    [Tooltip("The layer that pick up tests should ignore")]
    public LayerMask ignorePickupLayer;
    [Tooltip("The global rotation which the held object should attempt to rotate to")]
    public Vector3 targetAngle = Vector3.zero;
    [Tooltip("Only used for damping")]
    public Vector3 smoothVel = Vector3.zero;
    [Tooltip("The objects previous position, used to calculate the velocity that should be applied when dropped")]
    public Vector3 heldObjectPositionOld;
    [Tooltip("The velocity the held object should inherit when dropped")]
    public Vector3 heldObjectVelocity;
    [Tooltip("The angilar velocity the held object should inherit when dropped")]
    public Vector3 heldAngularVelocity;
    [Tooltip("The endpoint of the raycast")]
    public Vector3 endpoint;
    public GameObject rotationVector;
    public Vector3 finalPosition;
    public Quaternion tempRotation;
    CubeScript cs;
    Rigidbody rb;
    Rigidbody crb;
    Portal hitPortal;
    bool rotatePortal = true;
    public bool inPortal = false;

    // Start is called before the first frame update
    void Start()
    {
        //postProcessVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        //postProcessVolume.profile.TryGetSettings(out dof);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Camera mouse 
        CameraRotation(this.gameObject, camPitch, stopRotation);
        Vector3 cameraPosition = cam.transform.position;
        Vector3 cameraDirection = cam.transform.forward;

        // Set a temporary variable for the getObjectHit function
        objectHit = getObjectHit(cameraPosition, cameraDirection, range);
        //Debug.Log(getObjectHit(Camera.main.transform.position, Camera.main.transform.forward, range).name);
        if (heldObject)
        {
            if (rotateChild)
            {
                crb = heldChild.GetComponent<Rigidbody>();
            }
            else
            {
                rb = heldObject.GetComponent<Rigidbody>();
            }

            cs = heldObject.GetComponent<CubeScript>();

            // Gets a velocity vector based on delta position and applies it to the rigidbody. Used for kinematic velocity.
            heldObjectVelocity = (heldObject.transform.position - heldObjectPositionOld) / Time.deltaTime;
            rb.velocity = heldObjectVelocity;
        }
        else
        {
            cs = null;
            rb = null;
            crb = null;
        }

        if (Input.GetMouseButtonDown(0))
        {

            // Does the object exist and can it be picked up
            if (objectHit != null && canBePickedUp)
            {
                heldObject = objectHit;
                cs = heldObject.GetComponent<CubeScript>();
                rb = heldObject.GetComponent<Rigidbody>();
                // Does the object have a child that should be rotated instead of itself?
                if (cs.isLightParent)
                {
                    // If so, rotate the child instead
                    foreach (Transform t in heldObject.transform)
                    {
                        if (t.gameObject.name == "LightCube")
                        {
                            rotateChild = true; // Tells the object to rotate the child
                            rotateRelativeCamera = false; // Camera relative rotation is disorienting for child rotation
                            heldChild = t.gameObject;
                        }
                    }
                }
                else
                {
                    // If not, rotate itself
                    rotateChild = false; // Tells the object to rotate itself
                    rotateRelativeCamera = true;
                }
                heldAngularVelocity = rotateChild ? crb.angularVelocity : rb.angularVelocity;
            }
        }

        if (Input.GetMouseButton(0)/* && !Input.GetMouseButton(2)*/)
        {
            // Is an object being held?
            if (heldObject)
            {
                Vector3 heldPosition = heldObject.transform.position;
                Quaternion heldRotation;
                //cs = heldObject.GetComponent<CubeScript>();
                // Which object's rotation should be tracked?
                heldRotation = rotateChild ? heldChild.transform.rotation : heldObject.transform.rotation;

                // For velocity
                heldObjectPositionOld = heldPosition;

                RaycastHit hit;

                // Make the held object kinematic, it should not be affected by gravity or collisions
                rb.isKinematic = true;

                // Is the object allowed to snap to an area when released?
                if (cs.snappingEnabled)
                {
                    cs.snapAngle = snapAngle;
                    cs.held = true;
                }

                if (Physics.Raycast(transform.position + (cam.transform.forward * 0.01f), cam.transform.forward, out hit, range, (1 << 16)))
                {
                    if (rotatePortal)
                    {
                        hitPortal = hit.transform.GetComponent<Portal>();
                        if (!inPortal)
                            heldObject.transform.rotation = Portal.TransformRotationBetweenPortals(hitPortal, hitPortal.targetPortal, heldObject.transform.rotation);
                        rotatePortal = false;
                    }
                }
                else
                {
                    if (!rotatePortal)
                    {
                        if (hitPortal)
                        {
                            if (!inPortal)
                                heldObject.transform.rotation = Portal.TransformRotationBetweenPortals(hitPortal.targetPortal, hitPortal, heldObject.transform.rotation);
                            rotatePortal = true;
                        }
                    }
                }

                // Debug ideal object location
                ExtDebug.DrawBoxCastOnHit(
                cameraPosition,
                heldObject.transform.lossyScale / 2,
                heldObject.transform.rotation,
                //cs.scaling ? (heldPosition - cameraPosition).normalized : cameraDirection,
                Vector3.Slerp((heldPosition - cameraPosition).normalized, cameraDirection, cs.contractTime),
                range,
                new Color(0, 0, 255)
                );

                //if (Physics.BoxCast(Camera.main.transform.position, heldObject.transform.lossyScale / 2, Camera.main.transform.forward, out hit, heldObject.transform.rotation, range, ~(pickupLayer | (1 << 2) | (1 << 10) | (1 << 11) | (1 << 13) | (1 << 14))))
                //if (Portal.RaycastRecursive(Camera.main.transform.position, Camera.main.transform.forward, range, ~(pickupLayer | (1 << 2) | (1 << 10) | (1 << 11) | (1 << 13) | (1 << 14)), 8, out endpoint, out hit, out rotationVector))

                // Create a custom boxcast via Portal.cs which can be recursed through portals
                if (Portal.BoxcastRecursive(
                    cameraPosition,
                    heldObject.transform.lossyScale / 2,
                    //cs.scaling ? (heldPosition - cameraPosition).normalized : cameraDirection,
                    Vector3.Slerp((heldPosition - cameraPosition).normalized, cameraDirection, cs.contractTime),
                    heldRotation,
                    range,
                    range,
                    ~pickupLayer,
                    16,
                    out endpoint,
                    out hit,
                    out rotationVector,
                    out finalPosition))
                {
                    //Debug.Log(endpoint);
                    // If the boxcast hit an object, attempt to move the held object towards point without intersecting
                    //Debug.Log((Vector3.Distance(heldObject.transform.position, endpoint)) + " Distance");
                    //Debug.Log((2 * range * cs.lossyScale) + " Max Distance");
                    bool allowSnap = true;
                    Debug.DrawLine(cameraPosition, hit.point, new Color(255, 255, 0));
                    if (cs.connectedAreas.Length > 0)
                    {
                        foreach (Collider c in cs.connectedAreas)
                        {
                            allowSnap = cs.scaling ? false : !c.bounds.Contains(endpoint);
                        }
                    }

                    if (Vector3.Distance(heldPosition, endpoint) >= 2 * range * cs.scale)
                    {
                        if (allowSnap)
                        {
                            heldObject.transform.position = endpoint;
                            Debug.Log("Snapped position as distance was greater than max");
                        }
                        else
                        {
                            Debug.Log("Object snapping was denied");
                        }
                    }
                    // If the object is too far away, set position instead of lerp
                    else
                    {
                        if (allowSnap)
                        {
                            heldObject.transform.position = Vector3.Slerp(
                                heldPosition,
                                finalPosition + (rotationVector.transform.forward * hit.distance),
                                (Time.deltaTime * snapSpeed) / cs.scale);
                            //heldObject.transform.position = finalPosition + (rotationVector.transform.forward * hit.distance);
                        }
                    }
                }
                else
                {
                    //Debug.Log(endpoint);
                    bool allowSnap = true;
                    if (cs.connectedAreas.Length > 0)
                    {
                        foreach (Collider c in cs.connectedAreas)
                        {
                            allowSnap = cs.scaling ? false : !c.bounds.Contains(endpoint);
                        }
                    }
                    // Otherwise, hold the object at a fixed distance from the boxcasts origin
                    //Debug.Log((Vector3.Distance(heldObject.transform.position, endpoint)) + " Distance");
                    //Debug.Log((2 * range * cs.lossyScale) + " Max Distance");
                    // If the object is too far away, set position instead of lerp
                    if (Vector3.Distance(heldPosition, endpoint) >= 2 * range * cs.scale)
                    {
                        if (allowSnap)
                        {
                            heldObject.transform.position = endpoint;
                            Debug.Log("Snapped position as distance was greater than max");
                        }
                        else
                        {
                            Debug.Log("Object snapping was denied");
                        }
                    }
                    else
                    {
                        if (allowSnap)
                        {
                            heldObject.transform.position = Vector3.Slerp(
                                heldPosition,
                                endpoint,
                                (Time.deltaTime * snapSpeed) / cs.scale);
                        }
                        //heldObject.transform.position = endpoint;
                    }
                    //heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, (rangeIfNotHit))), snapSpeed * Time.deltaTime / heldObject.transform.lossyScale.magnitude);

                    //heldObject.GetComponent<Rigidbody>().drag = 10 - Mathf.Abs(Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, rangeIfNotHit)), heldObject.transform.position));
                    //heldObject.GetComponent<Rigidbody>().AddForce((Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, rangeIfNotHit)) - heldObject.transform.position) * Mathf.Abs(Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, rangeIfNotHit)), heldObject.transform.position)) * 10);
                }
            }
        }
        else
        {

        }

        if (Input.GetMouseButtonUp(0))
        {
            // Is an object being held?
            if (heldObject)
            {
                // If so, apply calculated velocity, toggle its held state, disable kinematic
                rb.isKinematic = false;
                rb.velocity = heldObjectVelocity;
                cs.held = false;

                // Retain angular velocity when released
                (rotateChild ? crb : rb).angularVelocity = heldAngularVelocity;
            }
            else
            {
                //heldObject.GetComponent<CubeScript>().held = false;
            }
            //heldObject.transform.parent = null;
            //heldObject.GetComponent<Rigidbody>().drag = 0f;
            //heldObject.GetComponent<Rigidbody>().useGravity = true;

            heldObject = null;
            heldChild = null;
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Disable cameras response to input

            //if (heldObject)
            //{
            stopRotation = true;
            // Save targeted objects angular velocity for later

            heldAngularVelocity = rotateChild ? crb.angularVelocity : rb.angularVelocity;
            RotateObject(rotateChild ? heldObject : heldChild, rotateRelativeCamera);

            //}
        }

        if (Input.GetMouseButton(1))
        {

            // If an object is held, rotate it from mouse input
            //if (heldObject != null)
            //{
            stopRotation = true;
            RotateObject(rotateChild ? heldChild : heldObject, rotateRelativeCamera);
            //}
        }
        else
        {
            // Re-enable camera rotation
            stopRotation = false;

            // Apply stored angular velocity when released
            if (heldObject != null)
            {
                KeepVelocity(rotateChild ? heldChild : heldObject, rotateRelativeCamera, true, heldAngularVelocity);
            }
        }

        // For fun stuff
        if (Input.GetMouseButtonDown(2))
        {
            //heldObject.GetComponent<CubeScript>().held = false;
            //heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldAngularVelocity = Vector3.one * 100;
            //heldObject = null;
        }

        // Cleanup
        if (Input.GetMouseButtonUp(1))
        {
            targetAngle = Vector3.zero;
            smoothVel = Vector3.zero;
        }
    }

    // Detect mouse input and rotate the camera respectively
    void CameraRotation(GameObject target, GameObject target2, bool rotationDisabled)
    {
        if (rotationDisabled)
        {
            // If the cameras rotation is disabled, store its rotation in case it changes
            //rotation.x = cam.transform.rotation.eulerAngles.x;
            //rotation.y = transform.rotation.eulerAngles.y;
            target.transform.localRotation = Quaternion.Euler(target.transform.rotation.x, rotation.y, target.transform.rotation.z);
            target2.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        }
        else
        {
            // Otherwise, rotate the camera from input
            rotation.y += Input.GetAxis("Mouse X") * sensitivity;
            rotation.x += -Input.GetAxis("Mouse Y") * sensitivity;
            rotation.x = Mathf.Clamp(rotation.x, -clampUpper, -clampLower);
            //Debug.Log(target.transform.eulerAngles);
            tempRotation = Quaternion.Lerp(tempRotation, Quaternion.identity, Time.deltaTime * 10);
            target.transform.localRotation = Quaternion.Euler(tempRotation.eulerAngles.x, rotation.y, tempRotation.eulerAngles.z);
            target2.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        }
    }

    // Detect mouse input and rotate target respectively, relative to the camera, or as a gimbal
    void RotateObject(GameObject target, bool cameraRelative)
    {
        if (target != null)
        {
            //target.transform.localRotation = Quaternion.Euler(target.transform.rotation.x, , target.transform.rotation.z);
            //target.transform.Rotate(Camera.main.transform.up, -Input.GetAxis("Mouse X"));
            //target.transform.Rotate(Camera.main.transform.right, Input.GetAxis("Mouse Y"));

            // Should the object rotate relative to the cameras viewpoint or a gimbal
            if (cameraRelative)
            {
                target.GetComponent<Rigidbody>().isKinematic = true;
                //heldRotation.y += Input.GetAxis("Mouse X");
                //heldRotation.x += -Input.GetAxis("Mouse Y");
                targetAngle.x -= Input.GetAxis("Mouse X") * rotationSensitivity;
                targetAngle.x = Mathf.SmoothDamp(targetAngle.x, 0f, ref smoothVel.x, mouseDamping);
                targetAngle.y += Input.GetAxis("Mouse Y") * rotationSensitivity;
                targetAngle.y = Mathf.SmoothDamp(targetAngle.y, 0f, ref smoothVel.y, mouseDamping);
                targetAngle.z += Input.GetAxis("Mouse ScrollWheel") * scrollWheelSensitivity * 10;
                targetAngle.z = Mathf.SmoothDamp(targetAngle.z, 0f, ref smoothVel.z, scrollWheelDamping);
                target.transform.RotateAround(target.transform.position, rotationVector.transform.up, targetAngle.x);
                target.transform.RotateAround(target.transform.position, rotationVector.transform.right, targetAngle.y);
                target.transform.RotateAround(target.transform.position, rotationVector.transform.forward, targetAngle.z);
            }
            else
            {
                //heldRotation.y += Input.GetAxis("Mouse X");
                //heldRotation.x += -Input.GetAxis("Mouse Y");
                //heldRotation.z = target.transform.rotation.eulerAngles.z;
                //target.transform.localRotation = Quaternion.Euler(heldRotation.x, heldRotation.y, 0f);
                target.transform.RotateAround(target.transform.position, Vector3.up, Input.GetAxis("Mouse X") * sensitivity);
                target.transform.RotateAround(target.transform.position, target.transform.right, Input.GetAxis("Mouse Y") * sensitivity);
            }
        }
    }

    // Retains the velocity of the target kinematic object
    void KeepVelocity(GameObject target, bool cameraRelative, bool keepVelocity, Vector3 angularVelocity)
    {
        if (target != null)
        {
            if (keepVelocity)
            {
                target.transform.RotateAround(target.transform.position, Vector3.up, (angularVelocity.y));
                target.transform.RotateAround(target.transform.position, Vector3.right, angularVelocity.x);
                target.transform.RotateAround(target.transform.position, Vector3.forward, angularVelocity.z);
                heldAngularVelocity = Vector3.Lerp(heldAngularVelocity, Vector3.zero, Time.deltaTime * 1);
            }
        }
    }

    // Casts a ray and returns the gameObject hit, if it applies
    GameObject getObjectHit(Vector3 origin, Vector3 direction, float range)
    {
        // Define and fire a custom raycast from Portal.cs which can recurse through portals
        Vector3 rayDirection = cam.transform.forward;
        RaycastHit hit;
        Vector3 rayOrigin;
        if (Portal.RaycastRecursive(cam.transform.position, rayDirection, range, ~ignorePickupLayer, 8, out endpoint, out hit, out rotationVector, out rayOrigin)) //Cast out a ray returns the gameobject of the collider hit
        {
            Debug.DrawLine(cam.transform.position, hit.point, new Color(0, 255, 0));
            hitPoint = hit.point;

            // No, you may not pick up the floor
            canBePickedUp = !string.Equals(hit.transform.tag, "Floors");

            return hit.transform.gameObject;
        }
        else
        {
            Debug.DrawLine(rayOrigin, endpoint, new Color(255, 0, 0));
            Debug.DrawLine(cam.transform.position, endpoint, new Color(255, 0, 0));
            return null;
        }
    }
}



//for if hit
//heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, Camera.main.transform.position + Camera.main.transform.forward* hit.distance, snapSpeed* Time.deltaTime / heldObject.transform.lossyScale.magnitude);

//for if not hit
//heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, (rangeIfNotHit))), snapSpeed * Time.deltaTime / heldObject.transform.lossyScale.magnitude);
