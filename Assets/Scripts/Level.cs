using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level",menuName ="Level Creation/Level")]
public class Level : ScriptableObject
{
    public string levelName;
    public int row, col;
    public int winColor;
    public List<CellData> cellData;
}
