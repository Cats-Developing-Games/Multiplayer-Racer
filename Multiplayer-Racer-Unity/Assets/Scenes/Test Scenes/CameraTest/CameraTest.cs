using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTest : MonoBehaviour
{
    [SerializeField]
    private Image img;

    // Start is called before the first frame update
    void Start()
    {
        img.color = Random.ColorHSV();
    }
}
