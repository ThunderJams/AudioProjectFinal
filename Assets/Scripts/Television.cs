using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Television : MonoBehaviour
{
    /// The Wwise event to trigger a footstep sound.
	public AK.Wwise.Event tv = new AK.Wwise.Event();
	public AK.Wwise.Event tvOff = new AK.Wwise.Event();

    public bool isTVon = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
