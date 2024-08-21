using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This script creates a button on the editor to generate all the shape data from the resources folder
/// which is then parsed into block positions and the block's type to be then used when spawning blocks
/// </summary>
public class BlockInfoDownloader : MonoBehaviour
{
#if UNITY_EDITOR

    // Paths
    private static string _downloadPath = Application.dataPath + "/Resources/";
    private static string _savePath = "Assets/Data/AllShapeData.asset";

    // Creates button named "Tools" on the toolbar of Unity
    [MenuItem("Tools/GenerateShapeData")]
    private static void GetShapeTextData()
    {
        // Generate a list of files from the path to download from
        List<FileInfo> updatedFiles = GetFileInfo(_downloadPath);
        
        // Creates the data asset and set the length of the RawBlockInfo as the total shape text documents
        AllShapesScritpableObject allShapeData = ScriptableObject.CreateInstance<AllShapesScritpableObject>();
        allShapeData.RawBlockInfo = new RawBlockInfo[updatedFiles.Count];

        for (int totalFileIndex = 0; totalFileIndex < updatedFiles.Count; totalFileIndex++)
        {
            // Gets the text document and reads it into a single string
            StreamReader reader = new StreamReader(updatedFiles[totalFileIndex].FullName);
            string shapeText = reader.ReadToEnd();
            // Split the string for every new line and place it into a array of strings (divided by rows)
            string[] shapeRowStrings = shapeText.Split("\n");

            // Parses header text into the created BlockTypes enum class
            BlockTypes blockType = BlockTypes.o_shape;
            if (Enum.TryParse(shapeRowStrings[0].ToLower(), out BlockTypes block))
            {
                blockType = block;
            }

            // Creates a new RawBlockInfo struct to save the BlockType, and the BlockPositions
            RawBlockInfo newBlockInfo = new RawBlockInfo();

            newBlockInfo.BlockType = blockType;
            newBlockInfo.BlockPositions = GetPositions(shapeRowStrings);

            // Sets the RawBlockInfo in the DataAsset at the current index as the created RawBlockInfo
            allShapeData.RawBlockInfo[totalFileIndex] = newBlockInfo;
        }

        // Saves this created scriptable object
        SaveCreatedData(allShapeData);
    }

    // Gets all the files from the path without any .meta files
    private static List<FileInfo> GetFileInfo(string path)
    {
        // Gets the directory based off the given path, then gets all files inside the path
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] files = dir.GetFiles();
        List<FileInfo> updatedFiles = new();
        // Parse through the files and only add files that dont have ".meta" in their file name
        foreach (FileInfo file in files)
        {
            if (file.FullName.Contains(".meta")) { }
            else
            {
                updatedFiles.Add(file);
            }
        }

        // Return all files that dont have ".meta" in their file name
        return updatedFiles;
    }

    // Gets all the positions of the shape inside the text document
    private static Vector2[] GetPositions(string[] textData)
    {
        int blockInterval = 0;
        Vector2[] blockLocations = new Vector2[4];
        Vector2[] updatedBlockLocations = new Vector2[4];
        Vector2 originLocation = new();

        // For each row in the textData
        for (int i = 1; i < textData.Length; i++)
        {
            // Split the rows into columns to isolate a single character
            char[] shapeCharacters = textData[i].ToCharArray();
            for (int j = 0; j < shapeCharacters.Length; j++)
            {
                // Y index used to set blocks to spawn up-right instead of upside-down
                int yIndex;
                // If the character is either an O or X, save the position
                if (shapeCharacters[j] == 'O' || shapeCharacters[j] == 'X')
                {
                    yIndex = shapeCharacters.Length - i;
                    blockLocations[blockInterval].x = j;
                    blockLocations[blockInterval].y = yIndex;

                    // If the character is an O
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

        // After getting all the raw positions, we need to set the rotational pivot as 0,0 to rotate the other shapes easily.
        // Confines the other block locations to the origins location
        for (int z = 0; z < blockLocations.Length; z++)
        {
            blockLocations[z].x -= originLocation.x;
            blockLocations[z].y -= originLocation.y;
            updatedBlockLocations[z] = blockLocations[z];
        }

        return updatedBlockLocations;
    }

    // Checks if the created data already exists, if it does, update the existing data with the new values
    // If it dosent exist, create the asset
    private static void SaveCreatedData(AllShapesScritpableObject createdData)
    {
        if (Directory.Exists(_savePath))
        {
            AllShapesScritpableObject existingData = AssetDatabase.LoadAssetAtPath<AllShapesScritpableObject>(_savePath);
            existingData = createdData;
            EditorUtility.SetDirty(existingData);
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.CreateAsset(createdData, _savePath);
        }

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = createdData;
    }

#endif
}
