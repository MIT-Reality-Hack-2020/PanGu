using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fadeAway : MonoBehaviour
{
    public float lifeSpan = 0f;
    public GameObject tutorial;

    public Animator changeElement;
    public Animator shoot;
    public Animator pinch;
    public Animator point;
    public Animator fist;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(lifeSpan);

        lifeSpan += Time.deltaTime;

        if (lifeSpan >= 60f)
        {
            changeElement.SetBool("Fade", true);
            shoot.SetBool("Fade", true);
            pinch.SetBool("Fade", true);
            point.SetBool("Fade", true);
            fist.SetBool("Fade", true);
        }
    }
}
