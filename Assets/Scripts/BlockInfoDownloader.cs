using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BlockInfoDownloader : MonoBehaviour
{
#if UNITY_EDITOR

    public static BlockInfoDownloader Instance;

    private void Awake()
    {
        Instance = this;
    }

    [MenuItem("Tools/GenerateShapeData")]
    private static void GetShapeTextData()
    {
        string path = Application.dataPath + "/Resources/";
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] files = dir.GetFiles();
       
        AllShapesScritpableObject allShapeData = ScriptableObject.CreateInstance<AllShapesScritpableObject>();
        allShapeData.BlockInfo = new BlockInfo[files.Length / 2];
        for (int totalFileIndex = 0; totalFileIndex < files.Length / 2; totalFileIndex++)
        {
            if (files[totalFileIndex].FullName.Contains(".meta"))
            {
                totalFileIndex--;
                continue;
            }

            StreamReader reader = new StreamReader(files[totalFileIndex].FullName);
            string shapeText = reader.ReadToEnd();
            string[] shapeRowStrings = shapeText.Split("\n");
            BlockTypes blockType = (BlockTypes)Enum.Parse(typeof(BlockTypes), shapeRowStrings[0]); 
            int blockInterval = 0;
            Vector2[] blockLocations = new Vector2[4];
            Vector2[] updatedBlockLocations = new Vector2[4];
            Vector2 originLocation = new();



            // Row
            for (int i = 1; i < shapeRowStrings.Length; i++)
            {
                // Column
                char[] shapeCharacters = shapeRowStrings[i].ToCharArray();
                for (int j = 0; j < shapeCharacters.Length; j++)
                {
                    int yIndex;
                    if (shapeCharacters[j] == 'O' || shapeCharacters[j] == 'X')
                    {
                        yIndex = shapeCharacters.Length - i;
                        blockLocations[blockInterval].x = j;
                        blockLocations[blockInterval].y = yIndex;

                        // Sets the rotational pivot (origin)
                        if (shapeCharacters[j] == 'O')
                        {
                            originLocation.x = blockLocations[blockInterval].x;
                            originLocation.y = blockLocations[blockInterval].y;
                        }

                        blockInterval++;
                    }
                }
            }

            // Confines the other block locations to the origins location
            for (int z = 0; z < blockLocations.Length; z++)
            {
                blockLocations[z].x -= originLocation.x;
                blockLocations[z].y -= originLocation.y;
                updatedBlockLocations[z] = blockLocations[z];
            }

            BlockInfo newBlockInfo = new BlockInfo();

            newBlockInfo.BlockChance = 5;
            newBlockInfo.BlockType = blockType;
            newBlockInfo.BlockPositions = updatedBlockLocations;

            allShapeData.BlockInfo[totalFileIndex] = newBlockInfo;
        }

        string name = ($"Assets/Data/AllShapeData.asset");
        if (Directory.Exists(name))
        {
            AssetDatabase.DeleteAsset(name);
        }
        AssetDatabase.CreateAsset(allShapeData, name);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}
