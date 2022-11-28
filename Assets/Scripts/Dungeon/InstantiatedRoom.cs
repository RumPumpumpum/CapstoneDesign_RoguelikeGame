using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]

public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        // 충돌범위 저장
        roomColliderBounds = boxCollider2D.bounds;
    }

    public void Initialise(GameObject roomGameobject)
    {
        PopulateTilemapMemberVariables(roomGameobject);

        DisableCollisionTilemapRenderer();
    }

    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        grid = roomGameobject.GetComponentInChildren<Grid>();

        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();
    
        foreach(Tilemap tilemap in tilemaps)
        {
            switch (tilemap.gameObject.tag)
            {
                case "groundTilemap":
                    groundTilemap = tilemap;
                    break;
                case "decoration1Tilemap":
                    decoration1Tilemap = tilemap;
                    break;
                case "decoration2Tilemap":
                    decoration2Tilemap = tilemap;
                    break;
                case "frontTilemap":
                    frontTilemap = tilemap;
                    break;
                case "collisionTilemap":
                    collisionTilemap = tilemap;
                    break;
                case "minimapTilemap":
                    minimapTilemap = tilemap;
                    break;
            }
        }
    }


    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }
}