using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

public static class UtilitiesMath
{
    // this modulo will behave more sensical for negative numbers. '%' is defined as a division reminder, whereas the following is defined as modulo
    public static int Modulo(this int self, int moduloVal)
    {
        int r = self%moduloVal;
        return r<0 ? r+moduloVal : r;
    } 
    
}