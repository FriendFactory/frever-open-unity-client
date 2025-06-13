using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NulcleusDNAEffect : MonoBehaviour {
    public int DNACount = 25;
    public GameObject DNAObject;


    GameObject[] DNAs;
	// Use this for initialization
	void Start () {
        DNAs = new GameObject[DNACount];

        for(int i=0;i<DNACount;i++)
        {
            Vector3 pos = new Vector3(Random.Range(-300, 300), Random.Range(25, 300), Random.Range(-300, 300));
            GameObject go = Instantiate(DNAObject);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f);

            DNAs[i] = go;
        }
	}
}
