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
    public float LerpTime;
    public Manager manager;
    float perc = 0;

    private void Awake()
    {
        manager = GameObject.Find("GameManager").GetComponent<Manager>();
    }

    private void Update()
    {
        if (isDropping)
        {
            perc += Time.deltaTime / LerpTime;
            if (perc < 1)
            {
                transform.position = Vector2.Lerp(transform.position, targetPos, perc);
            }
            else
            {
                perc = 0;
                isDropping = false;
            }
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
