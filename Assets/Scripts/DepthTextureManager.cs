using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTextureManager : MonoBehaviour
{

    public RenderTexture[] renderTextures;
    public Vector2 renderScale = Vector2.one;
    Vector2 res;

    // Start is called before the first frame update
    void Start()
    {
        res = new Vector2(Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        if (res.x != Screen.width || res.y != Screen.height)
        {
            res.x = Screen.width;
            res.y = Screen.height;
            foreach (RenderTexture rt in renderTextures)
            {
                rt.width = Mathf.RoundToInt(res.x/renderScale.x);
                rt.height = Mathf.RoundToInt(res.y / renderScale.y);
            }
        }
    }
}
