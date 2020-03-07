using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid : MonoBehaviour
{
    public int CellCountX;
    public int CellCountY;
    public Vector2[,] gridPosition;
    public GameObject[,] gridItems;
    public readonly float gridCellDistanceX = 3.9f;
    public readonly float gridCellDistanceY = 4.6f;

    // Start is called before the first frame update
    void Awake()
    {
        gridPosition = new Vector2[CellCountX,CellCountY];
        gridItems = new GameObject[CellCountX, CellCountY];
    }

    public void AddHexagonal(GameObject hexagonal)
    {

    }

    public void RemoveHexagonal(GameObject hexagonal)
    {
        int x = (int)hexagonal.GetComponent<BlockProperties>().gridCoordinate.x;
        int y = (int)hexagonal.GetComponent<BlockProperties>().gridCoordinate.y;

        gridItems[x, y] = null;
        Destroy(hexagonal);
    }

    public Vector2 GetDropCoordinate(GameObject hexagonalBlock)
    {
        Vector2 DropCoordinate = new Vector2(-1,-1) ;
        if (hexagonalBlock != null)
        {
            int x = (int)hexagonalBlock.GetComponent<BlockProperties>().gridCoordinate.x;
            int y = (int)hexagonalBlock.GetComponent<BlockProperties>().gridCoordinate.y;

            for (int i = y; i < CellCountY; i++)
            {
                if (gridItems[x, i] == null) DropCoordinate = new Vector2(x, i);
            }
        }
        return DropCoordinate;
    }

    public List<Vector2> EmptyGridCoordinates()
    {
        List<Vector2> list = new List<Vector2>();

        for (int j = 0; j < CellCountY - 1; j++)
        {
            for (int i = 0; i < CellCountX; i++)
            {
                if (gridItems[i, j] == null) list.Add(new Vector2(i, j));
            }

        }
        return list;
    }

    public List<GameObject> GridItemsToList()
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < CellCountX; i++)
        {
            for (int j = 0; j < CellCountY; j++)
            {
                list.Add(gridItems[i,j]);
            }
        }
        return list;
    }
}
