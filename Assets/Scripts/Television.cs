using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TVChannel
{
    None,
    Static,
    News
}

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
