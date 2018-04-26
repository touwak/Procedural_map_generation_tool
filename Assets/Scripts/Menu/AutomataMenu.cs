using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomataMenu : MonoBehaviour {

  public MapGenerator mapGenerator;

	public void GenerateMap() {
    mapGenerator.GenerateMap();
  }
}
