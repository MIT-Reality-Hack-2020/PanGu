using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LeftHanController : MonoBehaviour
{
    public bool checkfist;    
    
    
    public GameObject cube;
    // public GameObject sphere;
    // public GameObject capsule;
    // public GameObject cylinder;

    //public GameObject stone;
    //public Material gold;
    // Update is called once per frame

    public AudioSource fistSource;
    public AudioSource gunSoure;

    public AudioClip fistSound;
    public AudioClip spray;

    public bool fistIsPlayed = false;
    public bool sprayIsPlayed = false;

    void Start()
    {
        checkfist = false;
    }

    void Update()
    {

        //Fist Gesture
        if(OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.RawButton.Y))
        {
            if (!fistIsPlayed)
            {
                fistIsPlayed = true;
                //fistSource.PlayOneShot(fistSound);
                fistSource.Play();
                fistSource.loop = true;
            }

            //Debug.Log("1");
            checkfist = true;
            //cube.SetActive(true);
        }
        else
        {
            fistIsPlayed = false;
            fistSource.Stop();
            fistSource.loop = false;
            
            //cube.SetActive(false);
        }


        //Handgun Gesture
        if(!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && !OVRInput.Get(OVRInput.RawButton.Y))
        {
            if (!sprayIsPlayed)
            {
                sprayIsPlayed = true;
                //gunSoure.PlayOneShot(spray);
                gunSoure.Play();
                gunSoure.loop = true;
            }
        }
        else
        {
            sprayIsPlayed = false;
            gunSoure.Stop();
            gunSoure.loop = false;
        }


        //Pinch Gesture
        if(OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && !OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.RawButton.Y))
        {
            
        }
        else 
        {
            
        }

        //Point Gesture
        if(!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.RawButton.Y))
        {
            
        }
        else 
        {
            
        }
    }
}
