using UnityEngine;
using System.Collections;

public class particleBurst : MonoBehaviour {
    private bool seen;

	// Use this for initialization
	void Start () {
        GetComponent<ParticleSystem>().SetEmissionRate(0);
	}

	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown( KeyCode.P )){
            float vel = Random.Range(.1f, 1f);
            trigger(24, vel);
        }
        checkSeen();
	}

    public void setTexture(Texture2D texture){
        GetComponent<ParticleSystemRenderer>().material.mainTexture = texture;
    }

    void checkSeen(){
        if(isVisible()){
            seen = true;
        }
    }

    public bool isVisible(){
        return GetComponent<ParticleSystemRenderer>().isVisible;
    }

    public bool wasSeen(){
        return seen;
    }

    public void trigger(int numParticles, float vel){
        //REMOVE texture parameter
        GetComponent<ParticleSystem>().Emit((int)(numParticles * vel));
    }

}
