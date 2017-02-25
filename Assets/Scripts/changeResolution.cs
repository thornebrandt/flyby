using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeResolution : MonoBehaviour {
	public int width;
	public int height;


	// Use this for initialization
	void Start () {
		//SetRenderingResolution(2540, 1080);
		Screen.SetResolution(width, height, true);
	}

	// Update is called once per frame
	void Update () {

	}
}
