using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    private InitializeLevel init;
    private MyGrid grid;

    public float score = 0;
    public float scoreConstant;
    public int totalHexagonExploded = 0;
    public int totalStarExploded = 0;
    public int totalBombExploded = 0;
    public int totalCombo = 0;
    public int totalMoves = 0;

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
    public Text totalMovesText;
    private Text highScoreText;

    private System.Random random;

    private float dropHeight;
    public float dropWaitTimePerBlockFromTop;
    public float dropWaitTimePerBlockInGrid;

    public float dropTimeFromTop;
    public float dropTimeInGrid;

    public bool isDropDownFinished { get; set; }
    public bool isCreateBlockFinished { get; set; }

    private bool isBombCalled = false;
    private float timesBombCalled = 0;

    public GameObject GameOverPanel;
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
        totalMovesText = GameObject.Find("MovesValue").GetComponent<Text>();

        highScoreText.text = "High Score: " + PlayerPrefs.GetFloat("HighScoreF").ToString();
    }

    public IEnumerator DestroyBlocks(List<GameObject> destroyList)
    {
        AddScore(destroyList);

        for (int k = destroyList.Count - 1; k >= 0; k--)
        {
            if (destroyList.Count > 0)
            {
                grid.RemoveHexagonal(destroyList[k]);
            }
        }
        yield return null;
        StartCoroutine(MoveObjects());
    }

    public IEnumerator MoveObjects() {

        Coroutine dropDown = StartCoroutine(DropBlockDown());
        yield return dropDown;
        yield return new WaitForSeconds(dropTimeInGrid);

        Coroutine create =  StartCoroutine(CreateNewBlocks());
        yield return create;
        yield return new WaitForSeconds(dropTimeFromTop);

        isDropping = false;
        yield return StartCoroutine(CheckExplodeAfterDropBlock());
    }

    public IEnumerator DropBlockDown()
    {
        print("DropDown Started");
        isDropDownFinished = false;
        GameObject hexagonal;
        Vector2 targetCoordinate;

        List<GameObject> blockDropList = new List<GameObject>();

        for (int j = grid.CellCountY - 1; j >= 0; j--)
        {
            for (int i = 0; i < grid.CellCountX; i++)
            {
                if (grid.gridItems[i, j] != null) hexagonal = grid.gridItems[i, j];
                else continue;

                targetCoordinate = grid.GetDropCoordinate(hexagonal);
                if (targetCoordinate == new Vector2(-1, -1)) continue;

                grid.gridItems[(int)targetCoordinate.x, (int)targetCoordinate.y] = hexagonal;
                grid.gridItems[i, j] = null;

                hexagonal.GetComponent<BlockProperties>().targetPos = grid.gridPosition[(int)targetCoordinate.x,(int)targetCoordinate.y];
                hexagonal.GetComponent<BlockProperties>().gridCoordinate = targetCoordinate;
                hexagonal.GetComponent<BlockProperties>().LerpTime = dropTimeInGrid;

                blockDropList.Add(hexagonal);
            }
        }

        isDropping = true;

        for (int i = 0; i < blockDropList.Count; i++)
        {
            blockDropList[i].GetComponent<BlockProperties>().Drop(dropTimeInGrid);
            yield return new WaitForSeconds(dropTimeInGrid);
        }
        isDropDownFinished = true;
        print("DropDown Finished");
        yield break;
    }

    public IEnumerator CreateNewBlocks()
    {
        print("Create Started");

        isCreateBlockFinished = false;

        List<GameObject> newBlocks = new List<GameObject>();
        dropHeight = Camera.main.orthographicSize + grid.gridCellDistanceY;

        List<Vector2> emptyGrids = grid.EmptyGridCoordinates();
        for (int i = emptyGrids.Count - 1; i >= 0; i--)
        {
            Vector2 createPosition = grid.gridPosition[(int)emptyGrids[i].x, (int)emptyGrids[i].y];
            createPosition.y += dropHeight;

            GameObject hexagonelBlockClone = Instantiate(init.hexagonalBlock, createPosition, Quaternion.identity);

            hexagonelBlockClone.GetComponent<SpriteRenderer>().color = init.ColorList[random.Next(0, init.ColorList.Count)];
            hexagonelBlockClone.GetComponent<BlockProperties>().gridCoordinate = new Vector2(emptyGrids[i].x, emptyGrids[i].y);
            hexagonelBlockClone.GetComponent<BlockProperties>().targetPos = grid.gridPosition[(int)emptyGrids[i].x, (int)emptyGrids[i].y];
            grid.gridItems[(int)emptyGrids[i].x, (int)emptyGrids[i].y] = hexagonelBlockClone;

            if (isBombCalled)
            {
                hexagonelBlockClone.GetComponent<BlockProperties>().ChangeToBomb();
                isBombCalled = false;
            }
            else if (random.Next(0, 101) > 100 - init.HexagonWithStarRatio && init.HexagonWithStarRatio <= 100 && init.HexagonWithStarRatio >= 0)
            {
                if (init.HexagonWithStarRatio != 0) hexagonelBlockClone.GetComponent<BlockProperties>().ChangeToStar();
            }
            
            newBlocks.Add(hexagonelBlockClone);
        }
        yield return StartCoroutine(init.ChangeColorOnInitialize(newBlocks));
        isDropping = true;
        for (int i = 0; i < newBlocks.Count; i++)
        {
            newBlocks[i].GetComponent<BlockProperties>().Drop(dropTimeFromTop);
            yield return new WaitForSeconds(dropWaitTimePerBlockFromTop);
        }

        isCreateBlockFinished = true;
        print("Create Finished");

        yield break;
    }

    public IEnumerator CheckExplodeAfterDropBlock()
    {
        print("Checked Started");

        isDropping = false;
        bool isExplode = false;
        List<GameObject> blocksWillDestroy = new List<GameObject>();
        for (int i = 0; i < grid.CellCountX; i++)
        {
            for (int j = 0; j < grid.CellCountY - 1; j++)
            {
                Vector3 checkPosition = grid.gridPosition[i, j];
                checkPosition.y -= grid.gridCellDistanceY / 2;
                checkPosition.x += grid.gridCellDistanceX / 2;

                List<GameObject> hexagonalGroup;

                hexagonalGroup = FindNearestThreeBlock(checkPosition);
                blocksWillDestroy.AddRange(GetBlocksCanExplode(hexagonalGroup));
                if (blocksWillDestroy.Count > 0)
                {
                    isExplode = true;
                }
            }
        }
        if (isExplode)
        {
            blocksWillDestroy= blocksWillDestroy.Distinct().ToList();
            print("Destroy Started");
            yield return StartCoroutine(DestroyBlocks(blocksWillDestroy));
            yield break;
        }
        else
        {
            TriggerBomb();
            selectedBlocks = FindNearestThreeBlock(rotateStartPosition);
            CreateSelectItem(selectedBlocks);
            isExplodedOnRotate = false;
            isDropping = false;
            print("Checked Finished");
            yield break;
        }
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
                            if ((centerBlocks[i].GetComponent<BlockProperties>().isBomb
                                && (sameColor1stBlocksToOthers[j].GetComponent<BlockProperties>().hasStar
                                || sameColor1stBlocksToCenter[j].GetComponent<BlockProperties>().hasStar))

                                ||

                                (centerBlocks[i].GetComponent<BlockProperties>().hasStar
                                && (sameColor1stBlocksToOthers[j].GetComponent<BlockProperties>().isBomb
                                || sameColor1stBlocksToCenter[j].GetComponent<BlockProperties>().isBomb)))
                            {
                                    blocksCanExplode.AddRange(grid.GetAllIBlocksWithSameColor(centerBlocks[i].GetComponent<SpriteRenderer>().color));
                                totalCombo++;
                            }
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

        List<GameObject> blocksGo = grid.GridItemsToList();
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

    public void TriggerBomb()
    {
        List<GameObject> allBlocks =  grid.GridItemsToList();
        for (int i = 0; i < allBlocks.Count; i++)
        {
            if (allBlocks[i].GetComponent<BlockProperties>().isBomb) allBlocks[i].GetComponent<BlockProperties>().DeacreseCountdown();
        }
    }

    public void GameIsOver()
    {
        StopAllCoroutines();
        List<GameObject> allBlocks = grid.GridItemsToList();
        for (int i = allBlocks.Count - 1; i >= 0; i--)
        {
            Destroy(allBlocks[i]);

        }
        GameOverPanel.SetActive(true);
        GameObject.Find("BlocksDestroyedText").GetComponent<Text>().text = "Blocks destroyed: x" + totalHexagonExploded;
        GameObject.Find("CombosText").GetComponent<Text>().text = "Combos : x" + totalCombo;
        GameObject.Find("BombsText").GetComponent<Text>().text = "Bombs : x" + totalBombExploded;
        GameObject.Find("ScoreText").GetComponent<Text>().text = "Score : " + score;
    }

    public void Retry()
    {
        SceneManager.LoadScene(0);
    }

    public void AddScore(List<GameObject> blocks)
    {
        float groupScore=0;
        for (int i = 0; i < blocks.Count; i++)
        {
            groupScore += scoreConstant * blocks[i].GetComponent<BlockProperties>().scoreMultiplier;
        }
        score += groupScore;
        scoreText.text = "Score: " + score;
        if (score >= 1000 * (timesBombCalled + 1))
        {
            timesBombCalled++;
            isBombCalled = true;
        }
        if (PlayerPrefs.GetFloat("HighScoreF") < score)
        {
            PlayerPrefs.SetFloat("HighScoreF", score);
            highScoreText.text = "High Score: " + score;
        }
    }
}
