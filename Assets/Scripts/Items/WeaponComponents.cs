using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponComponents : MonoBehaviour {

  public Sprite[] modules;

  private Weapon parent;
  private SpriteRenderer spriteRenderer;


	void Start () {
    parent = GetComponent<Weapon>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = modules[Random.Range(0, modules.Length)];
	}
	
	void Update () {
    transform.eulerAngles = parent.transform.eulerAngles;
	}

  public SpriteRenderer GetSpriteRender() {
    return spriteRenderer;
  }
}
