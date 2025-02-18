using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] bool corners;
    public bool started = false;

    GridManager gm;
    CellScript[,] grid;
    [SerializeField] CellScript startCell = null, endCell = null;
    private float waitMul = 1f;

    public void StartPathfinder(string algo_name)
    {
        if (!started && startCell != null && endCell != null)
        {
            started = true;
            gm.Cleanup();
            gm.PropertyReset();
            StartCoroutine(algo_name);
        }
    }

    public void SliderChange(Slider sl)
    {
        waitMul = sl.value;
    }

    #region Setup
    private void Start()
    {
        gm = Object.FindObjectOfType<GridManager>();

        grid = gm.grid;
        startCell = gm.startCell;
        endCell = gm.endCell;

        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        grid = gm.grid;
        startCell = gm.startCell;
        endCell = gm.endCell;
    }

    #endregion

    #region Common Functions
    public double CalcDist(CellScript a, CellScript b)
    {
        return System.Math.Sqrt((a.gridpos_x - b.gridpos_x) * (a.gridpos_x - b.gridpos_x) + (a.gridpos_y - b.gridpos_y) * (a.gridpos_y - b.gridpos_y));
    }
    void AddCell(int x, int y, CellScript checkCell, List<CellScript> possibleCells, List<CellScript> activeCells)
    {
        if (x >= 0 && x < gm.cell_vert && y >= 0 && y < gm.cell_horiz && grid[x, y].State != CellScript.CellState.wall)
        {
            var cell = grid[x, y];
            possibleCells.Add(cell);
        }
    }

    List<CellScript> GetWalkable(CellScript checkCell, List<CellScript> activeCells)
    {
        var possibleCells = new List<CellScript>();

        int x, y;

        x = checkCell.gridpos_x + 1;
        y = checkCell.gridpos_y;
        AddCell(x, y, checkCell, possibleCells, activeCells);

        x = checkCell.gridpos_x - 1;
        y = checkCell.gridpos_y;
        AddCell(x, y, checkCell, possibleCells, activeCells);


        x = checkCell.gridpos_x;
        y = checkCell.gridpos_y + 1;
        AddCell(x, y, checkCell, possibleCells, activeCells);


        x = checkCell.gridpos_x;
        y = checkCell.gridpos_y - 1;
        AddCell(x, y, checkCell, possibleCells, activeCells);

        //corner
        if (corners)
        {
            x = checkCell.gridpos_x + 1;
            y = checkCell.gridpos_y + 1;
            AddCell(x, y, checkCell, possibleCells, activeCells);

            x = checkCell.gridpos_x - 1;
            y = checkCell.gridpos_y + 1;
            AddCell(x, y, checkCell, possibleCells, activeCells);


            x = checkCell.gridpos_x - 1;
            y = checkCell.gridpos_y + 1;
            AddCell(x, y, checkCell, possibleCells, activeCells);


            x = checkCell.gridpos_x - 1;
            y = checkCell.gridpos_y - 1;
            AddCell(x, y, checkCell, possibleCells, activeCells);

        }

        return possibleCells;
    }

    private void GiveResults(int cellsSearched, int pathLenght)
    {
        string t1 = cellsSearched + " cells searched\n\n";
        string t2 = "path lenght: " + pathLenght;
        if (pathLenght <= 0) t2 = "path lenght: infinite" ;

        GameObject.Find("ResultText").GetComponent<Text>().text = t1 + t2;
    }

    private List<CellScript> Shuffle(List<CellScript> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }

        return ts;
    }
    #endregion


    #region Algorithms

    IEnumerator A_Star()
    {
        bool ended = false;
        int cellCounter = 0;
        int pathCounter = 0;

        startCell.SetDistance(endCell.gridpos_x, endCell.gridpos_y);
        startCell.cost = 0;

        var activeCells = new List<CellScript>();
        activeCells.Add(startCell);

        var visitedCells = new List<CellScript>();

        while (activeCells.Count > 0)
        {
            cellCounter++;

            var checkCell = activeCells.First();
            foreach (var c in activeCells)
                if (c.CostDistance <= checkCell.CostDistance)
                    checkCell = c;
            

            visitedCells.Add(checkCell);
            activeCells.Remove(checkCell);

            if (checkCell != startCell & checkCell != endCell)
                checkCell.State = CellScript.CellState.searchpath;

            if (checkCell == endCell)
            {
                activeCells.Clear();
                ended = true;
                CellScript Cell = checkCell;

                while (Cell != startCell && Cell != null)
                {
                    pathCounter++;
                    if (Cell != endCell) Cell.State = CellScript.CellState.mainpath;
                    Cell = Cell.parent;
                        if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.001f * waitMul);
                }


            }

            var walkableCells = GetWalkable(checkCell, activeCells);
            walkableCells.ForEach(tile => tile.SetDistance(endCell.gridpos_x, endCell.gridpos_y));

            if (!ended)
            {
                foreach (var walkable in walkableCells)
                {
                    if (visitedCells.Contains(walkable))
                        continue;

                    double dis = CalcDist(walkable, checkCell);

                    if (activeCells.Contains(walkable))
                    {
                        if (checkCell.cost + dis < walkable.cost)
                        {
                            walkable.parent = checkCell;
                            walkable.cost = checkCell.cost + dis;
                        }
                    }
                    else
                    {

                        walkable.cost = checkCell.cost + dis;
                        walkable.parent = checkCell;
                        activeCells.Add(walkable);
                    }
                }
                if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.001f * waitMul);

            }

        }
        GiveResults(cellCounter, pathCounter);
        started = false;
    }

    IEnumerator Simple_Breadth()
    {
        bool ended = false;

        int pathCounter = 0;

        var activeCells = new List<CellScript>();

        activeCells.Add(startCell);
        startCell.cost = 0;
        startCell.parent = null;

        int i = 0;
        while (i < activeCells.Count)
        {
            var checkCell = activeCells[i];

            if (checkCell == endCell)
            {
                activeCells.Clear();
                ended = true;
                CellScript Cell = checkCell;

                while (Cell != startCell && Cell != null)
                {
                    pathCounter++;
                    if (Cell != endCell) Cell.State = CellScript.CellState.mainpath;
                    Cell = Cell.parent;
                        if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.005f * waitMul);
                }

            }

            if (checkCell != startCell & checkCell != endCell)
                checkCell.State = CellScript.CellState.searchpath;

            if (!ended)
            {

                var walkableCells = GetWalkable(checkCell, activeCells);
                foreach (var walkable in walkableCells)
                {
                    double dis = CalcDist(walkable, checkCell);
                    

                    if (activeCells.Contains(walkable))
                    {
                        if (walkable.cost > checkCell.cost + dis)
                        {
                            walkable.cost = checkCell.cost + dis;
                            walkable.parent = checkCell;
                        }
                    }
                    else
                    {
                        walkable.cost = checkCell.cost + dis;
                        walkable.parent = checkCell;
                        activeCells.Add(walkable);
                    }
                }
                    if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.001f * waitMul);
                i++;
            }

        }

        GiveResults(i+1, pathCounter);
        started = false;
    }

    IEnumerator Simple_Depth()
    {
        bool ended = false;
        int cellCounter = 0;
        int pathCounter = 0;

        var activeCells = new List<CellScript>();
        var visitedCells = new List<CellScript>();

        activeCells.Add(startCell);
        startCell.cost = 0;
        startCell.parent = null;

        while (activeCells.Count > 0)
        {
            var checkCell = activeCells[activeCells.Count-1];
            visitedCells.Add(checkCell);
            activeCells.Remove(checkCell);

            if (checkCell == endCell)
            {
                activeCells.Clear();
                ended = true;
                CellScript Cell = checkCell;

                while (Cell != startCell && Cell != null)
                {
                    pathCounter++;
                    if (Cell != endCell) Cell.State = CellScript.CellState.mainpath;
                    Cell = Cell.parent;

                    if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f)
                        yield return new WaitForSecondsRealtime(0.005f * waitMul);
                }

            }

            if (checkCell != startCell & checkCell != endCell)
                checkCell.State = CellScript.CellState.searchpath;

            if (!ended)
            {
                var walkableCells = GetWalkable(checkCell, activeCells);
                walkableCells = Shuffle(walkableCells);
                foreach (var walkable in walkableCells)
                {
                    double dis = CalcDist(walkable, checkCell);
                
                    if (visitedCells.Contains(walkable) || activeCells.Contains(walkable))
                    {
                        if (walkable.cost > checkCell.cost + dis)
                        {
                            walkable.cost = checkCell.cost + dis;
                            walkable.parent = checkCell;
                        }
                    }
                    else
                    {
                        walkable.cost = checkCell.cost + dis;
                        walkable.parent = checkCell;
                        activeCells.Add(walkable);
                    }

                }

                if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.001f * waitMul);

                cellCounter++;
            }

        }

        GiveResults(cellCounter, pathCounter);
        started = false;
    }

    IEnumerator Random_Walk()
    {
        bool ended = false;
        int cellCounter = 0;
        int pathCounter = 0;

        var activeCells = new List<CellScript>();
        var visitedCells = new List<CellScript>();

        activeCells.Add(startCell);
        startCell.cost = 0;
        startCell.parent = null;

        while (activeCells.Count > 0)
        {
            var checkCell = activeCells[Random.Range(0, activeCells.Count)];
            visitedCells.Add(checkCell);
            activeCells.Remove(checkCell);

            if (checkCell == endCell)
            {
                activeCells.Clear();
                ended = true;
                CellScript Cell = checkCell;

                while (Cell != startCell && Cell != null)
                {
                    pathCounter++;
                    if (Cell != endCell) Cell.State = CellScript.CellState.mainpath;
                    Cell = Cell.parent;
                    if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.005f * waitMul);
                }

            }

            if (checkCell != startCell & checkCell != endCell)
                checkCell.State = CellScript.CellState.searchpath;

            if (!ended)
            {

                var walkableCells = GetWalkable(checkCell, activeCells);
                foreach (var walkable in walkableCells)
                {
                    double dis = CalcDist(walkable, checkCell);

                    if (visitedCells.Contains(walkable) || activeCells.Contains(walkable))
                    {
                        /*if (walkable.cost > checkCell.cost + dis)
                        {
                            walkable.cost = checkCell.cost + dis;
                            walkable.parent = checkCell;
                        }*/
                    }
                    else
                    {
                        walkable.cost = checkCell.cost + dis;
                        walkable.parent = checkCell;
                        activeCells.Add(walkable);
                    }

                }

                //if (Random.Range(0, waitMul) != 0)
                if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.001f * waitMul);

                cellCounter++;
            }

        }

        GiveResults(cellCounter, pathCounter);
        started = false;
    }

    IEnumerator A_Star_NoCost()
    {
        bool ended = false;
        int cellCounter = 0;
        int pathCounter = 0;

        startCell.SetDistance(endCell.gridpos_x, endCell.gridpos_y);
        startCell.cost = 0;

        var activeCells = new List<CellScript>();
        activeCells.Add(startCell);

        var visitedCells = new List<CellScript>();

        while (activeCells.Count > 0)
        {
            cellCounter++;

            var checkCell = activeCells.OrderBy(x => x.distance).First();
            visitedCells.Add(checkCell);
            activeCells.Remove(checkCell);

            if (checkCell != startCell & checkCell != endCell)
                checkCell.State = CellScript.CellState.searchpath;

            if (checkCell == endCell)
            {
                activeCells.Clear();
                ended = true;
                CellScript Cell = checkCell;

                while (Cell != startCell && Cell != null)
                {
                    pathCounter++;
                    if (Cell != endCell) Cell.State = CellScript.CellState.mainpath;
                    Cell = Cell.parent;
                    //if (Random.Range(0, waitMul) != 0)
                    if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.005f * waitMul);
                }


            }

            var walkableCells = GetWalkable(checkCell, activeCells);
            walkableCells.ForEach(tile => tile.SetDistance(endCell.gridpos_x, endCell.gridpos_y));

            if (!ended)
            {
                foreach (var walkable in walkableCells)
                {
                    if (visitedCells.Contains(walkable))
                        continue;

                    double dis = CalcDist(walkable, checkCell);

                    if (activeCells.Contains(walkable))
                    {
                        if (checkCell.cost + dis < walkable.cost)
                        {
                            walkable.parent = checkCell;
                            walkable.cost = checkCell.cost + dis;
                        }
                    }
                    else
                    {

                        walkable.cost = checkCell.cost + dis;
                        walkable.parent = checkCell;
                        activeCells.Add(walkable);
                    }
                }
                //if (Random.Range(0, waitMul) != 0)
                if(waitMul != 0 && Random.Range(0, waitMul+0.5f) > 0.5f) yield return new WaitForSecondsRealtime(0.001f * waitMul);

            }

        }
        GiveResults(cellCounter, pathCounter);
        started = false;
    }

    #endregion
}
