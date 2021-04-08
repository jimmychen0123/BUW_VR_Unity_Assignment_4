using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class JumpingScript : MonoBehaviour
{
    private GameObject mainCamera = null;
    private GameObject platformCenter = null;
    private GameObject rightHandController = null;
    private XRController rightXRController = null;
    
    private Vector3 startPosition = Vector3.zero;
    private Quaternion startRotation = Quaternion.identity;
    private Quaternion rotTowardsHit = Quaternion.identity;

    public bool triggerPressed = false;
    public bool triggerReleased = false;
    private bool secondaryButtonLF = false;
    private Vector3 jumpingTargetPosition;
    private Vector3 centerOffset;

    private LineRenderer rightRayRenderer;
    private LineRenderer offsetRenderer;

    private bool rayOnFlag = false;

    public LayerMask myLayerMask;

    private GameObject rightRayIntersectionSphere = null;
    private GameObject jumpingPositionPreview = null;
    private GameObject jumpingPersonPreview = null;

    private RaycastHit hit;

    // YOUR CODE (IF NEEDED) - BEGIN 
    private float height = 1.0f;
    private Vector3 avatarDirection;
    //fix the rotation offset
    private Quaternion camLocalRot;
    private Quaternion platformLocalRot;
    private Vector3 goalAngle;
    private Vector3 turnAngle;

    public GameObject simulatedUser;
    
    private Vector3 sUjumpingTargetPosition;

    private GameObject sUjumpingPersonPreview = null;

    private Vector3 navRelative;

    //Joystick threshold
    public float threshold;
    private bool formationFlag = false;
    private Vector3 sUavatarDirection;
    private Quaternion sUrotTowardsHit = Quaternion.identity;
    

    // YOUR CODE - END    

    // Start is called before the first frame update
    void Start()
    {
        threshold = 0.0005f;
        startPosition = transform.position;
        startRotation = transform.rotation;

        mainCamera = GameObject.Find("Main Camera");
        platformCenter = GameObject.Find("Center");
        rightHandController = GameObject.Find("RightHand Controller");
        offsetRenderer = GetComponent<LineRenderer>();
        offsetRenderer.startWidth = 0.01f;
        offsetRenderer.positionCount = 2;

        camLocalRot = mainCamera.transform.localRotation;
        platformLocalRot = transform.localRotation;

        simulatedUser = GameObject.Find("Simulated User");
        sUjumpingPersonPreview = simulatedUser.GetComponent<SimulatedUser>().jumpingPersonPreview;

        navRelative = transform.InverseTransformPoint(simulatedUser.transform.position);
        


        if (rightHandController != null) // guard
        {
            rightXRController = rightHandController.GetComponent<XRController>();
            rightRayRenderer = rightHandController.AddComponent<LineRenderer>();
            rightRayRenderer.name = "Right Ray Renderer";
            rightRayRenderer.startWidth = 0.01f;
            rightRayRenderer.positionCount = 2;
            rayOnFlag = true;

            // geometry for intersection visualization
            rightRayIntersectionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightRayIntersectionSphere.name = "Right Ray Intersection Sphere";
            rightRayIntersectionSphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            rightRayIntersectionSphere.GetComponent<MeshRenderer>().material.color = Color.blue;
            rightRayIntersectionSphere.GetComponent<SphereCollider>().enabled = false; // disable for picking ?!
            rightRayIntersectionSphere.SetActive(false); // hide

            // geometry for Navidget visualization
            Material previewMaterial = new Material(Shader.Find("Standard"));
            previewMaterial.color = new Color(1.0f, 0.0f, 0.0f, 0.4f);
            previewMaterial.SetOverrideTag("RenderType", "Transparent");
            previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            previewMaterial.SetInt("_ZWrite", 0);
            previewMaterial.DisableKeyword("_ALPHATEST_ON");
            previewMaterial.DisableKeyword("_ALPHABLEND_ON");
            previewMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            previewMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            jumpingPositionPreview = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            jumpingPositionPreview.transform.localScale = new Vector3(1f, 0.02f, 1f);
            jumpingPositionPreview.name = "Navidget Intersection Sphere";
            jumpingPositionPreview.layer= LayerMask.NameToLayer("Ignore Raycast");
            jumpingPositionPreview.GetComponent<MeshRenderer>().material = previewMaterial;
            jumpingPositionPreview.SetActive(false); // hide

            jumpingPersonPreview = Instantiate(Resources.Load("Prefabs/RealisticAvatar"), startPosition, startRotation) as GameObject;
            jumpingPersonPreview.layer = LayerMask.NameToLayer("Ignore Raycast");
            jumpingPersonPreview.SetActive(false);

            // YOUR CODE (IF NEEDED) - BEGIN 

            // YOUR CODE - END    

        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateOffsetToCenter();

        if (rightHandController != null) // guard
        {
            // mapping: grip button (middle finger)
            //float grip = 0.0f;
            //rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.grip, out grip);

            // mapping: joystick
            //Vector2 joystick;
            //rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick);

            // mapping: primary button (A)
            //bool primaryButton = false;
            //rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton);

            // mapping: trigger (index finger)
            float trigger = 0.0f;
            rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out trigger);

            UpdateRayVisualization(trigger, 0.00001f);

            // YOUR CODE - BEGIN
            //before teleporting, check if the trigger button is fully pressed 
            if (rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed)
                &&
                triggerPressed)
            {
                //https://gamedevbeginner.com/coroutines-in-unity-when-and-how-to-use-them/
                StartCoroutine(Teleport());
            }//end if trigger button pressed
            // YOUR CODE - END    

            // mapping: secondary button (B)
            bool secondaryButton = false;
            rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButton);

            if (secondaryButton != secondaryButtonLF) // state changed
            {
                if (secondaryButton) // up (0->1)
                {
                    ResetXRRig();
                }
            }

            secondaryButtonLF = secondaryButton;
        }
    }

    
    private void UpdateOffsetToCenter()
    {
        // Calculate the offset between the platform center and the camera in the xz plane
        Vector3 a = transform.position;
        Vector3 b = new Vector3(mainCamera.transform.position.x, this.transform.position.y, mainCamera.transform.position.z);
        centerOffset = b - a;

        // visualize the offset as a line on the ground
        offsetRenderer.positionCount = 2; // line renderer visualizes a line between N (here 2) vertices
        offsetRenderer.SetPosition(0, a); // set pos 1
        offsetRenderer.SetPosition(1, b); // set pos 2

    }

    private void UpdateRayVisualization(float inputValue, float threshold)
    {
        // Visualize ray if input value is bigger than a certain treshhold
        if (inputValue > threshold && rayOnFlag == false)
        {
            rightRayRenderer.enabled = true;
            rayOnFlag = true;
        }
        else if (inputValue < threshold && rayOnFlag)
        {
            rightRayRenderer.enabled = false;
            rayOnFlag = false;
        }

        // update ray length and intersection point of ray
        if (rayOnFlag)
        { // if ray is on

            // Check if something is hit and set hit point
            if (Physics.Raycast(rightHandController.transform.position,
                                rightHandController.transform.TransformDirection(Vector3.forward),
                                out hit, Mathf.Infinity, myLayerMask))
            {
                rightRayRenderer.SetPosition(0, rightHandController.transform.position);
                rightRayRenderer.SetPosition(1, hit.point);

                rightRayIntersectionSphere.SetActive(true);
                rightRayIntersectionSphere.transform.position = hit.point;
            }
            else
            { // if nothing is hit set ray length to 100
                rightRayRenderer.SetPosition(0, rightHandController.transform.position);
                rightRayRenderer.SetPosition(1, rightHandController.transform.position + rightHandController.transform.TransformDirection(Vector3.forward) * 100);

                rightRayIntersectionSphere.SetActive(false);
            }
        }
        else
        {
            rightRayIntersectionSphere.SetActive(false);
        }
    }

    // YOUR CODE (ADDITIONAL FUNCTIONS)- BEGIN
    
    IEnumerator Teleport()
    {
        //store jumping location while preview is not yet activated
        //and the postion is located(intersection)
        while (!jumpingPositionPreview.activeSelf && rightRayIntersectionSphere.activeSelf )
        {
            //this allows to store the jumping location while user slightly presss the trigger for ray intersection
            setJumpingPosition();
            //set and activate the preview 
            setJumpingPositionPersonPreview();

            //caculate the second user's position relative to navigator
            
            Debug.Log("relative position: " + navRelative);
            //sUjumpingPersonPreview.transform.position = jumpingPersonPreview.transform.TransformPoint(navRelative);
            //sUjumpingPersonPreview.transform.Translate(Vector3.down * height);
            sUjumpingPersonPreview.SetActive(true);



            //what follow yield return will specify how long Unity will wait before continuing
            //execution will pause and be resumed the following frame
            yield return null;

        }
        //update the avatars direction
        while (rayOnFlag)
        {
            

            //store the avatars direction before releasing the trigger button
            // Determine which direction to rotate towards
            //https://answers.unity.com/questions/254130/how-do-i-rotate-an-object-towards-a-vector3-point.html
            //https://docs.unity3d.com/ScriptReference/Quaternion.Slerp.html

            avatarDirection = (rightRayIntersectionSphere.transform.position -jumpingPersonPreview.transform.position).normalized;

            rotTowardsHit = Quaternion.LookRotation(avatarDirection, Vector3.up );
            
            //only rotate around y axis
            rotTowardsHit = Quaternion.Euler(0f, rotTowardsHit.eulerAngles.y, 0f);

            jumpingPersonPreview.transform.rotation = Quaternion.Slerp(jumpingPersonPreview.transform.rotation, rotTowardsHit, Time.deltaTime );

            //set the initial formation
            if (!formationFlag)
            {
                
                sUjumpingPersonPreview.transform.position = jumpingPersonPreview.transform.TransformPoint(navRelative);
                sUjumpingPersonPreview.transform.Translate(Vector3.down * height);

                sUavatarDirection = (rightRayIntersectionSphere.transform.position - sUjumpingPersonPreview.transform.position).normalized;

                sUrotTowardsHit = Quaternion.LookRotation(sUavatarDirection, Vector3.up);


                sUrotTowardsHit = Quaternion.Euler(0f, sUrotTowardsHit.eulerAngles.y, 0f);

                sUjumpingPersonPreview.transform.rotation = Quaternion.Slerp(sUjumpingPersonPreview.transform.rotation, sUrotTowardsHit, Time.deltaTime);

            }
            
            //updating the formation with the change of navigator
            // mapping: joystick
            Vector2 joystick;

            //rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick);
            if (rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick)
                && joystick.magnitude > threshold)
                {
                    Debug.Log("Joystick: " + joystick.magnitude);
                    formationFlag = true;
                    float speed = 2.0f;
                    speed *= Time.deltaTime;
                    sUjumpingPersonPreview.transform.Translate(speed * joystick.x, 0f, speed * joystick.y);

                }
                else
                {

                }



            //https://docs.unity3d.com/ScriptReference/WaitUntil.html
            yield return new WaitUntil(() => !triggerPressed);

            //while fully press the trigger button, if there is no ray intersection, user would not be teleported but facing the ray direction 
        }

        UpdateUserPositionDirection();
        jumpingPositionPreview.SetActive(false);
        jumpingPersonPreview.SetActive(false);
        

    }

    private void setSUJumpingPositionPersonPreview()
    {
        
       
        
    }

    private void setJumpingPosition()
    {
        
        jumpingTargetPosition = hit.point;

    }// end setJumpingPosition()


    private void setJumpingPositionPersonPreview()
    {
        jumpingPositionPreview.transform.position = jumpingTargetPosition;
        jumpingPositionPreview.SetActive(true);
        //set the distance from avatar to position preview sphere
        jumpingPersonPreview.transform.position = new Vector3(jumpingTargetPosition.x, jumpingTargetPosition.y + height, jumpingTargetPosition.z);
        jumpingPersonPreview.SetActive(true);




    }//end UpdateJumpingPositionPreview()


    private void UpdateUserPositionDirection()
    {

        gameObject.transform.position = jumpingTargetPosition - centerOffset;
        // compensate rotation offset (between platform center and HMD/camera)
        Quaternion offsetRot = transform.rotation * Quaternion.Inverse(mainCamera.transform.rotation);
        offsetRot = Quaternion.Euler(0.0f, offsetRot.eulerAngles.y, 0.0f); // offset compensation only for YAW rotation
        transform.rotation = rotTowardsHit * offsetRot;
        //gameObject.transform.rotation = rotTowardsHit;
        //transform.Rotate(-turnAngle);

    }
    // YOUR CODE - END 

    private void ResetXRRig()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}
