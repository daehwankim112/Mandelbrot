﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explorer : MonoBehaviour {

    public Material mat;
    public Vector2 pos;
    public float scale;
    public float timespeed = 0.001f;

    private float timeDiff = 0;
    private float magicDiff = 0;
    private Vector2 smoothPos;
    private float smoothScale;
    private Vector2 firstTouch;
    private float ZoomedBy;
    private float scaleX, scaleY;
    private static bool paused;
    private int TapCount;
    private float MaxDubbleTapTime;
    private float NewTimeUntilDoubleTap;
    private bool waitingForSecondTap;
    private bool buffer;

    void Start()
    {
        TapCount = 0;
        MaxDubbleTapTime = 0.7f;
        waitingForSecondTap = false;
        scale = 5;
    }

    //Update location of centre and size of render
    private void UpdateShaer()
    {
        //These are for smoothing user interactions. Put these back if fps is good.
        //smoothPos = Vector2.Lerp(smoothPos, pos, .09f);
        //smoothScale = Mathf.Lerp(smoothScale, scale, .05f);

        float aspect = (float)Screen.width / (float)Screen.height; //These are integer so it should be casted to float otherwise, it will be truncated.

        scaleX = scale;
        scaleY = scale;

        if (aspect > 1f)
            scaleY /= aspect;
        else
            scaleX *= aspect;

        mat.SetVector("_Area", new Vector4(pos.x, pos.y, scaleX, scaleY));
    }

    //Pause game if it is double tapped
    private void DoubleTap()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended && Time.time > NewTimeUntilDoubleTap - MaxDubbleTapTime + 0.1f)
            {
                TapCount += 1;
                if ( TapCount == 1 )
                    waitingForSecondTap = true;
                else
                    waitingForSecondTap = false;
            }
            if (TapCount == 1 && waitingForSecondTap)
            {
                NewTimeUntilDoubleTap = Time.time + MaxDubbleTapTime;
                waitingForSecondTap = false;
            }
            else if (TapCount == 2 && Time.time <= NewTimeUntilDoubleTap)
            {
                //Double tapped. Pause the game
                PauseSetting.ChangeGameStatue(1);
                waitingForSecondTap = false;
                TapCount = 0;
            }
        }
        if (Time.time > NewTimeUntilDoubleTap && TapCount > 0)
        {
            TapCount = 0;
            waitingForSecondTap = false;
        }
    }

    // The difference between update and fixed update is that the fixed update execute fixed amount of update per seconds
    //while normal update depends on how fast your computer is.
    void FixedUpdate()
    {
        //Check if game is paused.
        paused = PauseSetting.GameIsPaused;

        //HandleInputs();
        UpdateShaer();
        if (!paused)
        {
            DoubleTap();
        }
        
        //Make time pass or changes color by toggle button
        if ( mat.GetFloat("_TimePass") == 1 ) {
            timeDiff += timespeed;
        }
        Shader.SetGlobalFloat("color_diff", timeDiff);
        if (mat.GetFloat("_Magic") == 1)
        {
            if (buffer)
            {
                magicDiff += timespeed;
                buffer = false;
            }
            else
            {
                buffer = true;
            }
        }
        Shader.SetGlobalFloat("magic_diff", magicDiff);

        //Scroll
        if (Input.touchCount >= 1 && !paused )
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                firstTouch = Input.GetTouch(0).position;
            }
                
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                //Vector of movement from first touch to new touch position is calculated
                Vector2 movement = new Vector2(scale * (Input.GetTouch(0).deltaPosition.x) / ((float)Screen.width) / 2, 
                    scaleY * (Input.GetTouch(0).deltaPosition.y) / ((float)Screen.height) / 2);

                pos -= movement;
            }
        }
    }
}
