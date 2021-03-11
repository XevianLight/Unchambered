using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPair : MonoBehaviour
{
	public Portal3[] Portals { private set; get; }

    private void Awake()
    {
	    Portals = GetComponentsInChildren<Portal3>();

        if(Portals.Length != 2)
        {
            Debug.LogError("PortalPair children must contain exactly two Portal components in total.");
        }
    }
}
