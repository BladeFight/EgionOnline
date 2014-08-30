#pragma strict


function Update () {

if (Input.GetKey("1"))
    {
 
    animation.Play("open");
    }

if (Input.GetKey("2"))
    {
    animation.Play("close");
    }

if (Input.GetKey("3"))
    {
    animation.Play("vibrate");
    } 

}