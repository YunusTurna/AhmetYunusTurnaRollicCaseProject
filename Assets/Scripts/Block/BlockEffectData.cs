using System;

[Serializable]
public class BlockEffectData
{
    public BlockEffectType type;
    public float bombTimer = 10f;
    public int iceBlocksToDestroy = 3;
}
