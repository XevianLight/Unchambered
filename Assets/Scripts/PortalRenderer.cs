using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
//using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;



public class PortalRenderer : MonoBehaviour
{
	public Camera portalCamera;
	public int maxRecursions = 2;

	public int debugTotalRenderCount;

	private Camera mainCamera;
	private PortalOcclusionVolume[] occlusionVolumes;
	
	private void Start()
	{
		mainCamera = Camera.main;
		occlusionVolumes = FindObjectsOfType<PortalOcclusionVolume>();
		//RenderPipeline.beginCameraRendering(GetComponent<Camera>());
		RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
		RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
	}

	private void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}
	private void OnEnable()
	{
		//RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
		//RenderPipelineManager.beginCameraRendering -= EndCameraRendering;
	}
	private void OnDisable()
	{
		//RenderPipelineManager.beginCameraRendering += EndCameraRendering;
		//RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
	}

	private void OnBeginCameraRendering(ScriptableRenderContext SRC, Camera camera)
	{
				if (Application.isPlaying){
		debugTotalRenderCount = 0;

		PortalOcclusionVolume currentOcclusionVolume = null;

		foreach (var occlusionVolume in occlusionVolumes)
		{
			if (occlusionVolume.collider.bounds.Contains(mainCamera.transform.position))
			{
				currentOcclusionVolume = occlusionVolume;
				break;
			}
		}

		if (currentOcclusionVolume != null)
		{
			var cameraPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

			foreach (var portal in currentOcclusionVolume.portals)
			{
				if (!portal.ShouldRender(cameraPlanes)) continue;

				portal.RenderViewthroughRecursive(
					mainCamera.transform.position,
					mainCamera.transform.rotation,
					out _,
					out _,
					out var renderCount,
					portalCamera,
					0,
					maxRecursions,
					SRC);

				debugTotalRenderCount += renderCount;
			}
		}
		}
	}

	private void OnEndCameraRendering(ScriptableRenderContext SRC, Camera camera)
	{
		RenderTexturePool.Instance.ReleaseAllTextures();
	}
}