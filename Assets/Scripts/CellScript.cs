using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellScript : MonoBehaviour
{
    private GridManager gridM;
    private Pathfinding pathF;
    private Image img;
    private AudioSource audioSrc;

    #region State
    [System.Serializable]
    public enum CellState
    {
        nothing,
        empty,
        wall,
        start,
        end,
        mainpath,
        searchpath
    };

    private CellState _state;

    public CellState State
    {
        get
        {
            return _state;
        }

        set
        {
            if (value != CellState.nothing)
            {

                switch (value)
                {
                    case CellState.start:
                        if (gridM.startCell) {
                            gridM.startCell.State = CellState.empty;
                            gridM.startCell = null;
                        }
                        gridM.startCell = this;

                        break;
                    case CellState.end:
                        if (gridM.endCell)
                        {
                            gridM.endCell.State = CellState.empty;
                            gridM.endCell = null;
                        }
                        gridM.endCell = this;

                        break;
                }

                _state = value;

                UpdateColor();

                if (_state != CellState.mainpath && _state != CellState.searchpath)
                    gridM.Cleanup();
                else
                {
                    float t = (float)(pathF.CalcDist(this, gridM.startCell) / (pathF.CalcDist(this, gridM.endCell) + pathF.CalcDist(this, gridM.startCell)));

                    if(audioSrc != null)
                    {
                        audioSrc.pitch = Mathf.Lerp(0.2f, 2f, t);
                        audioSrc.Play();
                    }

                }

                /*if (_state == CellState.start || _state == CellState.end)
                    gridM.MakeCellExclusive(gridpos_x, gridpos_y);*/
            }
        }
        
    }
    #endregion

    public int gridpos_x;
    public int gridpos_y;

    public double cost = 0;
    public double distance = 0;
    public CellScript parent = null;
    private bool isHovering = false;

    public double CostDistance => cost + distance;
    public void SetDistance(int targetX, int targetY)
    {
        //this.distance = System.Math.Abs(targetX - gridpos_x) + System.Math.Abs(targetY - gridpos_y);
        this.distance = System.Math.Sqrt((targetX - gridpos_x) * (targetX - gridpos_x) + (targetY - gridpos_y) * (targetY - gridpos_y));
    }


    #region Button Functionality

    public void EnterButtonEvent(bool b)
    {
        isHovering = b;
    }

    private void UpdateColor()
    {
        switch (State)
        {
            case CellState.empty:
                img.color = Color.white;
                break;
            case CellState.wall:
                img.color = Color.black;
                break;
            case CellState.start:
                img.color = Color.red;
                break;
            case CellState.end:
                img.color = Color.green;
                break;
            case CellState.mainpath:
                img.color = Color.blue;
                break;
            case CellState.searchpath:
                img.color = Color.cyan;
                break;
            default:
                img.color = Color.white;
                break;

        }
    }

    #endregion

    private void Awake()
    {
        gridM = Object.FindObjectOfType<GridManager>();
        pathF = Object.FindObjectOfType<Pathfinding>();
        img = GetComponent<Image>();
        audioSrc = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && isHovering && !pathF.started) State = gridM.activePaintState;
    }
}
