using UnityEngine;

public interface IResourceManager
{
    Sprite Icon { get; }
    void Add(int amount);
    void Spend(int amount);
    bool HasEnough(int amount);
}
