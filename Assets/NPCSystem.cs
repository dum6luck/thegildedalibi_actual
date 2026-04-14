using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    public GameObject Canvas;
    public GameObject ThingtoDisappear;
    public GameObject PickedUpMG;
    bool player_detection = false;
    public pickingathingup patu;



    private void Start()
    {
        patu = GetComponent<pickingathingup>();
    }


    // Update is called once per frame
    void Update()
    {

        if (player_detection && Input.GetKeyDown(KeyCode.E))
        {
            print("PLEASE WORK!");
            Canvas.SetActive(true);
            ThingtoDisappear.SetActive(false);
            PickedUpMG.SetActive(true);
        }
        

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            player_detection = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        player_detection = false;
    }

}
