﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    private InitializeLevel init;
    private MyGrid grid;

    public int score = 0;
    public int scoreConstant;

    public bool isRotating = false;
    public bool isRotatedOnLastDrag = false;
    public bool isExplodedOnRotate = false;
    public bool isDropping = false;
    public float blocksRotateTime;

    public Vector2 rotateStartPosition;
    public List<GameObject> selectedBlocks;
    public GameObject hexagonalCenterItemClone;
    private GameObject hexagonalShade;
    private GameObject hexagonalCenterItem;
    private Text scoreText;
    private Text highScoreText;

    private System.Random random;

    private float dropHeight;
    public float dropWaitTimePerBlock;
    public float dropTimeFromTop;
    public float dropTimeInGrid;


    // Start is called before the first frame update
    void Awake()
    {
        random = new System.Random();
        selectedBlocks = new List<GameObject>();
        init = gameObject.GetComponent<InitializeLevel>();
        grid = gameObject.GetComponent<MyGrid>();
        hexagonalCenterItem = Resources.Load<GameObject>("Prefabs/SelectObject");
        hexagonalShade = Resources.Load<GameObject>("Prefabs/HexagonalShade");
        scoreText = GameObject.Find("CurrentScoreText").GetComponent<Text>();
        highScoreText = GameObject.Find("HighScoreText").GetComponent<Text>();

        highScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore").ToString();
    }

    private void Update()
    {
        
    }

    public IEnumerator DestroyBlocks(List<GameObject> destroyList)
    {
        StartCoroutine(AddScore(destroyList));
        for (int k = destroyList.Count - 1; k >= 0; k--)
        {
            grid.RemoveHexagonal(destroyList[k]);
        }
        yield return StartCoroutine(DropBlockDown());
        yield return new WaitForSeconds(dropTimeInGrid/2);
        yield return StartCoroutine(CreateNewBlocks());
        yield return new WaitForSeconds(dropTimeFromTop);
        isDropping = false;
        yield return StartCoroutine(CheckExplodeAfterDropBlock());
    }

    public IEnumerator DropBlockDown()
    {
        GameObject hexagonal;
        Vector2 targetCoordinate;

        for (int j = grid.CellCountY - 1; j >= 0; j--)
        {
            for (int i = 0; i < grid.CellCountX; i++)
            {
                if (grid.gridItems[i, j] != null) hexagonal = grid.gridItems[i, j];
                else continue;

                targetCoordinate = grid.GetDropCoordinate(hexagonal);
                if (targetCoordinate == new Vector2(-1, -1)) continue;

                grid.gridItems[(int)targetCoordinate.x, (int)targetCoordinate.y] = hexagonal;

                hexagonal.GetComponent<BlockProperties>().targetPos = grid.gridPosition[(int)targetCoordinate.x,(int)targetCoordinate.y];
                hexagonal.GetComponent<BlockProperties>().gridCoordinate = targetCoordinate;
                hexagonal.GetComponent<BlockProperties>().LerpTime = dropTimeInGrid;
                isDropping = true;
                hexagonal.GetComponent<BlockProperties>().isDropping = true;

                grid.gridItems[i, j] = null;
                yield return new WaitForSeconds(dropWaitTimePerBlock);
            }
        }
        yield return null;
    }

    public IEnumerator CreateNewBlocks()
    {
        dropHeight = Camera.main.orthographicSize + grid.gridCellDistanceY;
        List<Vector2> emptyGrids = grid.EmptyGridCoordinates();
        for (int i = emptyGrids.Count - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(dropWaitTimePerBlock);

            Vector2 createPosition = grid.gridPosition[(int)emptyGrids[i].x, (int)emptyGrids[i].y];
            createPosition.y += dropHeight;

            GameObject hexagonelBlockClone = Instantiate(init.hexagonalBlock, createPosition, Quaternion.identity);

            hexagonelBlockClone.GetComponent<SpriteRenderer>().color = init.ColorList[random.Next(0, init.ColorList.Count)];
            hexagonelBlockClone.GetComponent<BlockProperties>().gridCoordinate = new Vector2(emptyGrids[i].x, emptyGrids[i].y);
            hexagonelBlockClone.GetComponent<BlockProperties>().targetPos = grid.gridPosition[(int)emptyGrids[i].x, (int)emptyGrids[i].y];
            hexagonelBlockClone.GetComponent<BlockProperties>().LerpTime = dropTimeFromTop;
            grid.gridItems[(int)emptyGrids[i].x, (int)emptyGrids[i].y] = hexagonelBlockClone;
            isDropping = true;

            hexagonelBlockClone.GetComponent<BlockProperties>().isDropping = true;
        }
        yield return null;
    }

    public IEnumerator CheckExplodeAfterDropBlock()
    {
        List<GameObject> destroyList = new List<GameObject>();
        for (int i = 0; i < grid.CellCountX; i++)
        {
            for (int j = 0; j < grid.CellCountY - 1; j++)
            {
                Vector3 checkPosition = grid.gridPosition[i, j];
                checkPosition.y -= grid.gridCellDistanceY / 2;
                checkPosition.x -= grid.gridCellDistanceX / 2;

                List<GameObject> hexagonalGroup = new List<GameObject>();
                List<GameObject> blocksWillDestroy = new List<GameObject>();

                hexagonalGroup = FindNearestThreeBlock(checkPosition);
                blocksWillDestroy = GetBlocksCanExplode(hexagonalGroup);

                destroyList.AddRange(blocksWillDestroy);
            }
        }
        if (destroyList.Count > 0)
        {
            StartCoroutine(DestroyBlocks(destroyList));
            yield return null;
        }
        else
        {
            selectedBlocks = FindNearestThreeBlock(rotateStartPosition);
            CreateSelectItem(selectedBlocks);
            isExplodedOnRotate = false;
        }
        yield return null;
    }

    public List<GameObject> GetBlocksCanExplode(List<GameObject> centerBlocks)
    {
        List<GameObject> blocksCanExplode = new List<GameObject>();
        for (int i = 0; i < centerBlocks.Count; i++)
        {
            List<GameObject> sameColor1stBlocksToCenter = new List<GameObject>();
            sameColor1stBlocksToCenter.AddRange(GetSameColorTouchingBlocks(centerBlocks[i]));
            if (sameColor1stBlocksToCenter.Count >= 2)
            {
                List<GameObject> sameColor1stBlocksToOthers = new List<GameObject>();
                for (int j = 0; j < sameColor1stBlocksToCenter.Count; j++)
                {
                    sameColor1stBlocksToOthers.AddRange(GetSameColorTouchingBlocks(sameColor1stBlocksToCenter[j]));
                    for (int k = 0; k < sameColor1stBlocksToOthers.Count; k++)
                    {
                        if (sameColor1stBlocksToCenter.Contains(sameColor1stBlocksToOthers[k]))
                        {
                            blocksCanExplode.Add(centerBlocks[i]);
                            blocksCanExplode.Add(sameColor1stBlocksToCenter[j]);
                            blocksCanExplode.Add(sameColor1stBlocksToOthers[k]);
                        }
                    }
                }
            }
        }
        blocksCanExplode = blocksCanExplode.Distinct().ToList();
        return blocksCanExplode;
    }

    public void CreateSelectItem(List<GameObject> selectedBlocks)
    {
        if (GameObject.FindGameObjectWithTag("selectObject") != null) Destroy(GameObject.FindGameObjectWithTag("selectObject"));
        if (GameObject.FindGameObjectsWithTag("HexagonalShade") != null)
        {
            foreach (var item in GameObject.FindGameObjectsWithTag("HexagonalShade"))
            {
                Destroy(item);
            }
        }
        Vector3 averagePoint = new Vector3(0, 0, 0);
        for (int i = 0; i < selectedBlocks.Count; i++)
        {
            averagePoint += selectedBlocks[i].transform.position;
            GameObject hexagonalShadeClone = Instantiate(hexagonalShade,
                new Vector3(selectedBlocks[i].transform.position.x + 0.075f,
                selectedBlocks[i].transform.position.y + 0.075f,
                selectedBlocks[i].transform.position.z),
                Quaternion.identity);
            hexagonalShadeClone.transform.parent = selectedBlocks[i].transform;
        }

        averagePoint /= selectedBlocks.Count;
        hexagonalCenterItemClone = Instantiate(hexagonalCenterItem, averagePoint, Quaternion.identity);
        hexagonalCenterItemClone.transform.position += new Vector3(0, 0, -5);
        for (int i = 0; i < selectedBlocks.Count; i++)
        {
            selectedBlocks[i].transform.parent = hexagonalCenterItemClone.transform;
        }
    }

    

    

    public List<GameObject> FindNearestThreeBlock(Vector3 referencePos)
    {
        List<GameObject> nearestBlocks = new List<GameObject>();

        List<GameObject> blocksGo = GameObject.FindGameObjectsWithTag("HexagonalBlock").ToList();
        nearestBlocks = blocksGo.OrderBy(go => (go.transform.position - referencePos).sqrMagnitude).Take(3).ToList();

        while (!nearestBlocks[1].GetComponent<BlockProperties>().TouchingBlocks.Contains(nearestBlocks[2]))
        {
            blocksGo.Remove(nearestBlocks[2]);
            nearestBlocks = blocksGo.OrderBy(go => (go.transform.position - referencePos).sqrMagnitude).Take(3).ToList();
        }
        return nearestBlocks;
    }
    
    public void DestroySelectItem()
    {
        if (GameObject.FindGameObjectWithTag("selectObject") != null)
        {
            selectedBlocks[0].transform.parent = null;
            selectedBlocks[1].transform.parent = null;
            selectedBlocks[2].transform.parent = null;
            Destroy(GameObject.FindGameObjectWithTag("selectObject"));
        }
        if (GameObject.FindGameObjectsWithTag("HexagonalShade") != null)
        {
            foreach (var item in GameObject.FindGameObjectsWithTag("HexagonalShade"))
            {
                Destroy(item);
            }
        }
    }

    public List<GameObject> GetSameColorTouchingBlocks(GameObject blockToCheck)
    {
        Color blockToCheckColor = blockToCheck.GetComponent<SpriteRenderer>().color;
        BlockProperties blockToCheckProperties = blockToCheck.GetComponent<BlockProperties>();
        List<GameObject> sameColorWithBlockToCheck = new List<GameObject>();
        for (int i = 0; i < blockToCheckProperties.TouchingBlocks.Count; i++)
        {
            if (blockToCheckProperties.TouchingBlocks[i].GetComponent<SpriteRenderer>().color == blockToCheckColor) sameColorWithBlockToCheck.Add(blockToCheckProperties.TouchingBlocks[i]);
        }
        return sameColorWithBlockToCheck;
    }

    public IEnumerator AddScore(List<GameObject> explodedBlocks)
    {
        for (int i = 0; i < explodedBlocks.Count; i++)
        {
            score += scoreConstant * explodedBlocks[i].GetComponent<BlockProperties>().scoreMultiplier;
        }
        scoreText.text = "Score: " + score;
        if (PlayerPrefs.GetInt("HighScore") < score)
        {
            PlayerPrefs.SetInt("HighScore", score);
            highScoreText.text = "High Score: " + score;
        }
        yield return null;
    }
}
