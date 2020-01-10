using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

[ExecuteInEditMode]
public class WaterBinder : MonoBehaviour
{
    public GameObject[] controlPoints;
    private VisualEffect _VisualEffect;
    private Transform _currentTransform;

    public bool setPosition, setAngle = false;

    public string positionName = "Sprinkler_position";
    public string rotationName = "Sprinkler_angles";

    private int positionNameID;
    private int rotationNameID;


    // Start is called before the first frame update
    void Start()
    {
        positionNameID = Shader.PropertyToID(positionName);
        rotationNameID = Shader.PropertyToID(rotationName);

        _VisualEffect = GetComponent<VisualEffect>();
        GetActiveTransform();
    }

    void OnEnable()
    {
        // When the effect is re-activated, do a new grab for the active source
        GetActiveTransform();
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentTransform == null || !_currentTransform.gameObject.activeInHierarchy)
           GetActiveTransform();

        if (_currentTransform == null)
            return;

        if(setPosition)
            _VisualEffect.SetVector3(positionName, _currentTransform.position);

        if (setAngle)
            _VisualEffect.SetVector3(rotationName, _currentTransform.eulerAngles);
    }

    void GetActiveTransform()
    {
        for (int i = 0; i < controlPoints.Length; i++)
        {
            if (controlPoints[i].activeInHierarchy)
            {
                _currentTransform = controlPoints[i].transform;
                break;
            }
        }
    }
}
