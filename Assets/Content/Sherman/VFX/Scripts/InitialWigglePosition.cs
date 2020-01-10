using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InitialWigglePosition : MonoBehaviour
{
    public static Vector3 _wigglePosition;
    public static bool _isActive;

    void Update()
    {
        _wigglePosition = transform.position;
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
