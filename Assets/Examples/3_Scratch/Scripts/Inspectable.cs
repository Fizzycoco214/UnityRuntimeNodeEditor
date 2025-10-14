using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class Inspectable : Attribute
{
    public string name;

    //Any additional info can be passed in through this array
    public object[] modifiers;

    public Inspectable(string name, params object[] modifiers)
    {
        this.name = name;
        this.modifiers = modifiers;
    }
}
 