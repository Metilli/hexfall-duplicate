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
    private CircleCollider2D circleCollider2D;

    public bool hasStar;
    public bool isBomb;

    public int countDown;
    public TextMesh bombText;

    private Manager manager;
    float perc = 0;

    private void Awake()
    {
        manager = GameObject.Find("GameManager").GetComponent<Manager>();
        circleCollider2D = gameObject.GetComponent<CircleCollider2D>();
        bombText = gameObject.GetComponentInChildren<TextMesh>();
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
                circleCollider2D.enabled = true;
                perc = 0;
                isDropping = false;
            }
        }
    }

    public void Drop(float DropTime)
    {
        circleCollider2D.enabled = false;
        LerpTime = DropTime;
        isDropping = true;
    }

    public void ChangeToStar()
    {
        if (!isBomb)
        {
            scoreMultiplier = 2;
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/hexagonStar");
            hasStar = true;
        }
    }

    public void ChangeToBomb()
    {
        scoreMultiplier = 1;
        gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/bomb");
        bombText.text = countDown.ToString();
        isBomb = true;
    }

    public void DeacreseCountdown()
    {
        countDown -= 1;
        bombText.text = countDown.ToString();
        if (countDown <= 0) manager.GameIsOver();
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
