using UnityEngine;
using Rewired;

public class InputListener : MonoBehaviour
{
	[System.Serializable]
	public enum ButtonState
	{
		Unpressed,
		Pressed,
		Hold,
		Released
	}

    [Header("CONTROLLER INPUTS")]
    [Space(10)]
    [SerializeField] protected string[] buttonUnpressedInputs;
    [SerializeField] string[] buttonInputs;
    [SerializeField] string[] buttonDownInputs;
    [SerializeField] string[] buttonUpInputs;
    [SerializeField] string[] axisInputs;

    protected bool lockAllInputs = false;

    protected virtual void GetButtonUnpressed(InputActionEventData data)
	{

	}

    protected virtual void GetButtonDown(InputActionEventData data)
	{

	}

    protected virtual void GetButton(InputActionEventData data)
	{

	}

    protected virtual void GetButtonUp(InputActionEventData data)
	{

	}

    protected virtual void GetAxis(InputActionEventData data)
	{

	}

    protected void InitAllInputs(Rewired.Player player)
    {
        // Button unpressed inputs
        foreach (string inputName in buttonUnpressedInputs)
        {
            player.AddInputEventDelegate(GetButtonUnpressed, UpdateLoopType.Update, InputActionEventType.ButtonUnpressed, inputName);
        }

        // Button inputs
        foreach (string inputName in buttonInputs)
        {
            player.AddInputEventDelegate(GetButton, UpdateLoopType.Update, InputActionEventType.ButtonPressed, inputName);
        }

        // Button down inputs
        foreach (string inputName in buttonDownInputs)
        {
            player.AddInputEventDelegate(GetButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, inputName);
        }

        // Button up inputs
        foreach (string inputName in buttonUpInputs)
        {
            player.AddInputEventDelegate(GetButtonUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, inputName);
        }

        // Axis inputs
        foreach (string inputName in axisInputs)
        {
            player.AddInputEventDelegate(GetAxis, UpdateLoopType.Update, InputActionEventType.Update, inputName);
        }
    }

    public void UnsubscribeAllInputs(Rewired.Player player)
    {
        // Button unpressed inputs
        foreach (string inputName in buttonUnpressedInputs)
        {
            player.RemoveInputEventDelegate(GetButtonUnpressed, InputActionEventType.ButtonUnpressed, inputName);
        }

        // Button inputs
        foreach (string inputName in buttonInputs)
        {
            player.RemoveInputEventDelegate(GetButton, InputActionEventType.ButtonPressed, inputName);
        }

        // Button down inputs
        foreach (string inputName in buttonDownInputs)
        {
            player.RemoveInputEventDelegate(GetButtonDown, InputActionEventType.ButtonJustPressed, inputName);
        }

        // Button up inputs
        foreach (string inputName in buttonUpInputs)
        {
            player.RemoveInputEventDelegate(GetButtonUp, InputActionEventType.ButtonJustReleased, inputName);
        }

        // Axis inputs
        foreach (string inputName in axisInputs)
        {
            player.RemoveInputEventDelegate(GetAxis, InputActionEventType.Update, inputName);
        }
    }

    public void LockAllInputs(bool lck)
	{
		lockAllInputs = lck;
	}
}
