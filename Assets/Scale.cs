using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale : MonoBehaviour
{

    public GameObject scalePlaceHolder;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        scalePlaceHolder.transform.localScale = transform.lossyScale;
    }
}
