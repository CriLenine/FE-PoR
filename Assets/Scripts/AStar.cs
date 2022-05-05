using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * Pathfinding structure, return a list of coordinates corresponding to the path to use, given the starting and ending coordinates
 * and an array of 0s for obstacles and 1s for free spaces.
 */
namespace AStar
{
    // A structure to hold the necessary parameters
    [System.Serializable]
    public class Cell
    {
        // Row and Column index of its parent
        public Vector2Int parent;
        // f = g + h
        public double f, g, h;
        public Cell()
        {
            parent = Vector2Int.zero;
            f = 0;
            g = 0;
            h = 0;
        }
        public Cell(Vector2Int _parent, double _f, double _g, double _h)
        {
            parent = _parent;
            f = _f;
            g = _g;
            h = _h;
        }
    }

    [System.Serializable]
    public class Main
    {
        #region UTILITY FUNCTIONS
        // Check whether given coord "point" is valid or not
        // in (row, col)-sized array.
        public bool isValid(int row, int col, Vector2Int point)
        { // Returns true if row number and column number is in range
            if (row > 0 && col > 0)
                return (point.x >= 0) && (point.x < row)
                       && (point.y >= 0)
                       && (point.y < col);
            return false;
        }

        // Check whether the given Cell is blocked or not
        public bool isFree(int[,] grid, int col, int row, Vector2Int point)
        {
            // Returns true if the Cell is free else false
            return isValid(col, row, point) && grid[point.x, point.y] == 1;
        }

        // Calculate the 'h' heuristics.
        public int CalculateHValue(Vector2Int src, Vector2Int dest)
        {
            // h is estimated for non-diagonal moves
            return (Mathf.Abs(src.x - dest.x) + Mathf.Abs(src.y - dest.y));
        }
        #endregion

        // A method to display the path from the source to destination in the console
        // return the Vector2Int List of the path
        public List<Vector2Int> tracePath(Cell[,] cellDetails, Vector2Int dest)
        {
            //Debug.Log("\nThe Path is ");

            List<Vector2Int> Path = new List<Vector2Int>();
            List<Vector2Int> chemin = new List<Vector2Int>();

            int row = dest.x;
            int col = dest.y;
            Vector2Int next_node = new Vector2Int(row, col)/*cellDetails[row, col].parent*/;
            do
            {
                Path.Add(next_node);
                next_node = cellDetails[row, col].parent;
                row = next_node.x;
                col = next_node.y;
            } while (cellDetails[row, col].parent != next_node);

            //Path.Add(new Vector2Int(row, col)); (source)
            while (Path.Count != 0)
            {
                Vector2Int p = Path.Last();
                Path.RemoveAt(Path.Count - 1);
                chemin.Add(p);
                //Debug.Log("-> (" + p.x + ", " + p.y + ") ");
            }
            return chemin;
        }

        // Main function, to find the shortest path between a given
        // source position to a destination position according to A* Search Algorithm
        public List<Vector2Int> aStarSearch(int[,] grid, Vector2Int src, Vector2Int dest)
        {
            //Debug.Log("Beginning to search...");
            int row = grid.GetLength(0);
            int col = grid.GetLength(1);

            // If the source is out of range
            if (!isValid(row, col, src))
            {
                Debug.Log("Source is invalid\n");
                return new List<Vector2Int>();
            }

            // If the destination is out of range
            if (!isValid(row, col, dest))
            {
                Debug.Log("Destination is invalid\n");
                return new List<Vector2Int>();
            }

            // Either the source or the destination is blocked
            if (!isFree(grid, row, col, src) || !isFree(grid, row, col, dest))
            {
                Debug.Log("Source or the destination is blocked\n");
                return new List<Vector2Int>();
            }

            // If the destination Cell is the same as source Cell
            if (src == dest)
            {
                Debug.Log("We are already at the destination\n");
                return new List<Vector2Int>();
            }

            // Create a closed list and initialise it to false which
            // means that no Cell has been included yet 
            // This closed list is implemented as a boolean 2D array
            bool[,] closedList = new bool[row, col];

            // Declare a 2D array of structure to hold the details
            // of that Cell
            Cell[,] cellDetails = new Cell[row, col];
            Vector2Int b = new Vector2Int(0, 0);
            for (int indi = 0; indi < row; indi++)
            {
                for (int indj = 0; indj < col; indj++)
                {
                    cellDetails[indi, indj] = new Cell(new Vector2Int(-1, -1), -1, -1, -1);
                }
            }

            int i, j;
            // Initialising the parameters of the starting node
            i = src.x;
            j = src.y;
            cellDetails[i, j].f = 0.0;
            cellDetails[i, j].g = 0.0;
            cellDetails[i, j].h = 0.0;
            cellDetails[i, j].parent = new Vector2Int(i, j);

            /*
            Create an open list having information as-
            <f, <i, j>>
            where f = g + h,
            and i, j are the row and column index of that Cell
            Note that 0 <= i <= ROW-1 & 0 <= j <= COL-1
            This open list is implenented as a set of tuple.*/
            List<(double, Vector2Int)> openList = new List<(double, Vector2Int)>();

            // Put the starting Cell on the open list and set its
            // 'f' as 0
            openList.Add((0.0, new Vector2Int(i, j)));

            // We set this boolean value as false as initially
            // the destination is not reached.
            bool foundDest = false;
            while (openList.Count != 0)
            {
                openList.Sort((a, b) => b.Item1.CompareTo(a.Item1));
                (double, Vector2Int) p = openList.Last();
                // Add this vertex to the closed list
                i = p.Item2.x; // y element of tupla
                j = p.Item2.y; // third element of tupla

                // Remove this vertex from the open list
                openList.RemoveAt(openList.Count - 1);
                closedList[i, j] = true;
                /*
                        Generating all the 4 successors of this Cell

                        Cell-->Popped Cell (i, j)
                        N --> North     (i-1, j)
                        S --> South     (i+1, j)
                        E --> East     (i, j+1)
                        W --> West         (i, j-1)
                */
                for (int add_x = -1; add_x <= 1; add_x++)
                {
                    for (int add_y = -1; add_y <= 1; add_y++)
                    {
                        if (add_x * add_y == 0 && add_x != add_y)
                        {
                            Vector2Int neighbour = new Vector2Int(i + add_x, j + add_y);
                            // Only process this Cell if this is a valid one
                            if (isValid(row, col, neighbour))
                            {
                                // If the destination Cell is the same
                                // as the current successor
                                if (neighbour == dest)
                                { // Set the Parent of
                                  // the destination Cell
                                    cellDetails[neighbour.x, neighbour.y].parent = new Vector2Int(i, j);
                                    //Debug.Log("The destination Cell is found\n");
                                    foundDest = true;
                                    return tracePath(cellDetails, dest);
                                }
                                // If the successor is already on the
                                // closed list or if it is blocked, then
                                // ignore it.  Else do the following
                                else if (!closedList[neighbour.x, neighbour.y] && isFree(grid, col, row, neighbour))
                                {
                                    double gNew, hNew, fNew;
                                    gNew = cellDetails[i, j].g + 1.0;
                                    hNew = CalculateHValue(neighbour, dest/*, undesired*/);
                                    fNew = gNew + hNew;

                                    // If it isn’t on the open list, add
                                    // it to the open list. Make the
                                    // current square the parent of this
                                    // square. Record the f, g, and h
                                    // costs of the square Cell
                                    //             OR
                                    // If it is on the open list
                                    // already, check to see if this
                                    // path to that square is better,
                                    // using 'f' cost as the measure.
                                    if (cellDetails[neighbour.x, neighbour.y].f == -1
                                        || cellDetails[neighbour.x, neighbour.y].f > fNew)
                                    {
                                        openList.Add((fNew, new Vector2Int(neighbour.x, neighbour.y)));

                                        // Update the details of this Cell
                                        cellDetails[neighbour.x, neighbour.y].g = gNew;
                                        cellDetails[neighbour.x, neighbour.y].h = hNew;
                                        cellDetails[neighbour.x, neighbour.y].f = fNew;
                                        cellDetails[neighbour.x, neighbour.y].parent = new Vector2Int(i, j);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // When the destination cell is not found and the open
            // list is empty, then we conclude that we failed to
            // reach the destination cell. This may happen when the
            // there is no way to destination cell (due to
            // blockages)
            if (foundDest == false)
                Debug.Log("Failed to find the Destination Cell\n");
            return new List<Vector2Int>();
        }
    }
    [System.Serializable]
    public class Grid
    {
        int row, col;
        public int[,] array;
        public Grid(int _row, int _col)
        {
            row = _row;
            col = _col;
            array = new int[row, col];
        }
        public void Create(int[,] grid)
        {
            array = grid;
        }
        public void AddObstacle(int i, int j)
        {
            array[i, j] = 0;
        }
        public void AddObstacle(Vector2Int vect)
        {
            AddObstacle(vect.x, vect.y);
        }
        public void RemoveObstacle(int i, int j)
        {
            array[i, j] = 1;
        }
        public void RemoveObstacle(Vector2Int vect)
        {
            RemoveObstacle(vect.x, vect.y);
        }
        public void FreeAllSpace()
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    array[i, j] = 1;
                }
            }
        }
    }
}