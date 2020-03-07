using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InitializeLevel : MonoBehaviour
{
    public GameObject hexagonalBlock { get; private set; }
    public List<Color> ColorList;

    public Vector3 firstHexagonalPosition { get; private set; }
    private readonly float referenceCameraSize = 40f;
    private readonly float referenceCameraAspectRatio = 1080f/1920f;
    
    private Manager Manager;
    private MyGrid grid;
    private System.Random random;

    // Start is called before the first frame update
    private void Awake()
    {
        random = new System.Random();
        hexagonalBlock = Resources.Load<GameObject>("Prefabs/HexagonalBlock");
        grid = GameObject.Find("GameManager").GetComponent<MyGrid>();
    }

    void Start()
    {
        Manager = GameObject.Find("GameManager").GetComponent<Manager>();

        ResizeCameraAndStartPositionDependsOnBlockCount();
        ResizeCameraDependsOnResolution();

        CreateFirstBlocksAndFillGridProperties();

        StartCoroutine(ChangeColorOnInitialize(grid.GridItemsToList()));
    }

   void CreateFirstBlocksAndFillGridProperties()
    {
        float yPosOfHexagonal;
        float xPosOfHexagonal;
        for (int i = 0; i < grid.CellCountY; i++)
        {
            for (int j = 0; j < grid.CellCountX; j++)
            {
                if (j % 2 == 0) yPosOfHexagonal = 0;
                else yPosOfHexagonal = -grid.gridCellDistanceY / 2;
                yPosOfHexagonal += -grid.gridCellDistanceY * i;

                xPosOfHexagonal = j * grid.gridCellDistanceX;

                Vector3 clonePosition = new Vector3(xPosOfHexagonal, yPosOfHexagonal, 0);
                clonePosition += firstHexagonalPosition;

                GameObject hexagonelBlockClone = Instantiate(hexagonalBlock, clonePosition, Quaternion.identity);

                hexagonelBlockClone.GetComponent<SpriteRenderer>().color = ColorList[random.Next(0, ColorList.Count)];
                hexagonelBlockClone.GetComponent<BlockProperties>().gridCoordinate = new Vector2(j,i);

                grid.gridItems[j,i] = hexagonelBlockClone;
                grid.gridPosition[j, i] = clonePosition;
            }
        }
    }

    void ResizeCameraDependsOnResolution()
    {
        Camera cam = Camera.main;
        cam.orthographicSize = referenceCameraSize * referenceCameraAspectRatio / cam.aspect;
    }

    void ResizeCameraAndStartPositionDependsOnBlockCount()
    {
        Camera cam = Camera.main;
        float blocksOffsetX = 1 * grid.gridCellDistanceX; ;
        float hexagonelTotalBlockWidth = grid.CellCountX * grid.gridCellDistanceX;
        float cameraSizeDependsOnWidth = hexagonelTotalBlockWidth + blocksOffsetX;

        float blocksOffsetYRatioFromTop = 0.3f;
        float blocksOffsetYRatioFromBottom = 0.15f;
        float blocksOffsetYRatioTotal = (1+blocksOffsetYRatioFromTop+blocksOffsetYRatioFromBottom);
        float hexagonelTotalBlockHeight = grid.CellCountY * grid.gridCellDistanceY;
        float cameraSizeDependsOnHeight = hexagonelTotalBlockHeight * blocksOffsetYRatioTotal;

        float startPosY;
        if (cameraSizeDependsOnWidth > cameraSizeDependsOnHeight / 2)
        {
            cam.orthographicSize = cameraSizeDependsOnWidth;
            startPosY = cameraSizeDependsOnHeight - hexagonelTotalBlockHeight;
        }
        else
        {
            cam.orthographicSize = cameraSizeDependsOnHeight / 2;
            startPosY = cam.orthographicSize * 2 - hexagonelTotalBlockHeight;
        }
        float startPosX = -(hexagonelTotalBlockWidth - blocksOffsetX) / 2;

        startPosY += startPosY * (blocksOffsetYRatioFromBottom - blocksOffsetYRatioFromTop);
        firstHexagonalPosition = new Vector2(startPosX, startPosY);
    }

    IEnumerator ChangeColorOnInitialize(List<GameObject> blockList)
    {
        yield return new WaitForSeconds(0.01f);
        List<Color> tempColorList = new List<Color>();
        tempColorList.AddRange(ColorList);
        System.Random random = new System.Random();
        bool isColorChanged = true;
        while (isColorChanged == true)
        {
            isColorChanged = false;
            for (int i = 0; i < blockList.Count; i++)
            {
                int sameColorBlockCount = Manager.GetSameColorTouchingBlocks(blockList[i]).Count;

                while (sameColorBlockCount >= 2)
                {
                    sameColorBlockCount = Manager.GetSameColorTouchingBlocks(blockList[i]).Count;
                    tempColorList.Remove(blockList[i].GetComponent<SpriteRenderer>().color);
                    if (tempColorList.Count == 0) tempColorList.AddRange(ColorList);
                    blockList[i].GetComponent<SpriteRenderer>().color = tempColorList[random.Next(0, tempColorList.Count)];
                    isColorChanged = true;
                }
            }
        }
    }

   
}
