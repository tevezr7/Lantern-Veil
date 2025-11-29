using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
public class ItemSO : ScriptableObject
{
    public string id;                  // unique key, e.g., "potion"
    public string displayName;         // "Potion"
    public Sprite icon;                // inventory icon
    public int maxStack = 99;
}
