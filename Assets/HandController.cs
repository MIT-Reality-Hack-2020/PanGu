using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public enum Hand {
        Left, Right
    }

    public enum Element {
        Water, Fire, Wood
    }

    public Hand hand;

    public Transform spawnPoint;

    private ParticleSimManagerGPU particleSimManager;

    public Element currentElement;
    
    private void Start() {
        particleSimManager = ParticleSimManagerGPU.instance;
    }

    private void CycleElement() {
        switch (currentElement) {
            case Element.Water:
                currentElement = Element.Fire;
                break;
            case Element.Fire:
                currentElement = Element.Wood;
                break;
            case Element.Wood:
                currentElement = Element.Water;
                break;
        }
    }

    private void Update() {
        bool fist;
        bool handgun;
        bool pinch;
        bool point;

        if (hand == Hand.Left) {
            fist    =  OVRInput.Get(OVRInput.RawButton.LIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.LHandTrigger)  &&  OVRInput.Get(OVRInput.RawButton.Y);
            handgun = !OVRInput.Get(OVRInput.RawButton.LIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.LHandTrigger)  && !OVRInput.Get(OVRInput.RawButton.Y);
            pinch   =  OVRInput.Get(OVRInput.RawButton.LIndexTrigger) &&
                      !OVRInput.Get(OVRInput.RawButton.LHandTrigger)  &&  OVRInput.Get(OVRInput.RawButton.Y);
            point   = !OVRInput.Get(OVRInput.RawButton.LIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.LHandTrigger)  &&  OVRInput.Get(OVRInput.RawButton.Y);
            
            if (OVRInput.GetDown(OVRInput.RawButton.X))
                CycleElement();
        } else {
            fist     = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.RHandTrigger)  &&  OVRInput.Get(OVRInput.RawButton.B);
            handgun = !OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.RHandTrigger)  && !OVRInput.Get(OVRInput.RawButton.B);
            pinch   =  OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                      !OVRInput.Get(OVRInput.RawButton.RHandTrigger)  &&  OVRInput.Get(OVRInput.RawButton.B);
            point   = !OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.RHandTrigger)  &&  OVRInput.Get(OVRInput.RawButton.B);
            
            if (OVRInput.GetDown(OVRInput.RawButton.A))
                CycleElement();
        }

        float handTrigger = (hand == Hand.Left) ? OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) : OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);

        if (handgun) {
            particleSimManager.SpawnParticle(spawnPoint.position, 10f * (handTrigger - 0.35f) * transform.forward, currentElement);
        }
    }
}
