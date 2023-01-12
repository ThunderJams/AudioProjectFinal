using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TVChannel
{
    None,
    Static,
    News
}

// Similar to the Door class, this script simply stores the events, states and switches for the TV
// This is A) to keep all tv related variables organised and B) to spatialise all of the sounds from the tv
// The TV has cone attenuation so that a LPF is applied if you are standing behind the TV
public class Television : MonoBehaviour
{
    /// The Wwise event to trigger a footstep sound.
	public AK.Wwise.Event tv = new AK.Wwise.Event();
	public AK.Wwise.Event tvOff = new AK.Wwise.Event();

    // wwise switch for news
    public AK.Wwise.Switch newsSwitch = new AK.Wwise.Switch();

    public bool isTVon = false;
    public TVChannel currentChannel = TVChannel.Static;
}
