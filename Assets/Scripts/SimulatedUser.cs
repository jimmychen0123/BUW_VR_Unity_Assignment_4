using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedUser : MonoBehaviour
{

    public float height = 1.0f; // Start is called before the first frame update

    private GameObject navigator;

 
    private Vector3 startPosition;

    public GameObject jumpingPersonPreview;

    void Start()
    {
        navigator = GameObject.Find("XR Rig");
        // Instantiate an object to the right of the current object
        startPosition = navigator.transform.TransformPoint(Vector3.right * 2);
        
        
        transform.position = new Vector3(startPosition.x, startPosition.y + height, startPosition.z);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
