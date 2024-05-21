using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextChanger : MonoBehaviour
{
    public TMP_Text textObject;
    public int timesWatched = 0;

    public void ChangeText()
    {
        timesWatched++;
        textObject.text = "Times observed: " + timesWatched;
    }

}
