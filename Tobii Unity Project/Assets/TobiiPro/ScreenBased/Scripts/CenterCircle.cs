using System.Collections;
using System.Collections.Generic;
using Tobii.Research.Unity;
using UnityEngine;

public class CenterCircle : MonoBehaviour
{

    public bool GazingCenter = false;

    private SpriteRenderer circleSpriteRenderer;

    private GazePlotter gazePlotter;

    // Start is called before the first frame update
    void Start()
    {
        circleSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        gazePlotter = GameObject.Find("[GazePlot]").GetComponent<GazePlotter>();
    }

    // Update is called once per frame
    void Update()
    {
        var gazepoint = gazePlotter.transform.position;
        if (gazepoint.x * gazepoint.x + gazepoint.y * gazepoint.y < 1.2)
        {
            circleSpriteRenderer.color = Color.green;
            GazingCenter = true;
        }
        else
        {
            circleSpriteRenderer.color = Color.red;
            GazingCenter = false;
        }
    }
}
