using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class MapManagement : MonoBehaviour
{
    private GridManager gm;
    private Pathfinding pathf;

    [SerializeField] private string _file_name;

#if UNITY_EDITOR
    private void SaveMap()
    {
       
        Map map = ScriptableObject.CreateInstance<Map>();
        
        for (int i = 0; i < gm.cell_vert; i++)
        {
            for (int j = 0; j < gm.cell_horiz; j++)
            {
                map._gridState.Add(gm.grid[i, j].State);
            }
        }


        if(gm.startCell != null)
        {
            map.startX = gm.startCell.gridpos_x;
            map.startY = gm.startCell.gridpos_y;
        }
        if(gm.endCell != null)
        {
            map.endX = gm.endCell.gridpos_x;
            map.endY = gm.endCell.gridpos_y;
        }

        AssetDatabase.CreateAsset(map, "Assets/ScriptableObjects/" + _file_name + ".asset");
        AssetDatabase.SaveAssets();
    }
#endif

    public void LoadToGrid(Map map)
    {
        gm.LoadMap(map);
    }

    private void Awake()
    {
        gm = Object.FindObjectOfType<GridManager>();
        pathf = Object.FindObjectOfType<Pathfinding>();
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P) && !pathf.started)
            SaveMap();
#endif
    }
}
