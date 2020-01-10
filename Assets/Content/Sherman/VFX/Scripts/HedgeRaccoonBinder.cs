using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Send the raccoon and hedge position to the shader to rustle the leaves based on proximity. 
/// </summary>

[ExecuteInEditMode]
public class HedgeRaccoonBinder : MonoBehaviour
{
    public Vector3 raccoonPositionOffset;
    private Vector3 _newPos;
    private MaterialPropertyBlock _mpb;
    private Renderer _renderer;
    private bool isOn = false;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!RaccoonReference._isActive && !InitialWigglePosition._isActive)
        {
            // set the "turn off" values to reset the values
            if (isOn)
            {
                isOn = false;
                SetMaterialPropertyBlock(false);
            }
            else
                return;
        }

        // turn the effect back on
        if (RaccoonReference._isActive || InitialWigglePosition._isActive)
            if(!isOn)
                isOn = true;

        if (_renderer == null)
            return;

        if (InitialWigglePosition._isActive)
            _newPos = InitialWigglePosition._wigglePosition;
        else if
            (RaccoonReference._isActive)
            _newPos = RaccoonReference._raccoonPosition + raccoonPositionOffset;

        SetMaterialPropertyBlock(true, _newPos);
    }

    // Set the raccoon position to the origin point if the raccoon is not detected
    void SetMaterialPropertyBlock(bool isOn, Vector3 position = new Vector3())
    {
        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_mpb);

        _mpb.SetVector("_RaccoonPosition", position);

        _mpb.SetVector("_HedgePosition", transform.position);
        _renderer.SetPropertyBlock(_mpb);
    }
}
