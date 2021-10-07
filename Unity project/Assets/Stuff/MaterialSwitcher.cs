using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour {
    public SkinnedMeshRenderer LeftHand;
    public SkinnedMeshRenderer RightHand;

    public List<Material> Materials = new List<Material>();
    private int index = 0;

    void Start() {
        int defaultindex = Materials.Count - 1;

        LeftHand.material = Materials[defaultindex];
        RightHand.material = Materials[defaultindex];
    }

    public void ChangeHandSkin(int newIndex)
    {
        LeftHand.material = Materials[newIndex];
        RightHand.material = Materials[newIndex];
    }

    void Update() {
        if(Input.GetKeyUp(KeyCode.LeftArrow)) {
            index = (index + Materials.Count - 1) % Materials.Count;
            LeftHand.material = Materials[index];
            RightHand.material = Materials[index];
        } else if(Input.GetKeyUp(KeyCode.RightArrow)) {
            index = (index + 1) % Materials.Count;
            LeftHand.material = Materials[index];
            RightHand.material = Materials[index];
        }
        
    }
}
