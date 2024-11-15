using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "Scriptable Item")]
public class ScriptableItem : ScriptableObject
{
    public Item item;
    public BaseObject objectPrefab;
    
}

public enum Item
{
    Platform = 0,
    Trap = 1
}
