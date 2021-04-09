using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;


public class Portal : MonoBehaviour
{
    public Portal targetPortal;


    public Transform normalVisible;
    public Transform normalInvisible;

    public Camera portalCamera;
    public Renderer viewthroughRenderer;
    public Material initialMaterial;
    private Material viewthroughMaterial;

    private Camera mainCamera;

    private Vector4 vectorPlane;

    Vector3 refPos;
    Quaternion refRot;

    private HashSet<PortalableObject> objectsInPortal = new HashSet<PortalableObject>();
    private HashSet<PortalableObject> objectsInPortalToRemove = new HashSet<PortalableObject>();

    public Portal[] visiblePortals;

    public Texture viewthroughDefaultTexture;
    ScriptableRenderContext SRCTemp;

    static GameObject orientRay;

    public int maxRecursionsOverride = -1;
    public bool allowRecursiveRaycasts = true;
    bool run = false;

    public Plane plane;

    public bool ShouldRender(Plane[] cameraPlanes) => viewthroughRenderer.isVisible && GeometryUtility.TestPlanesAABB(cameraPlanes, viewthroughRenderer.bounds);

    public static Vector3 TransformPositionBetweenPortals(Portal sender, Portal target, Vector3 position)
    {
        return target.normalInvisible.TransformPoint(sender.normalVisible.InverseTransformPoint(position));
    }

    public static Vector3 TransformDirectionBetweenPortals(Portal sender, Portal target, Vector3 position)
    {
        return target.normalInvisible.TransformDirection(sender.normalVisible.InverseTransformDirection(position));
    }

    public static Quaternion TransformRotationBetweenPortals(Portal sender, Portal target, Quaternion rotation)
    {
        return target.normalInvisible.rotation * Quaternion.Inverse(sender.normalVisible.rotation) * rotation;
    }

    /*private void OnEnable()
	{
	RenderPipeline.beginCameraRendering += UpdateCamera;
	}

	private void OnDisable()
	{
	RenderPipeline.beginCameraRendering -= UpdateCamera;
	}*/

    private void Start()
    {
        run = true;
        // Get cloned material

        viewthroughMaterial = viewthroughRenderer.material;

        // Cache the main camera

        mainCamera = Camera.main;

        // Generate bounding plane

        var plane = new Plane(normalVisible.forward, transform.position);
        vectorPlane = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        RenderPipeline.beginCameraRendering += UpdateCamera;
        StartCoroutine(WaitForFixedUpdateLoop());
        foreach (Transform t in transform)
        {
            if (t.gameObject.name == "OrientRay")
            {
                orientRay = t.gameObject;
            }
        }
    }

    private void Awake()
    {
        // Generate bounding plane

        plane = new Plane(normalVisible.forward, transform.position);
        vectorPlane = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
    }

    private IEnumerator WaitForFixedUpdateLoop()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        while (run)
        {
            yield return waitForFixedUpdate;
            try
            {
                CheckForPortalCrossing();
            }
            catch (Exception e)
            {
                // Catch exceptions so our loop doesn't die whenever there is an error
                Debug.LogException(e);
            }
        }
    }

    private void CheckForPortalCrossing()
    {
        // Clear removal queue

        objectsInPortalToRemove.Clear();

        // Check every touching object

        foreach (var portalableObject in objectsInPortal)
        {
            // If portalable object has been destroyed, remove it immediately

            if (portalableObject == null)
            {
                objectsInPortalToRemove.Add(portalableObject);
                continue;
            }

            // Check if portalable object is behind the portal using Vector3.Dot (dot product)
            // If so, they have crossed through the portal.
            var pivot = portalableObject.transform;
            var directionToPivotFromTransform = pivot.position - transform.position;
            directionToPivotFromTransform.Normalize();
            var pivotToNormalDotProduct = Vector3.Dot(directionToPivotFromTransform, normalVisible.forward);
            if (pivotToNormalDotProduct > 0) continue;

            // Warp object
            Rigidbody rb = portalableObject.GetComponent<Rigidbody>();
            var relVelocity = transform.InverseTransformDirection(new Vector3(-rb.velocity.x, rb.velocity.y, -rb.velocity.z));
            rb.velocity = targetPortal.transform.TransformDirection(relVelocity);
            var newPosition = TransformPositionBetweenPortals(this, targetPortal, portalableObject.transform.position);
            var newRotation = TransformRotationBetweenPortals(this, targetPortal, portalableObject.transform.rotation);
            //Debug.Log(newRotation.eulerAngles);
            //Debug.Log(newRotation);
            if (portalableObject.tag == "Player")
            {
                MouseLook ml;
                ml = portalableObject.GetComponent<MouseLook>();
                portalableObject.OnHasTeleported(this, targetPortal, newPosition, newRotation);
                portalableObject.transform.position = newPosition;
                portalableObject.transform.eulerAngles = new Vector3(newRotation.eulerAngles.x, portalableObject.transform.eulerAngles.y, newRotation.eulerAngles.z);
                ml.tempRotation = newRotation;
                ml.rotation.y = newRotation.eulerAngles.y;
                //portalableObject.transform.eulerAngles = new Vector3(newRotation.eulerAngles.x, ml.rotation.y, newRotation.eulerAngles.z);
            }
            else
            {
                portalableObject.OnHasTeleported(this, targetPortal, newPosition, newRotation);
                portalableObject.transform.position = newPosition;
                portalableObject.transform.eulerAngles = newRotation.eulerAngles;
            }
            // Object is no longer touching this side of the portal

            objectsInPortalToRemove.Add(portalableObject);
        }

        // Remove all objects queued up for removal

        foreach (var portalableObject in objectsInPortalToRemove)
        {
            objectsInPortal.Remove(portalableObject);
        }
    }

    public static bool RaycastRecursive(
        Vector3 position,
        Vector3 direction,
        float maxRange,
        LayerMask layerMask,
        int maxRecursions,
        out Vector3 endpoint,
        out RaycastHit hitInfo,
        out GameObject finalDirectionObj,
        out Vector3 finalPosition
        )
    {
        return RaycastRecursiveInternal(
            position,
            direction,
            maxRange,
            layerMask,
            maxRecursions,
            out endpoint,
            out hitInfo,
            out finalDirectionObj,
            out finalPosition,
            0,
            null);
    }

    private static bool RaycastRecursiveInternal(
        Vector3 position,
        Vector3 direction,
        float maxRange,
        LayerMask layerMask,
        int maxRecursions,
        out Vector3 endpoint,
        out RaycastHit hitInfo,
        out GameObject finalDirectionObj,
        out Vector3 finalPosition,
        int currentRecursion,
        GameObject ignoreObject)
    {
        // Ignore a specific object when raycasting.
        // Useful for preventing a raycast through a portal from hitting the target portal from the back,
        // which makes a raycast unable to go through a portal since it'll just be absorbed by the target portal's trigger.

        var ignoreObjectOriginalLayer = 16;
        if (ignoreObject)
        {
            ignoreObjectOriginalLayer = ignoreObject.layer;
            ignoreObject.layer = 2; // Ignore raycast
        }

        // Shoot raycast
        //if (portal.allowRecursiveRaycasts)
        //{
        //    layerMask = ~layerMask & ~(1 << 16);
        //}
        //else
        //{
        //    layerMask = ~layerMask | (1 << 16);
        //}
        var raycastHitSomething = Physics.Raycast(
            position,
            direction,
            out var hit,
            maxRange,
            layerMask); // Clamp to max array length

        // Reset ignore

        if (ignoreObject)
            ignoreObject.layer = ignoreObjectOriginalLayer;

        // If no objects are hit, the recursion ends here, with no effect

        if (!raycastHitSomething)
        {
            hitInfo = new RaycastHit(); // Dummy
            endpoint = position + (direction * (maxRange - hit.distance));
            orientRay.transform.position = position;
            orientRay.transform.LookAt(endpoint);
            finalDirectionObj = orientRay;
            finalPosition = position;
            return false;
        }

        // If the object hit is a portal, recurse, unless we are already at max recursions
        var portal = hit.collider.GetComponent<Portal>();
        if (portal && portal.allowRecursiveRaycasts)
        {
            if (currentRecursion >= maxRecursions)
            {
                hitInfo = new RaycastHit(); // Dummy
                endpoint = position + (direction * (maxRange - hit.distance));
                orientRay.transform.position = position;
                orientRay.transform.LookAt(endpoint);
                finalDirectionObj = orientRay;
                finalPosition = position;
                return false;
            }

            // Continue going down the rabbit hole...

            return RaycastRecursiveInternal(
                TransformPositionBetweenPortals(portal, portal.targetPortal, hit.point),
                TransformDirectionBetweenPortals(portal, portal.targetPortal, direction),
                (maxRange - hit.distance),
                layerMask,
                maxRecursions,
                out endpoint,// tempVector3,
                out hitInfo,
                out finalDirectionObj,
                out finalPosition,
                currentRecursion + 1,
                portal.targetPortal.gameObject);
        }

        // If the object hit is not a portal, then congrats! We stop here and report back that we hit something.

        endpoint = hit.point;
        hitInfo = hit;
        //endpoint = Vector3.zero;
        orientRay.transform.position = position;
        orientRay.transform.LookAt(endpoint);
        finalDirectionObj = orientRay;
        finalPosition = position;
        return true;
    }

    public static bool BoxcastRecursive(
        Vector3 position,
        Vector3 scale,
        Vector3 direction,
        Quaternion rotation,
        float maxRange,
        float rangeIfNotHit,
        LayerMask layerMask,
        int maxRecursions,
        out Vector3 endpoint,
        out RaycastHit hitInfo,
        out GameObject finalDirectionObj,
        out Vector3 finalPosition)
    {
        return BoxcastRecursiveInternal(
            position,
            scale,
            direction,
            rotation,
            maxRange,
            rangeIfNotHit,
            layerMask,
            maxRecursions,
            out endpoint,
            out hitInfo,
            out finalDirectionObj,
            out finalPosition,
            0,
            null);
    }

    private static bool BoxcastRecursiveInternal(
        Vector3 position,
        Vector3 scale,
        Vector3 direction,
        Quaternion rotation,
        float maxRange,
        float rangeIfNotHit,
        LayerMask layerMask,
        int maxRecursions,
        out Vector3 endpoint,
        out RaycastHit hitInfo,
        out GameObject finalDirectionObj,
        out Vector3 finalPosition,
        int currentRecursion,
        GameObject ignoreObject)
    {

        // Ignore a specific object when raycasting.
        // Useful for preventing a raycast through a portal from hitting the target portal from the back,
        // which makes a raycast unable to go through a portal since it'll just be absorbed by the target portal's trigger.

        var ignoreObjectOriginalLayer = 16;
        if (ignoreObject)
        {
            ignoreObjectOriginalLayer = ignoreObject.layer;
            ignoreObject.layer = 2; // Ignore raycast
        }

        // Shoot raycast
        //if (portal.allowRecursiveRaycasts)
        //{
        //    layerMask = ~layerMask & ~(1 << 16);
        //}
        //else
        //{
        //    layerMask = ~layerMask | (1 << 16);
        //}
        var raycastHitSomething = Physics.Raycast(
            position,
            direction,
            out var hit,
            maxRange,
            layerMask); // Clamp to max array 
        //Debug.Log(raycastHitSomething);
        //Debug.DrawLine(position, position + (direction * maxRange), new Color(255, 255, 0));

        if (ignoreObject)
            ignoreObject.layer = ignoreObjectOriginalLayer;


        // If no objects are hit, the recursion ends here, with no effect

        if (!raycastHitSomething)
        {
            LayerMask tempLayerMask = ~layerMask | (1 << 16);
            var boxCastHitSomething = Physics.BoxCast(
                position - direction.normalized,
                scale,
                direction,
                out var boxHit,
                rotation,
                maxRange + 1,
                ~tempLayerMask);
            ExtDebug.DrawBoxCastBox(position - direction.normalized, scale, rotation, direction, maxRange + 1, new Color(0, 255, 0));
            if (boxCastHitSomething)
            {
                hitInfo = boxHit;
                endpoint = boxHit.point;
                finalPosition = position;
                finalDirectionObj = orientRay;
                return true;
            }
        }

        // If the object hit is a portal, recurse, unless we are already at max recursions
        if (hit.collider)
        {
            var portal = hit.collider.GetComponent<Portal>();
            if (portal && portal.allowRecursiveRaycasts)
            {
                if (currentRecursion >= maxRecursions)
                {
                    hitInfo = new RaycastHit(); // Dummy
                    endpoint = position + (direction * (maxRange));
                    orientRay.transform.position = position;
                    orientRay.transform.LookAt(endpoint);
                    finalDirectionObj = orientRay;
                    finalPosition = position;
                    return false;
                }

                // Continue going down the rabbit hole...

                return BoxcastRecursiveInternal(
                    TransformPositionBetweenPortals(portal, portal.targetPortal, hit.point),
                    scale,
                    TransformDirectionBetweenPortals(portal, portal.targetPortal, direction),
                    rotation,
                    (maxRange - hit.distance),
                    rangeIfNotHit,
                    layerMask,
                    maxRecursions,
                    out endpoint,
                    out hitInfo,
                    out finalDirectionObj,
                    out finalPosition,
                    currentRecursion + 1,
                    portal.targetPortal.gameObject);
            }
        }
        // If the object hit is not a portal, then congrats! We stop here and report back that we hit something.

        LayerMask tempLayerMask1 = ~layerMask | (1 << 16);
        var boxCastHitSomething1 = Physics.BoxCast(
            position - direction.normalized,
            scale,
            direction,
            out var boxHit1,
            rotation,
            maxRange + 1,
            ~tempLayerMask1);
        Debug.DrawLine(position, position - direction.normalized * 2, new Color(255, 0, 0));
        //Debug.Log(boxCastHitSomething1);
        ExtDebug.DrawBoxCastBox(position - direction.normalized, scale, rotation, direction, maxRange + 1, new Color(255, 0, 0));
        if (boxCastHitSomething1)
        {
            hitInfo = boxHit1;
            endpoint = boxHit1.point;
            finalPosition = position;
            finalDirectionObj = orientRay;
            return true;
        }
        hitInfo = new RaycastHit();
        endpoint = position + (direction * maxRange);
        finalPosition = position;
        finalDirectionObj = orientRay;
        return false;
    }

    void UpdateCamera(ScriptableRenderContext SRC, Camera camera)
    {
        SRCTemp = SRC;
    }

    private void Update()
    {
        if (transform.parent)
        {
            if (transform.parent.GetComponent<CubeScript>())
            {
                if (transform.parent.GetComponent<CubeScript>().held)
                {
                    gameObject.layer = 2;
                    allowRecursiveRaycasts = false;
                }
                else
                {
                    gameObject.layer = 16;
                    allowRecursiveRaycasts = true;
                }
            }
        }
    }

    /*private void Update()
    {
    if (viewthroughRenderer.isVisible)
    {
    viewthroughRenderer.enabled = true;
    }
    else
    {
    viewthroughRenderer.enabled = false;
    }
    }*/

    public void RenderViewthroughRecursive(
        Vector3 refPosition,
        Quaternion refRotation,
        out RenderTexturePool.PoolItem temporaryPoolItem,
        out Texture originalTexture,
        out int debugRenderCount,
        Camera portalCamera,
        int currentRecursion,
        int maxRecursions,
        ScriptableRenderContext SRC)
    {

        refPos = refPosition;
        refRot = refRotation;
        debugRenderCount = 1;

        // Calculate virtual camera position and rotation

        var virtualPosition = TransformPositionBetweenPortals(this, targetPortal, refPosition);
        var virtualRotation = TransformRotationBetweenPortals(this, targetPortal, refRotation);

        // Setup portal camera for calculations

        portalCamera.transform.SetPositionAndRotation(virtualPosition, virtualRotation);

        // Convert target portal's plane to camera space (relative to target camera)

        var targetViewThroughPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * targetPortal.vectorPlane;

        // Set portal camera projection matrix to clip walls between target portal and target camera
        // Inherits main camera near/far clip plane and FOV settings

        var obliqueProjectionMatrix = mainCamera.CalculateObliqueMatrix(targetViewThroughPlaneCameraSpace);
        portalCamera.projectionMatrix = obliqueProjectionMatrix;

        // Store visible portal resources to release and reset (see function description for details)

        var visiblePortalResourcesList = new List<VisiblePortalResources>();

        // Recurse if not at limit

        if (currentRecursion < maxRecursions)
        {
            foreach (var visiblePortal in targetPortal.visiblePortals)
            {
                visiblePortal.RenderViewthroughRecursive(
                    virtualPosition,
                    virtualRotation,
                    out var visiblePortalTemporaryPoolItem,
                    out var visiblePortalOriginalTexture,
                    out var visiblePortalRenderCount,
                    portalCamera,
                    currentRecursion + 1,
                    maxRecursions,
                    SRC);

                visiblePortalResourcesList.Add(new VisiblePortalResources()
                {
                    OriginalTexture = visiblePortalOriginalTexture,
                    PoolItem = visiblePortalTemporaryPoolItem,
                    VisiblePortal = visiblePortal
                });

                debugRenderCount += visiblePortalRenderCount;
            }
        }
        else
        {
            foreach (var visiblePortal in targetPortal.visiblePortals)
            {
                visiblePortal.ShowViewthroughDefaultTexture(out var visiblePortalOriginalTexture);

                visiblePortalResourcesList.Add(new VisiblePortalResources()
                {
                    OriginalTexture = visiblePortalOriginalTexture,
                    VisiblePortal = visiblePortal
                });
            }
        }

        // Get new temporary render texture and set to portal's material
        // Will be released by CALLER, not by this function. This is so that the caller can use the render texture
        // for their own purposes, such as a Render() or a main camera render, before releasing it.

        temporaryPoolItem = RenderTexturePool.Instance.GetTexture();

        // Use portal camera

        portalCamera.targetTexture = temporaryPoolItem.Texture;
        portalCamera.transform.SetPositionAndRotation(virtualPosition, virtualRotation);
        portalCamera.projectionMatrix = obliqueProjectionMatrix;

        // Render portal camera to target texture
        UniversalRenderPipeline.RenderSingleCamera(SRC, portalCamera);

        // Reset and release

        foreach (var resources in visiblePortalResourcesList)
        {
            // Reset to original texture
            // So that it will remain correct if the visible portal is still expecting to be rendered
            // on another camera but has already rendered its texture. Originally the texture may be overriden by other renders.
            if (viewthroughMaterial != null)
                resources.VisiblePortal.viewthroughMaterial.mainTexture = resources.OriginalTexture;

            // Release temp render texture

            if (resources.PoolItem != null)
            {
                RenderTexturePool.Instance.ReleaseTexture(resources.PoolItem);
            }
        }

        // Must be after camera render, in case it renders itself (in which the texture must not be replaced before rendering itself)
        // Must be after restore, in case it restores its own old texture (in which the new texture must take precedence)
        if (viewthroughMaterial != null)
        {
            originalTexture = viewthroughMaterial.mainTexture;
            viewthroughMaterial.mainTexture = temporaryPoolItem.Texture;
        }
        else
        {
            originalTexture = null;
        }
    }

    private void ShowViewthroughDefaultTexture(out Texture originalTexture)
    {
        if (viewthroughMaterial != null)
        {
            originalTexture = viewthroughMaterial.mainTexture;
            if (viewthroughDefaultTexture)
                viewthroughDefaultTexture.anisoLevel = 8;
            viewthroughMaterial.mainTexture = viewthroughDefaultTexture;
        }
        else
        {
            originalTexture = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var portalableObject = other.GetComponent<PortalableObject>();
        if (portalableObject)
        {
            if (portalableObject.GetComponent<CubeScript>())
            {
                if (!portalableObject.GetComponent<CubeScript>().held)
                    objectsInPortal.Add(portalableObject);
            }
            else
            {
                objectsInPortal.Add(portalableObject);
            }
            Type[] components = { typeof(MeshFilter), typeof(MeshRenderer)};
            GameObject clone = CloneWithComponents(other.gameObject, components);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GameObject.Find(other.name + " clone"))
        {
            GameObject clone = GameObject.Find(other.name + " clone");
            clone.transform.position = TransformPositionBetweenPortals(this, targetPortal, other.transform.position);
            clone.transform.rotation = TransformRotationBetweenPortals(this, targetPortal, other.transform.rotation);
            clone.transform.localScale = other.transform.localScale;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var portalableObject = other.GetComponent<PortalableObject>();
        if (portalableObject)
        {
            if (portalableObject.GetComponent<CubeScript>())
            {
                if (!portalableObject.GetComponent<CubeScript>().held)
                    objectsInPortal.Remove(portalableObject);
            }
            else
            {
                objectsInPortal.Remove(portalableObject);
            }
            if (GameObject.Find(other.name + " clone"))
            {
                GameObject clone = GameObject.Find(other.name + " clone");
                Destroy(clone);
            }
        }
    }

    private void OnDestroy()
    {
        // Destroy cloned material
        //if (viewthroughMaterial != null)
        //Destroy(viewthroughMaterial);
    }

    private void OnDrawGizmos()
    {
        // Linked portals

        if (targetPortal != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPortal.transform.position);
        }

        // Visible portals

        Gizmos.color = Color.blue;
        foreach (var visiblePortal in visiblePortals)
        {
            Gizmos.DrawLine(transform.position, visiblePortal.transform.position);
        }
    }

    private struct VisiblePortalResources
    {
        public Portal VisiblePortal;
        public RenderTexturePool.PoolItem PoolItem;
        public Texture OriginalTexture;
    }

    void LateUpdate()
    {

        // Calculate virtual camera position and rotation

        var virtualPosition = TransformPositionBetweenPortals(this, targetPortal, refPos);
        var virtualRotation = TransformRotationBetweenPortals(this, targetPortal, refRot);

        // Setup portal camera for calculations

        portalCamera.transform.SetPositionAndRotation(virtualPosition, virtualRotation);

        var plane = new Plane(normalVisible.forward, transform.position);
        vectorPlane = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

        // Convert target portal's plane to camera space (relative to target camera)

        var targetViewThroughPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix))
            * targetPortal.vectorPlane;

        // Set portal camera projection matrix to clip walls between target portal and target camera
        // Inherits main camera near/far clip plane and FOV settings

        var obliqueProjectionMatrix = mainCamera.CalculateObliqueMatrix(targetViewThroughPlaneCameraSpace);
        portalCamera.projectionMatrix = obliqueProjectionMatrix;

        // Store visible portal resources to release and reset (see function description for details)

        var visiblePortalResourcesList = new List<VisiblePortalResources>();
        //var plane = new Plane(normalVisible.forward, transform.position + normalInvisible.forward * 0.01f);
        //vectorPlane = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
    }

    private GameObject CloneWithComponents(GameObject source, Type[] componentTypes)
    {
        GameObject cloneInternal = new GameObject(source.name + " clone");
        CopyComponents(source, cloneInternal, componentTypes);
        cloneInternal.layer = 2;
        return cloneInternal;
    }

    public static void CopyComponents(GameObject source, GameObject target, Type[] componentTypes)
    {
        foreach (Type t in componentTypes)
        {
            UnityEditorInternal.ComponentUtility.CopyComponent(source.GetComponent(t));
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
        }
    }

    void OnApplicationQuit()
    {
        //Debug.Log("If this log does not appear, exit unity immediately");
        run = false;
        StopCoroutine(WaitForFixedUpdateLoop());
        this.GetComponent<Portal>().enabled = false;
    }
}
