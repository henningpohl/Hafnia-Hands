using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=ExNRbEjVT-8
[ExecuteAlways]
public class InvertScaleOfParent : MonoBehaviour {
    void Update() {
        if(transform.parent.parent.hasChanged) {
            transform.localScale = new Vector3(
                transform.parent.parent.localScale.x == 0 ? 1f : 1f / transform.parent.parent.localScale.x,
                transform.parent.parent.localScale.y == 0 ? 1f : 1f / transform.parent.parent.localScale.y,
                transform.parent.parent.localScale.z == 0 ? 1f : 1f / transform.parent.parent.localScale.z
            );
        }
    }
}
