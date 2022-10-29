using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    private static InputManager instance;

    public static InputManager newInstance()
    {
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        playerControls = new PlayerControls();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnDisable()
    {
        playerControls.Disable();
    }

    public void OnEnable()
    {
        playerControls.Enable();
    }

    public bool moveLeft()
    {
        return playerControls.player.moveLeft.IsPressed();
    }

    public bool moveRight()
    {
        return playerControls.player.moveRight.IsPressed();
    }

    public bool sprint()
    {
        return playerControls.player.Sprint.IsPressed();
    }

    public bool jump()
    {
        return playerControls.player.Jump.WasPressedThisFrame();
    }

    public bool slide()
    {
        return playerControls.player.Slide.IsPressed();
    }

    public bool pause()
    {
        return playerControls.player.pauseGame.WasPressedThisFrame();
    }
}
