using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockTouchHandler : MonoBehaviour
{
    private Manager manager;
    private MyGrid grid;

    private void Awake()
    {
        manager = GameObject.Find("GameManager").GetComponent<Manager>();
        grid = GameObject.Find("GameManager").GetComponent<MyGrid>();
    }

    public void OnMouseDown()
    {
        if (manager.isRotating == false && manager.isExplodedOnRotate == false && manager.isDropping == false)
        {
            manager.isRotatedOnLastDrag = false;
            manager.rotateStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (manager.isRotating == false && manager.isExplodedOnRotate == true && manager.isDropping == false )
        { 
            manager.isExplodedOnRotate = false;
            if (manager.selectedBlocks.Count > 0)
            {
                for (int i = 0; i < manager.selectedBlocks.Count; i++)
                {
                    if (manager.selectedBlocks[i] != null) manager.selectedBlocks[i].transform.parent = null;
                }
            }
            manager.selectedBlocks = manager.FindNearestThreeBlock(manager.rotateStartPosition);
            manager.CreateSelectItem(manager.selectedBlocks);
        }
    }
    private void OnMouseDrag()
    {
        if (manager.isRotating == false && manager.isExplodedOnRotate == false && manager.isDropping == false )
        {
            if (manager.selectedBlocks.Count == 3)
            {
                if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0 || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0)
                {
                    manager.isRotatedOnLastDrag = true;
                    Vector2 mouseInputForce = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                    Vector2 distanceBetweenObjects = (Vector2)manager.hexagonalCenterItemClone.transform.position - manager.rotateStartPosition;
                    distanceBetweenObjects = new Vector2(Mathf.Abs(distanceBetweenObjects.x), Mathf.Abs(distanceBetweenObjects.y));

                    RotateDirection rotateDirection;
                    if (manager.rotateStartPosition.y > manager.hexagonalCenterItemClone.transform.position.y
                        && manager.rotateStartPosition.x > manager.hexagonalCenterItemClone.transform.position.x)
                    { //1. bölge
                        if (mouseInputForce.x >= 0 && mouseInputForce.y >= 0)
                        {
                            if (Mathf.Abs(mouseInputForce.x) * distanceBetweenObjects.y > Mathf.Abs(mouseInputForce.y) * distanceBetweenObjects.x) rotateDirection = RotateDirection.Clockwise;
                            else rotateDirection = RotateDirection.CounterClockwise;
                        }
                        else if (mouseInputForce.x >= 0 && mouseInputForce.y <= 0) rotateDirection = RotateDirection.Clockwise;
                        else if (mouseInputForce.x <= 0 && mouseInputForce.y >= 0) rotateDirection = RotateDirection.CounterClockwise;
                        else
                        {
                            if (Mathf.Abs(mouseInputForce.x) * distanceBetweenObjects.y > Mathf.Abs(mouseInputForce.y) * distanceBetweenObjects.x) rotateDirection = RotateDirection.CounterClockwise;
                            else rotateDirection = RotateDirection.Clockwise;
                        }
                    }
                    else if (manager.rotateStartPosition.y > manager.hexagonalCenterItemClone.transform.position.y
                        && manager.rotateStartPosition.x < manager.hexagonalCenterItemClone.transform.position.x)
                    { //2. bölge
                        if (mouseInputForce.x >= 0 && mouseInputForce.y >= 0) rotateDirection = RotateDirection.Clockwise;
                        else if (mouseInputForce.x >= 0 && mouseInputForce.y <= 0)
                        {
                            if (Mathf.Abs(mouseInputForce.x) * distanceBetweenObjects.y > Mathf.Abs(mouseInputForce.y) * distanceBetweenObjects.x) rotateDirection = RotateDirection.Clockwise;
                            else rotateDirection = RotateDirection.CounterClockwise;
                        }
                        else if (mouseInputForce.x <= 0 && mouseInputForce.y >= 0)
                        {
                            if (Mathf.Abs(mouseInputForce.x) * distanceBetweenObjects.y > Mathf.Abs(mouseInputForce.y) * distanceBetweenObjects.x) rotateDirection = RotateDirection.CounterClockwise;
                            else rotateDirection = RotateDirection.Clockwise;
                        }
                        else rotateDirection = RotateDirection.CounterClockwise;
                    }
                    else if (manager.rotateStartPosition.y < manager.hexagonalCenterItemClone.transform.position.y
                       && manager.rotateStartPosition.x < manager.hexagonalCenterItemClone.transform.position.x)
                    { //3. bölge
                        if (mouseInputForce.x >= 0 && mouseInputForce.y >= 0)
                        {
                            if (Mathf.Abs(mouseInputForce.x) * distanceBetweenObjects.y > Mathf.Abs(mouseInputForce.y) * distanceBetweenObjects.x) rotateDirection = RotateDirection.CounterClockwise;
                            else rotateDirection = RotateDirection.Clockwise;
                        }
                        else if (mouseInputForce.x >= 0 && mouseInputForce.y <= 0) rotateDirection = RotateDirection.CounterClockwise;
                        else if (mouseInputForce.x <= 0 && mouseInputForce.y >= 0) rotateDirection = RotateDirection.Clockwise;
                        else
                        {
                            if (Mathf.Abs(mouseInputForce.x) * distanceBetweenObjects.y > Mathf.Abs(mouseInputForce.y) * distanceBetweenObjects.x) rotateDirection = RotateDirection.Clockwise;
                            else rotateDirection = RotateDirection.CounterClockwise;
                        }
                    }
                    else
                    { //4. bölge
                        if (mouseInputForce.x >= 0 && mouseInputForce.y >= 0) rotateDirection = RotateDirection.CounterClockwise;
                        else if (mouseInputForce.x >= 0 && mouseInputForce.y <= 0)
                        {
                            if (Mathf.Abs(mouseInputForce.x) * distanceBetweenObjects.y > Mathf.Abs(mouseInputForce.y) * distanceBetweenObjects.x) rotateDirection = RotateDirection.CounterClockwise;
                            else rotateDirection = RotateDirection.Clockwise;
                        }
                        else if (mouseInputForce.x <= 0 && mouseInputForce.y >= 0)
                        {
                            if (Mathf.Abs(mouseInputForce.x) * distanceBetweenObjects.y > Mathf.Abs(mouseInputForce.y) * distanceBetweenObjects.x) rotateDirection = RotateDirection.Clockwise;
                            else rotateDirection = RotateDirection.CounterClockwise;
                        }
                        else rotateDirection = RotateDirection.Clockwise;
                    }
                    manager.isRotatedOnLastDrag = true;

                    StartCoroutine(RotateBlocksAndCheckExplode(rotateDirection, manager.blocksRotateTime));
                }
            }
            else
            {
                manager.isRotatedOnLastDrag = false;
            }
        }
    }

    private void OnMouseUp()
    {
        if (manager.isRotating == false && manager.isRotatedOnLastDrag == false && manager.isExplodedOnRotate == false && manager.isDropping == false)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            if (hit.rigidbody != null)
            {
                if (manager.selectedBlocks.Count > 0)
                {
                    for (int i = 0; i < manager.selectedBlocks.Count; i++)
                    {
                        if (manager.selectedBlocks[i] != null) manager.selectedBlocks[i].transform.parent = null;
                    }
                }
                manager.selectedBlocks = manager.FindNearestThreeBlock(hit.point);
                manager.CreateSelectItem(manager.selectedBlocks);
            }
        }
    }
    

    
    private IEnumerator RotateBlocksAndCheckExplode(RotateDirection direction, float rotateTime)
    {
        manager.isRotating = true;

        Vector3 rotateAngle;
        if (direction == RotateDirection.CounterClockwise) rotateAngle = Vector3.forward * 120;
        else rotateAngle = Vector3.back * 120;
        for (int i = 0; i < manager.selectedBlocks.Count; i++)
        {
            List<Vector2> transformStartPositions = getTransformPositions(manager.selectedBlocks);

            var fromAngle = manager.hexagonalCenterItemClone.transform.rotation;
            var toAngle = Quaternion.Euler(manager.hexagonalCenterItemClone.transform.eulerAngles + rotateAngle);
            for (var t = 0f; t < 1; t += Time.deltaTime / rotateTime)
            {
                for (int k = 0; k < manager.hexagonalCenterItemClone.transform.childCount; k++)
                {
                    if(manager.hexagonalCenterItemClone.transform.GetChild(k).GetComponent<BlockProperties>().isBomb == true ||
                       manager.hexagonalCenterItemClone.transform.GetChild(k).GetComponent<BlockProperties>().hasStar == true)
                        manager.hexagonalCenterItemClone.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                manager.hexagonalCenterItemClone.transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
                yield return null;
            }

            for (int k = 0; k < manager.hexagonalCenterItemClone.transform.childCount; k++)
            {
                if (manager.hexagonalCenterItemClone.transform.GetChild(k).GetComponent<BlockProperties>().isBomb == true ||
                   manager.hexagonalCenterItemClone.transform.GetChild(k).GetComponent<BlockProperties>().hasStar == true)
                    manager.hexagonalCenterItemClone.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            manager.hexagonalCenterItemClone.transform.rotation = toAngle;

            List<Vector2> transformEndPositions = getTransformPositions(manager.selectedBlocks);

            SetBlockCoordinateOnRotate(manager.selectedBlocks,transformStartPositions, transformEndPositions);

            List<List<GameObject>> upperList = new List<List<GameObject>>();
            List<GameObject> blocksWillDestroy;
            yield return blocksWillDestroy = manager.GetBlocksCanExplode(manager.selectedBlocks);
            if (blocksWillDestroy.Count > 0)
            {
                upperList.Add(blocksWillDestroy);
                manager.totalMoves++;
                manager.totalMovesText.text = manager.totalMoves.ToString();
                manager.DestroySelectItem();
                StartCoroutine(manager.DestroyBlocks(upperList));
                manager.isRotating = false;
                manager.isExplodedOnRotate = true;
                break;
            }
            yield return new WaitForSeconds(0.07f);
        }
        yield return new WaitForSeconds(0.15f);

        manager.isRotating = false;
    }

    private void SetBlockCoordinateOnRotate(List<GameObject> blocks,List<Vector2> startTransformPositions, List<Vector2> endTransformPositions)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            float xPos = (endTransformPositions[i].x - startTransformPositions[i].x)/grid.gridCellDistanceX;
            blocks[i].GetComponent<BlockProperties>().gridCoordinate.x += Mathf.Round(xPos);

            float yPosOffset=0;
            float yPos = 0;
            if (Mathf.Round(xPos) % 2 == 0)
            {
                yPosOffset = 0;
                if (endTransformPositions[i].y - startTransformPositions[i].y > 0) yPos = -1;
                else yPos = +1;
            }
            else if (endTransformPositions[i].y - startTransformPositions[i].y > 0)
            {
                yPos = -1;
                if (blocks[i].GetComponent<BlockProperties>().gridCoordinate.x % 2 == 0) yPosOffset = +1;
                else yPosOffset = 0;
            }
            else if (endTransformPositions[i].y - startTransformPositions[i].y < 0)
            {
                yPos = +1;
                if (blocks[i].GetComponent<BlockProperties>().gridCoordinate.x % 2 == 0) yPosOffset = 0;
                else yPosOffset = -1;
            }
            blocks[i].GetComponent<BlockProperties>().gridCoordinate.y += yPos + yPosOffset;
            int x = (int)blocks[i].GetComponent<BlockProperties>().gridCoordinate.x;
            int y = (int)blocks[i].GetComponent<BlockProperties>().gridCoordinate.y;

            grid.gridItems[x, y] = blocks[i];
        }
    }


    private List<Vector2> getTransformPositions(List<GameObject> blocks)
    {
        List<Vector2> transformPositions = new List<Vector2>();

        for (int j = 0; j < blocks.Count; j++)
        {
            transformPositions.Add(blocks[j].transform.position+blocks[j].transform.parent.transform.position);
        }
        return transformPositions;
    }




    enum RotateDirection
    {
        Clockwise,
        CounterClockwise
    }
}
