using UnityEngine;

public class NearClipPlane : MonoBehaviour
{
	public float nearClipPlane = 0.000001f;
	public float farClipPlane = 1000;
    
	private void Update()
	{
		GetComponent<Camera>().nearClipPlane = nearClipPlane;
		GetComponent<Camera>().farClipPlane = farClipPlane;
	}
}