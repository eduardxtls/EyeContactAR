using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanging : MonoBehaviour
{
    public Material GreenMaterial;
    public Material RedMaterial;
    public Material BlueMaterial;
    public Material YellowMaterial;
    private Material PreviousMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        PreviousMaterial = GetComponent<Renderer>().material;
    }

    public void TurnRed()
    {
        PreviousMaterial = GetComponent<Renderer>().material;

        GetComponent<Renderer>().material = RedMaterial;
    }

    public void TurnBlue()
    {
        PreviousMaterial = GetComponent<Renderer>().material;

        GetComponent<Renderer>().material = BlueMaterial;
    }

    public void TurnGreen()
    {
        PreviousMaterial = GetComponent<Renderer>().material;

        GetComponent<Renderer>().material = GreenMaterial;
    }

    public void TurnYellow()
    {
        PreviousMaterial = GetComponent<Renderer>().material;

        GetComponent<Renderer>().material = YellowMaterial;
    }

    public void TurnBack()
    {
        GetComponent<Renderer>().material = PreviousMaterial;
    }
}
