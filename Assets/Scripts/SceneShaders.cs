using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SceneShaders : MonoBehaviour
{

    MeshRenderer mesh;
    public Material material;
    public Material editorMaterial;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        mesh.material = Application.isPlaying ? material : editorMaterial;
    }
}
