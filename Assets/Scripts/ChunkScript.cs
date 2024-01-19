using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Classes;


public class ChunkScript : MonoBehaviour
{
    public GameObject Land;
    public GameObject Water;
    public GameObject Sand;
    public GameObject Mountain;
    public GameObject Tree;

    public Vector2[][] NodeVectorsHeight = new Vector2[GenerateParams.CountOctaves][];
    public int ID;
    private static int _id;
    private int sizeCh = GenerateParams.SizeChunk;
    private GenerateMap genMap;
    private float x0;
    private float y0;
    private int countOct = GenerateParams.CountOctaves;


    [SerializeField]
    public List<GameObject> cells = new List<GameObject>();
    public List<float> noiseValuesHeight;
    private float waterLevel = GenerateParams.WaterLevel;
    private float sandLevel = GenerateParams.SandLevel;
    private float landLevel = GenerateParams.LandLevel;

    void Start()
    {
        ID = ++_id;
        x0 = transform.position.x;
        y0 = transform.position.y;
        for (int k = 0; k < countOct; k++)
        {
            int countAllNodes = (int)Mathf.Pow(Mathf.Pow(2, k) + 1, 2);
            NodeVectorsHeight[k] = new Vector2[countAllNodes];
        }
        genMap = GetComponentInParent<GenerateMap>();

        Invoke("GenerateCellsWithNoise", 0.01f); // ������� #1

    }
    private void GenerateCellsWithNoise()
    {
        SetNodeVectors();
        CreateCellsOnTheField();
    }

    private void CreateCellsOnTheField()
    {
        MyPerlin perlin = new MyPerlin(NodeVectorsHeight);
        noiseValuesHeight = perlin.GetNoiseValues(gameObject);
        for (int i = 0; i < sizeCh; i++)
        {
            for (int j = 0; j < sizeCh; j++)
            {
                float x = x0 - sizeCh / 2 + 0.5f + i;
                float y = y0 - sizeCh / 2 + 0.5f + j;
                if (noiseValuesHeight[i * sizeCh + j] < waterLevel)
                    cells.Add(Instantiate(Water, new Vector3(x, y, 0), Quaternion.identity, transform));
                else if (noiseValuesHeight[i * sizeCh + j] < sandLevel)
                    cells.Add(Instantiate(Sand, new Vector3(x, y, 0), Quaternion.identity, transform));
                else if (noiseValuesHeight[i * sizeCh + j] < landLevel)
                    cells.Add(Instantiate(Land, new Vector3(x, y, 0), Quaternion.identity, transform));
                else cells.Add(Instantiate(Mountain, new Vector3(x, y, 0), Quaternion.identity, transform));
                
                CellScipt celSc = cells[i * sizeCh + j].GetComponent<CellScipt>();
                celSc.value = noiseValuesHeight[i * sizeCh + j];
            }
        }
    }

    public void SetNodeVectors()
    {
        foreach (var chunk in genMap.map)
        {
            if (IsFindedChunk(chunk, "left"))
            {
                SetNodeVectorsFromOtherChunk(chunk, "left");
            }
            if (IsFindedChunk(chunk, "right"))
            {
                SetNodeVectorsFromOtherChunk(chunk, "right");
            }
            if (IsFindedChunk(chunk, "top"))
            {
                SetNodeVectorsFromOtherChunk(chunk, "top");
            }
            if (IsFindedChunk(chunk, "bottom"))
            {
                SetNodeVectorsFromOtherChunk(chunk, "bottom");
            }
        }
        SetNewNodeVectors();
    }

    private void SetNewNodeVectors()
    {
        for (int k = 0; k < countOct; k++)
        {
            int countAllNodes = (int)Mathf.Pow(Mathf.Pow(2, k) + 1, 2);
            for (int i = 0; i < countAllNodes; ++i)
            {
                if (NodeVectorsHeight[k][i] == new Vector2())
                {
                    NodeVectorsHeight[k][i] = GetRandomVector();
                }
            }
        }
    }

    private Vector2 GetRandomVector()
    {
        float x1 = (float)(2 * Random.value - 1);
        float x2 = (float)(2 * Random.value - 1);
        return new Vector2(x1 / Mathf.Sqrt(x1 * x1 + x2 * x2), x2 / Mathf.Sqrt(x1 * x1 + x2 * x2));
    }
    private void SetNodeVectorsFromOtherChunk(GameObject otherChunk, string where)
    {
        ChunkScript chSc = otherChunk.GetComponent<ChunkScript>();
        for (int k = 0; k < countOct; k++)
        {
            int countNodes = (int)Mathf.Pow(2, k) + 1;
            for (int i = 0; i < countNodes; i++)
            {
                int index = countNodes * (countNodes - 1) + i;
                int indextb1 = (i + 1) * countNodes - 1;
                int indextb2 = i * countNodes;
                switch (where)
                {
                    case "left":
                        NodeVectorsHeight[k][i] = chSc.NodeVectorsHeight[k][index]; 
                        break;
                    case "right":
                        NodeVectorsHeight[k][index] = chSc.NodeVectorsHeight[k][i];
                        break;
                    case "top":
                        NodeVectorsHeight[k][indextb1] = chSc.NodeVectorsHeight[k][indextb2];
                        break;
                    case "bottom":
                        NodeVectorsHeight[k][indextb2] = chSc.NodeVectorsHeight[k][indextb1];
                        break;

                }
            }
        }
    }

    private bool IsFindedChunk(GameObject chunk, string where)
    {
        ChunkScript chSc = chunk.GetComponent<ChunkScript>();
        if (chSc.NodeVectorsHeight[0][0] != new Vector2())
        {
            switch (where)
            {
                case "left":
                    return chunk.transform.position.x == x0 - sizeCh && chunk.transform.position.y == y0;
                case "right":
                    return chunk.transform.position.x == x0 + sizeCh && chunk.transform.position.y == y0;
                case "top":
                    return chunk.transform.position.x == x0 && chunk.transform.position.y == y0 + sizeCh;
                case "bottom":
                    return chunk.transform.position.x == x0 && chunk.transform.position.y == y0 - sizeCh;
                default: return false;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && collision.GetType() == typeof(CircleCollider2D))
        {
            genMap.LCDChunks(x0, y0);
        }

    }
    private bool IsPlayer(Collider2D collision)
    {
        return collision.tag == "Player" && collision.GetType() == typeof(BoxCollider2D);
    }
}
