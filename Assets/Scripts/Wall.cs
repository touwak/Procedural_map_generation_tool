using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Wall : MonoBehaviour {
  public Sprite dmgSprite;
  public int hp = 3;
  public GameObject[] foodTiles;
  private SpriteRenderer spriteRenderer;

  void Awake() {
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  /// <summary>
  /// Manage the damage that the wall can afford until destroy it
  /// </summary>
  /// <param name="loss"> Amount of damage </param>
  public void DamageWall(int loss) {

    spriteRenderer.sprite = dmgSprite;
    hp -= loss;

    if (hp <= 0) {
      /*if (Random.Range(0, 5) == 1) {
        GameObject food = foodTiles[Random.Range(0, foodTiles.Length)];
        GameManager.instance.InstanceTile(transform.position, food, transform.parent);
      }*/

      gameObject.SetActive(false);
    }
  }
}
