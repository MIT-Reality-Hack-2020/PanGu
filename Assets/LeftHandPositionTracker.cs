using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public class LeftHandPositionTracker : MonoBehaviour
{
    private float3 currentPosition;
    private float3 lastPosition;

    public AudioSource moveLeft;
    public AudioSource moveRight;
    public AudioSource moveDown;
    
    public AudioClip downSoundL;
    public AudioClip leftSoundL;
    public AudioClip rightSoundL;

    public bool DSisplayed = false;
    public bool LSisplayed = false;
    public bool RSisplayed = false;

    public float difference = 0.01f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentPosition = transform.position;
        
        //move left
        if ((lastPosition.x - currentPosition.x) > difference )
        {
            RSisplayed = false;
            
            if (!LSisplayed)
            {
                moveLeft.PlayOneShot(leftSoundL);
                LSisplayed = true;
            }
        }
        
        //move right
        if ((lastPosition.x - currentPosition.x) < -difference )
        {
            LSisplayed = false;
            
            if (!RSisplayed)
            {
                moveRight.PlayOneShot(rightSoundL);
                RSisplayed = true;
            }
        }
        
        //move down
        if ((lastPosition.y - currentPosition.y) > difference )
        {
            if (!DSisplayed)
            {
                moveDown.PlayOneShot(downSoundL);
                DSisplayed = true;
            }
        }
        
        //move up
        if ((lastPosition.y - currentPosition.y) < -difference )
        {
            DSisplayed = false;
        }

        lastPosition = currentPosition;
    }
}


