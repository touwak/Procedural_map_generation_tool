using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour {


  public bool inPlayerInventory = false;

  private Player player;
  private WeaponComponents[] weaponsComps;
  //private bool weaponUsed = false;
  public int attackMod, defenseMod;

  //public void AcquireWeapon() {

  //}

  /// <summary>
  /// Generate the stats and rarity of the weapon
  /// </summary>
  private void Start() {
    int randomLevel = Random.Range(0, 100);

    if (randomLevel >= 0 && randomLevel < 50) {
      attackMod += Random.Range(1, 4);
      defenseMod += Random.Range(1, 4);
    }
    else if (randomLevel >= 50 && randomLevel < 75) {
      attackMod += Random.Range(4, 10);
      defenseMod += Random.Range(4, 10);
    }
    else if (randomLevel >= 75 && randomLevel < 90) {
      attackMod += Random.Range(15, 25);
      defenseMod += Random.Range(15, 25);
    }
    else {
      attackMod += Random.Range(40, 55);
      defenseMod += Random.Range(40, 55);
    }
  }

  // Update is called once per frame
 // void Update () {
		
	//}

 // public void UseWeapon() {

 // }

 // public void EnableSpriteRender(bool isEnabled) {

 // }

 // public Sprite GetComponentImage(int index) {

 //   return null;
  //}
}
