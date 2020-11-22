﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;

public class World : MonoBehaviour
{
    private const string mainDir = @"Assets/Resources/Scripts Content";

    public Star[] starsAll = null;
    public Planet[] planetsAll = null;

    public static Vector2Int mapSize = new Vector2Int(10, 10);

    public static Vector2Int cellSize = new Vector2Int(10, 10);
    private const float marginPrc = 0.3f;

    private const int minPlanets = 2;
    private const int maxPlanets = 5;

    private static Sys[,] systems;
    public static Vector2Int currSysId { get; private set; }

    public class Sys
    {
        public string name;
        public float security;

        public Vector2 localCoords;
        public Vector2Int cellCoords;

        public Star star;
        public List<Planet> planets;

    
        public Vector2 GlobalPos()
        {
            Vector2 globalPos = (cellCoords * cellSize) + localCoords;
            return globalPos;
        }

        public string FormatSec()
        {
            string str = System.Math.Round(this.security, 1).ToString();
            string strFinal = "";

            foreach(char ch in str)
            {
                if(ch != ',')
                {
                    strFinal += ch;
                }
                else
                {
                    strFinal += '.';
                }
            }
            return strFinal;
        }
    }

    void Awake()
    {
        systems = new Sys[mapSize.x, mapSize.y];
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        string[,] systemNames = new string[mapSize.x, mapSize.y];
        float[,] systemSecs = new float[mapSize.x, mapSize.y];
        Star[,] systemStars = new Star[mapSize.x, mapSize.y];
        List<Planet>[,] systemPlanets = new List<Planet>[mapSize.x, mapSize.y]; //Array of lists.
        Vector2[,] systemCoords = new Vector2[mapSize.x, mapSize.y];

        //Get system names from a file.
        #region Names
        var systemNamesFilePath = mainDir + "/systemNames.txt";
        var systemNamesFile = new StreamReader(systemNamesFilePath);

        List<int> systemNamesIdsAll = new List<int>();

        int nameCount = 0;
        while (systemNamesFile.ReadLine() != null) //Count all names.
        {
            systemNamesIdsAll.Add(nameCount);
            nameCount++;
        }

        int[,] systemNamesIds = new int[mapSize.x, mapSize.y];

        for (int x = 0; x <= mapSize.x - 1; x++) //Generate random system name ids.
        {
            for (int y = 0; y <= mapSize.y - 1; y++)
            {
                int random = Random.Range(0, systemNamesIdsAll.Count - 1);
                int systemNameId = systemNamesIdsAll[random];
                systemNamesIdsAll.RemoveAt(random);

                systemNamesIds[x, y] = systemNameId;
            }
        }

        systemNamesFile.DiscardBufferedData();
        systemNamesFile.BaseStream.Seek(0, SeekOrigin.Begin);
        systemNamesFile.BaseStream.Position = 0;

        string line;
        int j = 0;

        while ((line = systemNamesFile.ReadLine()) != null)
        {
            for (int x = 0; x <= mapSize.x - 1; x++) //Check if the current line is a valid system name.
            {
                for (int y = 0; y <= mapSize.y - 1; y++)
                {
                    if (systemNamesIds[x, y] == j)
                    {
                        systemNames[x, y] = line;
                        goto endOfCheck;
                    }
                }
            }
            endOfCheck:
            j++;
        }

        systemNamesFile.Close();
        #endregion

        //Generate world security levels using Perlin Noise.
        #region Security
        const float mapScale = 20f;

        Vector2 perlinSeeds = GetSecPerlinSeed(mapScale);

        for (int x = 0; x <= mapSize.x - 1; x++)
        {
            float xFloat = (float)x / mapSize.x * mapScale + perlinSeeds.x;

            for (int y = 0; y <= mapSize.y - 1; y++)
            {
                float yFloat = (float)y / mapSize.y * mapScale + perlinSeeds.y;
                systemSecs[x, y] = Mathf.PerlinNoise(xFloat, yFloat);
            }
        }

        #endregion

        //Generate stars of the systems.
        #region Stars
        for (int x = 0; x <= mapSize.x - 1; x++)
        {
            for (int y = 0; y <= mapSize.y - 1; y++)
            {
                int randomId = Random.Range(0, starsAll.Length - 1);

                Star star = starsAll[randomId];
                star.temperature = Random.Range(star.minTemperature, star.maxTemperature);

                systemStars[x, y] = star;
            }
        }
        #endregion

        //Generate planets of the systems.
        #region Planets
        int planetsCount = Random.Range(minPlanets, maxPlanets);

        for (int x = 0; x <= mapSize.x - 1; x++)
        {
            for (int y = 0; y <= mapSize.y - 1; y++)
            {
                List <Planet> planetList = new List<Planet>();

                for(int i = 0; i <= planetsCount - 1; i++)
                {
                    int randomId = Random.Range(0, planetsAll.Length - 1);
                    planetList.Add(planetsAll[randomId]);
                }

                systemPlanets[x, y] = planetList;
            }
        }
        #endregion

        //Place the systems on a map.
        #region Coords
        const float marginPrc = 0.3f;

        Vector2 margin = new Vector2
        {
            x = cellSize.x / 2 - (cellSize.x / 2 * marginPrc),
            y = cellSize.y / 2 - (cellSize.y / 2 * marginPrc)
        };

        for (int x = 0; x <= mapSize.x - 1; x++)
        {
            for (int y = 0; y <= mapSize.y - 1; y++)
            {
                Vector2 systemCoord = new Vector2
                {
                    x = Mathf.Clamp(Random.Range(-(cellSize.x / 2f), (cellSize.x / 2f)), -margin.x, margin.x),
                    y = Mathf.Clamp(Random.Range(-(cellSize.y / 2f), (cellSize.y / 2f)), -margin.y, margin.y)
                };

                systemCoords[x, y] = systemCoord;
            }
        }
        #endregion

        //Save to system objects array.
        #region Save
        for (int x = 0; x <= mapSize.x - 1; x++)
        {
            for (int y = 0; y <= mapSize.y - 1; y++)
            {
                Sys sys = new Sys
                {
                    name = systemNames[x, y],
                    security = systemSecs[x, y],
                    star = systemStars[x, y],
                    planets = systemPlanets[x, y],
                    localCoords = systemCoords[x, y],
                    cellCoords = new Vector2Int(x, y)
                };

                systems[x, y] = sys;
            }
        }
        #endregion
    }

    public static void SetSystem(Vector2Int id)
    {
        if(id.x < mapSize.x && id.y < mapSize.y )
        {
            currSysId = id;
        }
    }

    public static Sys GetSystem() //Get current.
    {
        return systems[currSysId.x, currSysId.y];
    }
    
    public static Vector2 GetSecPerlinSeed(float mapScale) //Gets a seed that generates high sec in the first systems.
    {
        int x, y;

        float seedX, seedY;

        float floatX, floatY;

        float[] perlinValues = new float[3];

        while(true)
        {
            seedX = Random.Range(0f, 1000000f);
            seedY = Random.Range(0f, 1000000f);

            x = 0; y = 0;                             //[0,0]
            floatX = (float)x / mapSize.x * mapScale + seedX;
            floatY = (float)y / mapSize.y * mapScale + seedY;
            perlinValues[0] = Mathf.PerlinNoise(floatX, floatY);

            x = 1; y = 0;                             //[1,0]
            floatX = (float)x / mapSize.x * mapScale + seedX;
            floatY = (float)y / mapSize.y * mapScale + seedY;
            perlinValues[1] = Mathf.PerlinNoise(floatX, floatY);

            x = 0; y = 1;                             //[0,1]
            floatX = (float)x / mapSize.x * mapScale + seedX;
            floatY = (float)y / mapSize.y * mapScale + seedY;
            perlinValues[2] = Mathf.PerlinNoise(floatX, floatY);

            if (perlinValues[0] >= 0.8f && perlinValues[1] >= 0.7f && perlinValues[2] >= 0.7f)
                    break;
        }

        Vector2 perlinSeeds = new Vector2
        {
            x = seedX,
            y = seedY
        };

        return perlinSeeds;
    }

    public static Sys GetSystem(Vector2Int id) //Get by id.
    {
        return systems[id.x, id.y];
    }
}
