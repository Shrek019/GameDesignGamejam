using UnityEngine;

public abstract class BuffEffect : ScriptableObject
{
    public string buffName;
    public string description;

    public abstract void Apply(GameObject player);
}