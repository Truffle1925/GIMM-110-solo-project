using Leap;
using UnityEngine;

public class LeapCursorController : MonoBehaviour
{
    public LeapProvider leapProvider;

    void OnUpdateFrame(Frame frame)
    {
        //Get a list of all the hands in the frame and loop through
        //to find the first hand that matches the Chirality
        foreach (var hand in frame.Hands)
        {
            if (hand.IsLeft)
            {
                //We found a left hand
            }
        }

        //Use a helpful utility function to get the first hand that matches the Chirality
        Hand _leftHand = frame.GetHand(Chirality.Left);
        Hand _rightHand = frame.GetHand(Chirality.Right);
    }
}