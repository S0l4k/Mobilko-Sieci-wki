using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;
    public const byte MOUSEBUTTON1 = 2;
    public const byte INTERACT = 4;

    public NetworkButtons buttons;
    public Vector3 direction;        // PC Input (WASD)
    public Vector2 mobileDirection;  // Android joystick
    public bool interact;            // Android button
}
