using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generationDeath : MonoBehaviour
{
    public GameObject deathBox;
    private float generationSpeed = 0.45f;
    private int xPos;
    private int yPos;
    private int enemyCount = 0;
    private int enemyLimit = 20;
    private int quadrant = 1;
    public int quadrantValue = 1;
    private float speed = 5.0f;
    private Vector3 position;
    private float posX;
    private float posY;
    private Queue<GameObject> clones = new Queue<GameObject>();
    private bool hasClone = false;
    private float buffer = 7;
    private float increment = 0.045f;
    private float speedincrement = 1f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EnemyDrop());
    }

    IEnumerator EnemyDrop()
    {
        while (true)
        {
            while (enemyCount <= enemyLimit)
            {
                quadrant = Random.Range(1, 5);
                switch (quadrant)
                {
                    case 1:
                        xPos = -22 + Random.Range(0, 10);
                        yPos = -5 + Random.Range(0, 10);
                        quadrantValue = 1;
                        break;
                    case 2:
                        xPos = -12 + Random.Range(0, 20);
                        yPos = 5 + Random.Range(0, 5);
                        quadrantValue = 2;
                        break;
                    case 3:
                        xPos = -12 + Random.Range(0, 20);
                        yPos = -10 + Random.Range(0, 5);
                        quadrantValue = 3;
                        break;
                    case 4:
                        xPos = 13 + Random.Range(0, 10);
                        yPos = -5 + Random.Range(0, 10);
                        quadrantValue = 4;
                        break;

                }
                GameObject clone = (GameObject)Instantiate(deathBox, new Vector3(xPos, yPos, 0), Quaternion.identity);
                switch (quadrantValue)
                {
                    case 1:
                        clone.GetComponent<Rigidbody2D>().velocity = new Vector2(speed, 0);
                        break;
                    case 2:
                        clone.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1 * speed);
                        break;
                    case 3:
                        clone.GetComponent<Rigidbody2D>().velocity = new Vector2(0, speed);
                        break;
                    case 4:
                        clone.GetComponent<Rigidbody2D>().velocity = new Vector2(-1 * speed, 0);
                        break;

                }
                clones.Enqueue(clone);
                hasClone = true;
                yield return new WaitForSeconds(generationSpeed);
                enemyCount += 1;

            }
            enemyCount = 0;
            enemyLimit += (int)buffer;
            buffer = buffer * 1.475f;
            speed += speedincrement;
            speedincrement = speedincrement * 0.92f;
            generationSpeed -= increment;
            increment = increment * 0.97f;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (hasClone == true)
        {
            if (clones.Peek() == null)
            {
                clones.Dequeue();
                if (clones.Count == 0)
                {
                    hasClone = false;
                }
            }
            else
            {
                GameObject obj = clones.Peek();
                position = obj.transform.position;
                posX = position.x;
                posY = position.y;
                if (posX > 25 || posX < -25 || posY > 25 || posY < -25)
                {
                    Destroy(obj);
                    obj = clones.Dequeue();
                    if (clones.Count == 0)
                    {
                        hasClone = false;
                    }
                }
            }
        }
        
    }
}
