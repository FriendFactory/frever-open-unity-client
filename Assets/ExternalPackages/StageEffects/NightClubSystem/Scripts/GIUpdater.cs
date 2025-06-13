using UnityEngine;
using System.Collections;

public class GIUpdater : MonoBehaviour {

	private Renderer _renderer;

	void Start() {
		_renderer = GetComponent<Renderer>();
	}

	// Update is called once per frame
	void Update () {		
		if (_renderer != null) {
			RendererExtensions.UpdateGIMaterials(_renderer);
		}	
	}
}
