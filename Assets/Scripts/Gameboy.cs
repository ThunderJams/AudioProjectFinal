using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        gbTimer += Time.deltaTime;

    }

    public float GetTimer()
    {
        return gbTimer;
    }

    public void StartMusic()
    {
        musicOn.Post(gameObject);
    }
}
