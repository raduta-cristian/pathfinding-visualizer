using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("Grid Generation")]
    public int cell_horiz;
    public int cell_vert;
    public Vector2 offset;
    public Vector2 size;
    public Vector2 cell_margin;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] GameObject gridPanel;
   
    public CellScript[,] grid = null;

    public CellScript startCell = null, endCell = null;

    [HideInInspector] public CellScript.CellState activePaintState = CellScript.CellState.nothing;

    private bool generated = false;
    private Pathfinding pathF;
    public void SetPaintState(string stateName)
    {

        activePaintState = (CellScript.CellState) System.Enum.Parse(typeof(CellScript.CellState), stateName);
    }

    #region Grid Maintanance
    public void LoadMap(Map map)
    {
        if (pathF.started) return;


        for (int i = 0; i < cell_vert; i++)
        {
            for (int j = 0; j < cell_horiz; j++)
            {
                grid[i, j].State = map._gridState[i*cell_horiz+j];
            }
        }

        if (map.startX != -1) this.startCell = grid[map.startX, map.startY];
        else this.startCell = null;

        if (map.endX != -1) this.endCell = grid[map.endX, map.endY];
        else this.endCell = null;
    }

    
    void GenerateGrid()
    { 
        Vector2 cellsize = new Vector2(size.x / cell_horiz, size.y / cell_vert);

        grid = new CellScript[cell_vert, cell_horiz];

        for(int i = 0; i < cell_vert; i++)
        {
            for(int j = 0; j < cell_horiz; j++)
            {
                GameObject btn = GameObject.Instantiate(buttonPrefab, gridPanel.transform);

                CellScript c = btn.GetComponent<CellScript>();
                c.gridpos_x = i;
                c.gridpos_y = j;
                c.State = CellScript.CellState.empty;

                RectTransform rect = btn.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = cellsize - cell_margin;
                rect.localPosition = new Vector2((j+0.5f) * cellsize.x, (i+0.5f) * cellsize.y) - size/2 + offset;

                grid[i,j] = c;
            }
        }

        generated = true;


    }
    public void Cleanup()
    {
        if (generated)
        {
            for (int i = 0; i < cell_vert; i++)
            {
                for (int j = 0; j < cell_horiz; j++)
                {
                    if(grid[i, j].State == CellScript.CellState.mainpath || grid[i, j].State == CellScript.CellState.searchpath)
                        grid[i, j].State = CellScript.CellState.empty;
                }
            }
            GameObject.Find("ResultText").GetComponent<Text>().text = "";
        }

    }
    public void PropertyReset()
    {
        if (!pathF.started)
        {
            for (int i = 0; i < cell_vert; i++)
            {
                for (int j = 0; j < cell_horiz; j++)
                {
                    grid[i, j].parent = null;
                    grid[i, j].cost = 0;
                    grid[i, j].distance = 0;
                }
            }
        }
    }
    public void ClearGrid()
    {
        if (!pathF.started)
        {
            PropertyReset();

            for (int i = 0; i < cell_vert; i++)
            {
                for (int j = 0; j < cell_horiz; j++)
                {

                    grid[i, j].State = CellScript.CellState.empty;
                }
            }

            startCell = null;
            endCell = null;
            GameObject.Find("ResultText").GetComponent<Text>().text = "";
        }

        
    }

    #endregion

    private void Awake()
    {
        pathF = Object.FindObjectOfType<Pathfinding>();
        generated = false;
        GenerateGrid();
    }
}
