using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health;
    
        internal void TakeDamge(float damageToInflic)
    {
        health -= damageToInflic;
    }
}
