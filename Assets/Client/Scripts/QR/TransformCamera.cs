using Microsoft.MixedReality.SampleQRCodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformCam : MonoBehaviour
{
    public GameObject anchor;
    public bool transformed = false;
    private Vector3 relativePos;
    private Vector3 relativeDirection;
    private Quaternion relativeRotation;

    public Vector3 RelativePos { get => relativePos; set => relativePos = value; }
    public Quaternion RelativeRotation { get => relativeRotation; set => relativeRotation = value; }

    private static TransformCam _singleton;
    public static TransformCam Singleton
    {
        get => _singleton;
        private set

        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != null)
            {
                Debug.Log($"{nameof(TransformCam)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }
    public void Awake()
    {
        Singleton = this;
    }

    public void transformCam()
    {
        transformed = true;

        RelativePos = anchor.transform.InverseTransformPoint(Vector3.zero);

        RelativeRotation = Quaternion.Inverse(anchor.transform.rotation) * Quaternion.identity;

        Debug.Log($"Get Position of anchor {anchor.transform.position}");
        Debug.Log($"Get  Rotation of anchor {anchor.transform.rotation}");

        Debug.Log($"Get Relative Position from Origin {RelativePos}");
        Debug.Log($"Get Relative Rotation from origin {RelativeRotation}");

    }
}