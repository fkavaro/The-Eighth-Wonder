using System;
using UnityEngine;

// ARISTA EDD
public struct Edge : IComparable<Edge>
{
    // Índices de sus extremos (vértices)
    public int indexA;
    public int indexB;
    // Vértice opuesto
    public int indexOther;
    // Tipo de muelle al que corresponde
    public Spring.Type springType;

    // "Constructor"
    public Edge(int _indexA, int _indexB, int _indexOther, Spring.Type _type)
    {
        //A siempre menor que B
        indexA = Mathf.Min(_indexA, _indexB);
        indexB = Mathf.Max(_indexA, _indexB);
        indexOther = _indexOther;
        springType = _type;
    }

    // Para poder comparar dos aristas
    public int CompareTo(Edge other)
    {
        // Primero, comparamos los índices del primer vértice
        int compareFirstIndex = indexA.CompareTo(other.indexA);
        if (compareFirstIndex != 0)
        {
            return compareFirstIndex;
        }

        // Si los índices del primer vértice son iguales, comparamos los índices del segundo vértice
        return indexB.CompareTo(other.indexB);
    }
}
