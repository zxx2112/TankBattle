using Destructible2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainStampGenerator : MonoBehaviour
{
    public Vector2 Size = Vector2.one;
    public float Angle;
    public Texture2D Shape;
    public Color Color = Color.white;
    public LayerMask Layers = -1;

    private void Awake() {


    }

    public void GenerateStamp(Vector2 worldPosition) {
        D2dStamp.All(D2dDestructible.PaintType.Cut, worldPosition, Size, Angle, Shape, Color, Layers);
    }

}
