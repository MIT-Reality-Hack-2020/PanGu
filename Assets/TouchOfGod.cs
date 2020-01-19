using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchOfGod : MonoBehaviour
{
    public GameObject LHandController;
    public GameObject Stone;
    public Material gold;


    void OnTriggerEnter(Collider col)
    {
        if(LHandController.GetComponent<LeftHanController>().checkfist == true && col.gameObject.CompareTag("obj1"))
        {
            Stone.GetComponent<Renderer>().material = gold;
        }
    }
}
