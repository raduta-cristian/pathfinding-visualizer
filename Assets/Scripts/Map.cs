using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Map", menuName = "ScriptableObjects/Map", order = 1)]
public class Map : ScriptableObject
{

    public List<CellScript.CellState> _gridState = new List<CellScript.CellState>();
    public int startX = -1;
    public int startY = -1;
    public int endX = -1;
    public int endY = -1;


}