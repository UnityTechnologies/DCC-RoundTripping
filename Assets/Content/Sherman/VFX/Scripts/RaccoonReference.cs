using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RaccoonReference : MonoBehaviour
{

    public static Vector3 _raccoonPosition;
    public static bool _isActive;

    void Update()
    {
        _raccoonPosition = transform.position;
    }

    void OnEnable()
    {
        _isActive = true;
    }

    void OnDisable()
    {
        _isActive = false;
    }


}
