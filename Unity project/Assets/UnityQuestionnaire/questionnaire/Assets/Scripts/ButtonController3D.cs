using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController3D : MonoBehaviour
{
    public bool buttonDown = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject finger = collision.collider.gameObject;
        if(finger.tag == "Finger")
        {
            if (!AnyOtherClicked())
            {
                GetComponent<Image>().color = Color.green;
                this.buttonDown = true;
            }
        }
    }

    private bool AnyOtherClicked()
    {
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");
        foreach (GameObject b in buttons)
        {
            if (b == this.gameObject) continue;

            ButtonController3D button = b.GetComponent<ButtonController3D>();
            if(button.buttonDown)
            {
                // someone else is already being clicked
                return true;
            }
        }

        return false;
    }

    void OnCollisionExit(Collision collision)
    {
        GameObject finger = collision.collider.gameObject;
        if (finger.tag == "Finger")
        {
            // trigger button only if no other button is currently pressed
            if (!AnyOtherClicked())
            {
                Button b = GetComponent<Button>();
                b.onClick.Invoke();
                GetComponent<Image>().color = Color.white;
                this.buttonDown = false;
            }
        }
    }
}
