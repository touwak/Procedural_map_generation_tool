using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

  public bool isActive = true;

  private void Start() {
    gameObject.SetActive(isActive);
  }

  public void MapToLoad(int map) {
    switch (map) {
      case 0:
        LoadMap("TileMaps");
        break;
      case 1:
        LoadMap("Automata");
        break;
      case 2:
        LoadMap("Hexagons");
        break;
    }
  }

  public void Open() {
    gameObject.SetActive(true);
    HexMapCamera.Locked = true;
  }

  void LoadMap(string map) {
    HexMapCamera.Locked = false;
    SceneManager.LoadScene(map);
  }

  public void CloseApp() {
    Application.Quit();
  }

}
