using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;
public class ExampleListSort : MonoBehaviour
{
    LinkedList<string> linkedListTest = new LinkedList<string>();
    void Start()
    {
        //nodo incial
        linkedListTest.AddFirst("start position");
        linkedListTest.AddLast("second position");
        linkedListTest.AddLast("third position");
        linkedListTest.AddLast("last position");

        Debug.Log("List:");
        foreach(String s in linkedListTest)
        {
            Debug.Log(s);
        }
        //referencia del nodo inicial
        LinkedListNode<string> currentNode = linkedListTest.First;
        Debug.Log("current node value: "+currentNode.Value.ToString());
        Debug.Log("next node value: "+currentNode.Next.Value.ToString());

        //define si tenemos un elemento en la lista
        Debug.Log("la lista contiene el nodo"+linkedListTest.Contains(currentNode.Value));

        //define si tenemos un elemento en la lista
        Debug.Log("la lista contiene el nodo" + linkedListTest.Contains("Fourth position"));

        linkedListTest.RemoveLast();
        foreach(String s in linkedListTest)
        {
            Debug.Log(s);
        }

        //cambiando el valor del current node conb el valor que encontro
        currentNode = linkedListTest.Find("third Position");

        //agregamos al siguiente nodo el valor asignado
        linkedListTest.AddAfter(currentNode, "fourth Position");

        Debug.Log("New List");
        foreach(String s in linkedListTest)
        {
            Debug.Log(s);
        }

        currentNode = linkedListTest.Find("second position");

        linkedListTest.AddBefore(currentNode, "first position");

        Debug.Log("New List 2:");
        foreach (String s in linkedListTest)
        {
            Debug.Log(s);
        }
        //linkedListTest.Clear();
        linkedListTest.Remove(currentNode);
        Debug.Log("New List 3:");
        foreach (String s in linkedListTest)
        {
            Debug.Log(s);
        }

        currentNode = linkedListTest.First;

       // Debug.Log("Preview Node: " + currentNode.Previous.Value + "next node:" + currentNode.Next.Value);
        Debug.Log("next node:" + currentNode.Next.Value);

        linkedListTest.AddAfter(currentNode, "first position");
        Debug.Log("New List 4:");
        foreach (String s in linkedListTest)
        {
            Debug.Log(s);
        }

        currentNode = linkedListTest.Find("first position");
        Debug.Log("preview node: "+ currentNode.Previous.Value+ "next node: "+ currentNode.Next.Value);

        currentNode= linkedListTest.FindLast("first position");
        Debug.Log("preview node: "+ currentNode.Previous.Value+ "next node: "+ currentNode.Next.Value);


    }
}
