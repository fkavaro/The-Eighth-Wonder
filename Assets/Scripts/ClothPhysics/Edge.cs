using System;
using UnityEngine;

// ARISTA EDD
public struct Edge : IComparable<Edge>
{
    // �ndices de sus extremos (v�rtices)
    public int indexA;
    public int indexB;
    // V�rtice opuesto
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
        // Primero, comparamos los �ndices del primer v�rtice
        int compareFirstIndex = indexA.CompareTo(other.indexA);
        if (compareFirstIndex != 0)
        {
            return compareFirstIndex;
        }

        // Si los �ndices del primer v�rtice son iguales, comparamos los �ndices del segundo v�rtice
        return indexB.CompareTo(other.indexB);
    }
}
