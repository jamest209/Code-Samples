using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour {

    public GameObject pause_screen;
    public static bool paused;
    public bool speeding_up = false;

	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
            if (!paused)
            {
                Resume();
            }
        }

        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            SpeedUp();
            speeding_up = true;
        }

        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            SlowDown();
            speeding_up = false;
        }
        else
        {
            //add any fixes here.
        }


        if (paused)
        {
            Time.timeScale = 0f;
            pause_screen.SetActive(true);
        }
        else if (!paused && !speeding_up)
        {
            Resume();
        }
	}

    public static void TogglePause()
    {
        paused = !paused;
    }

    public static void SpeedUp()
    {
        if(!paused)
        {
            Time.timeScale = 2f;
        }
    }
    
    public static void SlowDown()
    {
        if (!paused)
        {
            Time.timeScale = 1f;
        }
            
    }


    public void Resume()
    {
        Time.timeScale = 1f;
        pause_screen.SetActive(false);
    }

    private void Awake()
    {
        paused = false;
    }
}
