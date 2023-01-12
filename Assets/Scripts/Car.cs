using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script attached to the background car
public class Car : MonoBehaviour
{
    float carTimer = 0;
    bool carNoisePlaying = false;
    
    // wwise event
    public AK.Wwise.Event carNoise = new AK.Wwise.Event();
    public Transform carTargetPosition;

    public Transform initialPos;

    // character controller
    CharacterController controller;

    int carTimerThreshold = 10;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // once the car has reached it's final position, reset the car to the initial position
        if (transform.position.x < carTargetPosition.position.x)
        {
            // debug
            Debug.Log("car reached target position");
            carNoisePlaying = false;
            carTimer = 0;
            transform.position = initialPos.position;

            // randomise car timer threshold between 7 and 20
            carTimerThreshold = Random.Range(7, 20);
        }

        // increase the car timer each frame
        carTimer += Time.deltaTime;

        // once the car timer has elapsed, begin moving the car and play the sfx
        if (carTimer > carTimerThreshold)
        {
            if (carNoisePlaying == false)
            {             
                carNoise.Post(gameObject);
                carNoisePlaying = true;
            }

            // move forward
            controller.Move(transform.forward * Time.deltaTime * 3);


        }

        
    }
}
