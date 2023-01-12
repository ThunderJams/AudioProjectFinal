using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script for the GBA - this is mostly referenced directly by the player controller
public class Gameboy : MonoBehaviour
{
    // wwise event for music on
    public AK.Wwise.Event musicOn;
    private float gbTimer = 0;

    // wwise event
    public AK.Wwise.Event pauseEvent = new AK.Wwise.Event();
    public AK.Wwise.Event resumeEvent = new AK.Wwise.Event();

    // Start is called before the first frame update
    void Start()
    {
        // set position to directly infront of the camera
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2;
    }

    // Update is called once per frame
    void Update()
    {
        // Increase the timer representing how long the GBA has been open
        gbTimer += Time.deltaTime;

    }

    // Getter which returns the total time the GBA has been open. Used for the score roundup at the end of the game
    public float GetTimer()
    {
        return gbTimer;
    }

    // Called the first time the GBA is opened. This allows the music to start playing with the intro, rather than skipping directly to the main loop
    public void StartMusic()
    {
        musicOn.Post(gameObject);
    }
}
