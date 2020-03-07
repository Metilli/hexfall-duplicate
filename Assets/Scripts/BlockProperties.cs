using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockProperties : MonoBehaviour
{
    public Vector2 gridCoordinate;
    public Vector2 targetPos;
    public bool isDropping;
    public int scoreMultiplier;
    public List<GameObject> TouchingBlocks = new List<GameObject>();
    float speed = 4.6f;

    private void Update()
    {
        if (isDropping)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            if (Mathf.Abs(transform.position.y - targetPos.y) < 0.0001f) isDropping = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TouchingBlocks.Add(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TouchingBlocks.Remove(collision.gameObject);
    }
}
