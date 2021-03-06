﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Planet")]
public class PlanetType : ScriptableObject
{
    public int id;

    public string type;
    public string description;

    public bool canHaveAtmosphere;
    public bool canHaveMoons;
    public bool canSupportLife;
    public bool field; //Eg. asteroid field, gas cloud.

    public int visSize;
    public Color visColor;
}
