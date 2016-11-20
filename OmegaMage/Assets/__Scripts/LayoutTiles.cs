﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TileTex
{
    // This class enables us to define various textures for tiles
    public string str;
    public Texture2D tex;
}

public class LayoutTiles : MonoBehaviour
{
    static public LayoutTiles S;

    public TextAsset roomsText; // The Rooms.xml file
    public string roomNumber = "0"; // Current room # as a string
    // ^ roomNumber as string allows encoding in the XML & rooms 0-F
    public GameObject tilePrefab; // Prefab for all Tiles
    public TileTex[] tileTextures; // A list of named textures for Tiles

    public bool ________________;

    public PT_XMLReader roomsXMLR;
    public PT_XMLHashList roomsXML;
    public Tile[,] tiles;
    public Transform tileAnchor;

    void Awake()
    {
        S = this; // Set the Singleton for LayoutTiles

        // Make a new GameObject to be the TileAnchor (the parent transform of
        //  all Tiles). This keeps Tiles tidy in the Hierarchy pane.
        GameObject tAnc = new GameObject("TileAnchor");
        tileAnchor = tAnc.transform;

        // Read the XML
        roomsXMLR = new PT_XMLReader(); // Create a PT_XMLReader
        roomsXMLR.Parse(roomsText.text); // Parse the Rooms.xml file
        roomsXML = roomsXMLR.xml["xml"][0]["room"]; // Pull all the <room>s

        // Build the 0th Room
        BuildRoom(roomNumber);
    }

    // This is the GetTileTex() method that Tile uses
    public Texture2D GetTileTex(string tStr)
    {
        // Search through all the tileTextures for the proper string
        foreach (TileTex tTex in tileTextures)
        {
            if (tTex.str == tStr)
            {
                return (tTex.tex);
            }
        }
        // Return null if nothing was found
        return (null);
    }


    // Build a room based on room number. This is an alternative version of
    // BuildRoom that grabs roomXML based on <room> num.
    public void BuildRoom(string rNumStr)
    {
        PT_XMLHashtable roomHT = null;
        for (int i = 0; i < roomsXML.Count; i++)
        {
            PT_XMLHashtable ht = roomsXML[i];
            if (ht.att("num") == rNumStr)
            {
                roomHT = ht;
                break;
            }
        }
        if (roomHT == null)
        {
            Utils.tr("ERROR", "LayoutTiles.BuildRoom()",
                     "Room not found: " + rNumStr);
            return;
        }
        BuildRoom(roomHT);
    }



    // Build a room from an XML <room> entry
    public void BuildRoom(PT_XMLHashtable room)
    {
        // Get the texture names for the floors and walls from <room> attributes
        string floorTexStr = room.att("floor");
        string wallTexStr = room.att("wall");
        // Split the room into rows of tiles based on carriage returns in the Rooms.xml file
        string[] roomRows = room.text.Split('\n');
        // Trim tabs from the beginnings of lines. However, we're leaving spaces
        //  and underscores to allow for non-rectangular rooms.
        for (int i = 0; i < roomRows.Length; i++)
        {
            roomRows[i] = roomRows[i].Trim('\t');
        }
        // Clear the tiles Array
        tiles = new Tile[100, 100]; // Arbitrary max room size is 100x100

        // Declare a number of local fields that we'll use later
        Tile ti;
        string type, rawType, tileTexStr;
        GameObject go;
        int height;
        float maxY = roomRows.Length - 1;

        // These loops scan through each tile of each row of the room
        for (int y = 0; y < roomRows.Length; y++)
        {
            for (int x = 0; x < roomRows[y].Length; x++)
            {
                // Set defaults
                height = 0;
                tileTexStr = floorTexStr;

                // Get the character representing the tile
                type = rawType = roomRows[y][x].ToString();
                switch (rawType)
                {
                    case " ": // empty space
                    case "_": // empty space
                              // Just skip over empty space
                        continue;
                    case ".": // default floor
                              // Keep type="."
                        break;
                    case "|": // default wall
                        height = 1;
                        break;
                    default:
                        // Anything else will be interpreted as floor
                        type = ".";
                        break;
                }

                // Set the texture for floor or wall based on <room> attributes
                if (type == ".")
                {
                    tileTexStr = floorTexStr;
                }
                else if (type == "|")
                {
                    tileTexStr = wallTexStr;
                }

                // Instantiate a new TilePrefab
                go = Instantiate(tilePrefab) as GameObject;
                ti = go.GetComponent<Tile>();
                // Set the parent Transform to tileAnchor
                ti.transform.parent = tileAnchor;
                // Set the position of the tile
                ti.pos = new Vector3(x, maxY - y, 0);
                tiles[x, y] = ti; // Add ti to the tiles 2D Array

                // Set the type, height, and texture of the Tile
                ti.type = type;
                ti.height = height;
                ti.tex = tileTexStr;

                // If the type is still rawType, continue to the next iteration
                if (rawType == type) continue;

                // Check for specific entities in the room
                switch (rawType)
                {                                          // 1
                    case "X": // Starting position for the Mage
                        Mage.S.pos = ti.pos; // Uses the Mage Singleton
                        break;
                }

                // More to come here...
            }
        }
    }
}