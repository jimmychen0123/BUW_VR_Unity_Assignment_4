using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedUser : MonoBehaviour
{

    public float height = 1.0f; // Start is called before the first frame update

    private GameObject navigator;

 
    private Vector3 startPosition = Vector3.zero;
    private Quaternion startRotation = Quaternion.identity;

    public GameObject jumpingPersonPreview;

    void Start()
    {
        startRotation = transform.rotation;
        navigator = GameObject.Find("XR Rig");
        // Instantiate an object to the right of the current object
        startPosition = navigator.transform.TransformPoint(Vector3.right * 2);
        // Give the avatar height  
        transform.position = new Vector3(startPosition.x, startPosition.y + height, startPosition.z);

        //initiate the second user's avatar preview
        jumpingPersonPreview = Instantiate(Resources.Load("Prefabs/RealisticAvatar"), startPosition, startRotation) as GameObject;
        jumpingPersonPreview.layer = LayerMask.NameToLayer("Ignore Raycast");
        jumpingPersonPreview.SetActive(false);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
