using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Package : MonoBehaviour {

    public Image icon;
    public Image background;

    public PackageState state;

    bool initialized = false;

    [Serializable]
    public enum PackageState {
        notice, readyForSelection, awaitingPickup
    }

    [Serializable]
    public struct PackageStateVisual {
        public PackageState state;
        public Color32 color;
        public Sprite icon;
    }

    public PackageStateVisual[] visuals;

    public PackageStateVisual GetVisual(PackageState state) {
        foreach (PackageStateVisual visual in visuals) {
            if (visual.state == state) return visual;
        }
        return visuals[0];
    }

    public void SetState(PackageState state) {
        if (this.state == state && initialized) return;
        this.state = state;
        initialized = true;

        PackageStateVisual visual = GetVisual(state);

        icon.sprite = visual.icon;
        background.color = visual.color;
    }

}