using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script for the door: all this contains is an event which plays the random container for the door opening/closing
// This is done in this script rather than in the Drone script, so that the sound spatialises from the door, rather than the moving Drone.
public class Door : MonoBehaviour
{
    public AK.Wwise.Event doorEvent = new AK.Wwise.Event();
}
