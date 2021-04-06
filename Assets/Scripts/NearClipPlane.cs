using UnityEngine;

public class NearClipPlane : MonoBehaviour
{
	public float nearClipPlane = 0.000001f;
	public float farClipPlane = 1000;
	public Camera cam;

	private void Update()
	{
		GetComponent<Camera>().nearClipPlane = nearClipPlane;
		GetComponent<Camera>().farClipPlane = farClipPlane;
		//if (cam)
		//cam.rect = new Rect(0, 0, 1, 2);
		if (cam)
			cam.aspect = Camera.main.aspect;
	}
}