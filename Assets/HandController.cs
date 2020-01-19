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
        Water, Fire, Wood, Earth, Metal
    }

    public enum Gesture {
        None, Fist, Handgun, Pinch, Point
    }

    public Hand hand;

    public Transform spawnPoint;

    private ParticleSimManagerGPU particleSimManagerGPU;
    private ParticleSimManager    particleSimManager;

    public Element currentElement;
    
    private void Start() {
        particleSimManagerGPU = ParticleSimManagerGPU.instance;
        particleSimManager    = ParticleSimManager   .instance;
    }

    private void CycleElement() {
        switch (currentElement) {
            case Element.Water:
                currentElement = Element.Earth;
                break;
            case Element.Earth:
                currentElement = Element.Wood;
                break;
            case Element.Wood:
                //currentElement = Element.Metal;
                currentElement = Element.Fire;
                break;
            case Element.Fire:
                currentElement = Element.Water;
                break;
            case Element.Metal:
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
                       OVRInput.Get(OVRInput.RawButton.LHandTrigger)  &&  OVRInput.Get(OVRInput.RawTouch.Y);
            handgun = !OVRInput.Get(OVRInput.RawButton.LIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.LHandTrigger)  && !OVRInput.Get(OVRInput.RawTouch.Y);
            pinch   =  OVRInput.Get(OVRInput.RawButton.LIndexTrigger) &&
                      !OVRInput.Get(OVRInput.RawButton.LHandTrigger)  &&  OVRInput.Get(OVRInput.RawTouch.Y);
            point   = !OVRInput.Get(OVRInput.RawButton.LIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.LHandTrigger)  &&  OVRInput.Get(OVRInput.RawTouch.Y);
            
            if (OVRInput.GetDown(OVRInput.RawButton.X))
                CycleElement();
        } else {
            fist     = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.RHandTrigger)  &&  OVRInput.Get(OVRInput.RawTouch.B);
            handgun = !OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.RHandTrigger)  && !OVRInput.Get(OVRInput.RawTouch.B);
            pinch   =  OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                      !OVRInput.Get(OVRInput.RawButton.RHandTrigger)  &&  OVRInput.Get(OVRInput.RawTouch.B);
            point   = !OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                       OVRInput.Get(OVRInput.RawButton.RHandTrigger)  &&  OVRInput.Get(OVRInput.RawTouch.B);
            
            if (OVRInput.GetDown(OVRInput.RawButton.A))
                CycleElement();
        }

        float handTrigger = (hand == Hand.Left) ? OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) : OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);

        //if (handgun) {
        //    particleSimManagerGPU?.SpawnParticle(spawnPoint.position, transform.forward, (handTrigger - 0.25f) * 10f, currentElement);
        //}

        Gesture gesture = Gesture.None;
        if (fist) {
            gesture = Gesture.Fist;
        } else if (handgun) {
            gesture = Gesture.Handgun;
        } else if (pinch) {
            gesture = Gesture.Pinch;
        } else if (point) {
            gesture = Gesture.Point;
        }

        particleSimManagerGPU.SetHandState(hand, gesture, spawnPoint.position, transform.forward, handTrigger, currentElement);
    }
}
