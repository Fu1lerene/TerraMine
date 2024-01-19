using Assets;
using Assets.Classes;
//using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GenerateMap : MonoBehaviour
{
    public GameObject ChunkPref;

    public List<GameObject> map = new List<GameObject>();
    public List<GameObject> newChunks;
    public List<GameObject> loadingChunks;
    public List<GameObject> oldChunks;

    private int distLoad = GenerateParams.LoadingDistance;
    private int startCountChunks = GenerateParams.StartCountChunks;
    private int sizeCh = GenerateParams.SizeChunk;

    private void Awake()
    {
        for (int i = 0; i < 2 * startCountChunks + 1; i++)
        {
            for (int j = 0; j < 2 * startCountChunks + 1; j++)
            {
                map.Add(Instantiate(ChunkPref, new Vector3(i * sizeCh, j * sizeCh, 0), Quaternion.identity, transform)); /// ����� +0,5f
            }
        }
    }

    void Start()
    {

    }

    public void LCDChunks(float x0, float y0)
    {
        newChunks = new List<GameObject>();
        loadingChunks = new List<GameObject>();
        oldChunks = new List<GameObject>();

        for (int i = 0; i < 2 * distLoad + 1; i++)
        {
            for (int j = 0; j < 2 * distLoad + 1; j++)
            {
                LoadChunks(x0, y0, loadingChunks, i, j, out float x, out float y, out bool existFlag);
                CreateNewChunks(x, y, existFlag);
            }
        }
        DeactivateChunks(x0, y0);
        map.AddRange(newChunks);
    }

    private void DeactivateChunks(float x0, float y0)
    {
        foreach (var chunk in map.Where(chunk => chunk.activeSelf))
        {
            bool checkLeft = chunk.transform.position.x < x0 - distLoad * sizeCh;
            bool checkRight = chunk.transform.position.x > x0 + distLoad * sizeCh;
            bool checkTop = chunk.transform.position.y > y0 + distLoad * sizeCh;
            bool checkBottom = chunk.transform.position.y < y0 - distLoad * sizeCh;
            if (checkLeft || checkRight || checkTop || checkBottom)
            {
                if (!oldChunks.Contains(chunk))
                {

                    chunk.SetActive(false);
                    oldChunks.Add(chunk);
                }
            }
        }
    }

    private void CreateNewChunks(float x, float y, bool existFlag)
    {
        if (!existFlag)
        {
            newChunks.Add(Instantiate(ChunkPref, new Vector3(x, y, 0), Quaternion.identity, transform));
        }
    }

    private void LoadChunks(float x0, float y0, List<GameObject> loadChunks, int i, int j, out float x, out float y, out bool existFlag)
    {
        x = x0 - distLoad * sizeCh + i * sizeCh;
        y = y0 - distLoad * sizeCh + j * sizeCh;
        existFlag = false;
        foreach (var chunk in map)
        {
            if (chunk.transform.position.x == x && chunk.transform.position.y == y)
            {
                existFlag = true;
                if (!chunk.activeSelf)
                {
                    loadChunks.Add(chunk);
                    chunk.SetActive(true);
                    break;
                }
            }
        }
    }



    private void Update()
    {

    }
}
