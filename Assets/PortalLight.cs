using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalLight : MonoBehaviour
{

    Dictionary<GameObject, GameObject> lightPairs = new Dictionary<GameObject, GameObject>();

    Dictionary<GameObject, int> duplicates = new Dictionary<GameObject, int>();

    public Portal portal;

    // Start is called before the first frame update
    void Start()
    {
        //portal = transform.parent.GetComponent<Portal>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<GameObject, GameObject> g in lightPairs)
        {
            if (g.Value)
            {
                var i = 0;
                Vector3 position = g.Key.transform.position;
                g.Value.transform.position = Portal.TransformPositionBetweenPortals(portal, portal.targetPortal, position);
                foreach (var gs in FindObjectsOfType(typeof(GameObject)) as GameObject[])
                {
                    if (gs.name == g.Key.name + " light clone")
                        i++;
                }
                g.Value.GetComponent<Light>().intensity = (g.Key.GetComponent<Light>().intensity * Portal.PortalScaleRatio(portal, portal.targetPortal)) / i;
                Debug.Log(i);
                //g.Value.GetComponent<Light>().range = g.Key.GetComponent<Light>().range * Portal.PortalScaleRatio(portal, portal.targetPortal);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CubeScript>())
        {
            if (other.GetComponent<CubeScript>().kinematicTime >= Time.fixedUnscaledDeltaTime * 2)
            {
                if (other.GetComponent<Light>() && !lightPairs.ContainsKey(other.gameObject))
                {
                    if (other.GetComponent<Light>().type == LightType.Point)
                    {
                        if (!lightPairs.ContainsKey(other.gameObject))
                        {
                            GameObject lightClone;
                            Type[] components = { typeof(Light) };
                            lightClone = Portal.CloneWithComponents(other.gameObject, components, "light clone");
                            lightClone.GetComponent<Light>().cullingMask = 1 << 8;
                            lightPairs.Add(other.gameObject, lightClone);
                        }

                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CubeScript>())
        {
            if (other.GetComponent<CubeScript>().kinematicTime >= Time.fixedUnscaledDeltaTime * 2)
            {
                if (other.GetComponent<Light>())
                {
                    if (other.GetComponent<Light>().type == LightType.Point)
                    {
                        if (lightPairs.ContainsKey(other.gameObject))
                        {
                            GameObject lightClone;
                            lightPairs.TryGetValue(other.gameObject, out lightClone);
                            lightPairs.Remove(other.gameObject);
                            //duplicates.Remove(other.gameObject);
                            Destroy(lightClone);
                            Debug.Log("destroyed");
                        }
                    }
                }
            }
        }
    }
}
