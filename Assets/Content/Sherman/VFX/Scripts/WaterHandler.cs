using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class WaterHandler : MonoBehaviour
{
    public Transform target;
    public VisualEffect vf;
    public bool debug;
    private bool waterIsOn;

    // Start is called before the first frame update
    void Start()
    {
       // vf.SendEvent()
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();


            if (Physics.Raycast(screenRay, out hit))
            {
                waterIsOn = true;
              //  target.position = hit.point;

                VFXEventAttribute eventAttributes = vf.CreateVFXEventAttribute();
                eventAttributes.SetVector3("targetPosition", hit.point);

                vf.SendEvent("Start", eventAttributes);

                Vector3 difference = hit.point - transform.position;
                float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0.0f, rotationY, 0.0f);

            }
        }

        else if (Input.GetMouseButtonUp(0) && !debug)
        {
            vf.SendEvent("Stop");
            //waterIsOn = false;
        }
    }
}
