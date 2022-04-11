using UnityEngine;

public class TransformUpdate
{
    public ushort Tick { get; private set; }
    public bool IsTeleport { get; private set; }
    public Vector3 Position { get; private set; }

    public TransformUpdate(ushort tick, bool isTeleport, Vector3 position)
    {
        Tick = tick;
        IsTeleport = isTeleport;
        Position = position;
    }
}
