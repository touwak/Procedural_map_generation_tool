using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCamera2D : MonoBehaviour {

  public float moveSpeed = 40f;

	void Update () {
    float xDelta = Input.GetAxis("Horizontal");
    float yDelta = Input.GetAxis("Vertical");

    if (xDelta != 0 || yDelta != 0) {
      AdjustPosition(xDelta, yDelta);
    }
  }

  public void AdjustPosition(float xDelta, float yDelta) {
    Vector3 direction = transform.localRotation *
      new Vector3(xDelta, yDelta, 0f).normalized;
    float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(yDelta));
    float distance = moveSpeed * damping * Time.deltaTime;

    Vector3 position = transform.localPosition;
    position += direction * distance;

    transform.localPosition = position;   
  }
}
