using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class TextHandle {

  public void WriteFile(string file, string text) {

    string path =  "Assets/Resources/" + file + ".txt";
   
    //DateTime thisDay = DateTime.Today;
    //string date = thisDay.ToString("d");
    string line =text;

    //write the line
    StreamWriter writer = new StreamWriter(path, true);
    writer.Write(line);
    writer.Close();

    AssetDatabase.ImportAsset(path);

    AddSeed(line);
  }

  Dictionary<Vector2, int> seeds = new Dictionary<Vector2, int>();

  public void ReadFile(string file) {

    string path = "Assets/Resources/" + file + ".txt";
    StreamReader reader = new StreamReader(path);
    string text = reader.ReadToEnd();
    reader.Close();

    AddSeed(text);    
  }

  private void AddSeed(string text) {
    String pattern = @"\|";
    string[] elements = Regex.Split(text, pattern);

    for (int i = 0; i < elements.Length - 1; i += 3) {
      Vector2 currentPos = new Vector2(float.Parse(elements[i]), float.Parse(elements[i + 1]));
      seeds.Add(currentPos, int.Parse(elements[i + 2]));
    }
  }

  public int FindDungeon(Vector2 position) {

    foreach(KeyValuePair<Vector2, int> seed in seeds) {
      if (seed.Key == position) {
        return seed.Value;
      }
    }

    return -1;
  }

}
