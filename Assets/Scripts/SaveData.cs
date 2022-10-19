using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    private List<(string, Vector2Int)> _prefabsToSave = new List<(string, Vector2Int)>();
    private List<(string, Vector2Int)> _prefabsToLoad = new List<(string, Vector2Int)>();

    private string _fileStart = "Prefab";
    private string _fileEnd = ".txt";

    private bool _endOfPrefabs = false;

    public void LoadJson()
    {
        int index = 0;

        while (!_endOfPrefabs)
        {
            (string, Vector2Int) _newData = ("", Vector2Int.zero);
            string json = ReadFromFile(index);

            if (json != "")
            {
                JsonUtility.FromJsonOverwrite(json, _newData);
                _prefabsToLoad.Add(_newData);
                index++;
            }
        }

        SpawnPrefabsFromLoad();
    }

    public void SaveIntoJson()
    {
        DeleteOldSave();

        GetCharacters();
        string json;
        int index = 0;

        foreach ((string, Vector2Int) data in _prefabsToSave)
        {
            json = JsonUtility.ToJson(data);
            WriteToFile(index, json);
            index++;
        }
        foreach (Vector2Int pos in MainManager.Map.CondemnedCells)
        {
            json = JsonUtility.ToJson(("Tree", pos));
            WriteToFile(index, json);
            index++;
        }
    }

    private void GetCharacters()
    {
        MapCell cell;

        for (int x = 0; x < MainManager.Map.Height; ++x)
        {
            for (int y = 0; y < MainManager.Map.Width; ++y)
            {
                cell = MainManager.Map.GetCell(x, y);
                if(cell.Character != null)
                {
                    _prefabsToSave.Add((cell.Character.Data.Name, new Vector2Int(x, y)));
                }
            }
        }
    }

    private void DeleteOldSave()
    {
        var existingFiles = Directory.GetFiles(Application.dataPath + "/SaveFolder");

        for (int i = 0; i < existingFiles.Length; i++)
        {
            File.Delete(existingFiles[i]);
        }
    }

    private List<Character> SpawnPrefabsFromLoad()
    {
        List<Vector2Int> treeCells = new List<Vector2Int>();
        List<CharacterData> characters = new List<CharacterData>();
        foreach ((string s, Vector2Int v) data in _prefabsToLoad)
        {
            if(data.s == "Tree")
                treeCells.Add(data.v);
            else
                characters.Add(PrefabManager.Prefabs[data.s].GetComponent<Character>().Data);
        }
        return MainManager.Map.Initialize(characters, treeCells);
    }

    private void WriteToFile(int index, string json)
    {
        string path = GetFilePath(index);
        FileStream fileStream = new FileStream(path, FileMode.Create);

        using StreamWriter writer = new StreamWriter(fileStream);
        writer.Write(json);
    }

    private string ReadFromFile(int index)
    {
        string path = GetFilePath(index);
        if (File.Exists(path))
        {
            using StreamReader streamReader = new StreamReader(path);
            string json = streamReader.ReadToEnd();
            return json;
        }
        else
        {
            _endOfPrefabs = true;
            return "";
        }
    }

    private string GetFilePath(int index)
    {
        return Application.dataPath + "/SaveFolder/" + _fileStart + index.ToString() + _fileEnd;
    }
}
