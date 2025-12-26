using System;

[Serializable]
public class BlockData : GridObjectData
{
    public MoveDirection moveDirection;
    public BlockSO shapeData;
    public BlockEffectData effect = new BlockEffectData();
}
