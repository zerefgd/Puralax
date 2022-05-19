using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllLevels", menuName = "Level Creation/AllLevels")]
public class LevelDictionary : ScriptableObject
{
    [SerializeField]
    private List<Level> allLevels;
    private Dictionary<string, Level> levels = new();

    private void OnEnable()
    {
        foreach (var item in allLevels)
        {
            levels[item.levelName] = item;
        }
    }

    public Level GetLevel(string name)
    {
        levels.TryGetValue(name, out Level result);
        return result;
    }
}
