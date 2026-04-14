using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickingathingup : MonoBehaviour
{

    public enum PickUpVariable : int
    {
        NONE = 0,
            PICKEDITUP,
        //..
    }

    public int PickedUp;
    public bool mustBeTrue;
    public string text;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PickedUp = 1;
        }
    }
}
