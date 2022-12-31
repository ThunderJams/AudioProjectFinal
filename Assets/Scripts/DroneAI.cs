using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAI : MonoBehaviour
{
    CharacterController cc;
    float internalTimer = 0;
    bool goingLeft = true;

    /// The Wwise event to trigger a footstep sound.
	public AK.Wwise.Event footstepSound = new AK.Wwise.Event();

    /// The speed at which footstep sounds are triggered.
    [Range(0.01f, 1.0f)]
    public float footstepRate = 0.3f;

    ///	Used to determine when to trigger footstep sounds.
	private bool walking = true;
    ///	Used to determine when to trigger footstep sounds.
    private float walkCount = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        internalTimer += Time.deltaTime;
        // alternate moving left and right on a cycle
        if (internalTimer > 3)
        {
            internalTimer = 0;
            goingLeft = !goingLeft;
        }

        if (goingLeft)
        {
            cc.Move(Vector3.left * Time.deltaTime);
        }
        else
        {
            cc.Move(Vector3.right * Time.deltaTime);
        }

        if (walking)
        {
            walkCount += Time.deltaTime;

            if (walkCount > footstepRate)
            {
                footstepSound.Post(gameObject);

                walkCount = 0.0f;
            }
        }

    }
}
