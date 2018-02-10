using UnityEditor;
using UnityEngine;
using System.IO;

public class TextHandle {

  public void WriteFile(string file, string text) {

    string path =  "Assets/Resources/" + file + ".txt";
    string line = text;

    //write the line
    StreamWriter writer = new StreamWriter(path, true);
    writer.WriteLine(line);
    writer.Close();

    AssetDatabase.ImportAsset(path);
    TextAsset asset = Resources.Load(file) as TextAsset;

    Debug.Log(asset.text);
  }

  public string ReadFile(string file) {

    string path = "Assets/Resources/" + file + ".txt";
    StreamReader reader = new StreamReader(path);
    string text = reader.ReadToEnd();
    reader.Close();
    char separator = text[0];
    //read the first character, that will be the separator and send it


    return text;
  }

  //apportion rthe text in levels and dungeons

}
