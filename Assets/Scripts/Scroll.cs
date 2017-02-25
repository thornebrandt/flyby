using UnityEngine;
using System.Collections;

public class Scroll : MonoBehaviour {

    public float xSpeed = -.1f;
    public float ySpeed = 0;
    public float zSpeed = 0;//for 3d crap;
    public float spinSpeed = 0;
    private bool seen = false;
    private Vector3 holdingStation = new Vector3(-999, -999, -999);


    void Update(){
        checkVisible();
        scroll();
    }

    void scroll(){
        if(seen){
            transform.Translate(new Vector3(xSpeed, ySpeed, zSpeed) * Time.deltaTime, Space.World);
            transform.Rotate(0, 0, Time.deltaTime * spinSpeed, Space.Self);
        }
    }


    void checkVisible(){

        if(GetComponent<Renderer>()){
            if(GetComponent<Renderer>().isVisible){
                seen = true;
            }

            if(seen && !GetComponent<Renderer>().isVisible){
                hide();
            }
        } else {
            //meant for  to empty objects with ONLY ONE CHILD!
            foreach (Transform child in transform){
                if(child.gameObject.GetComponent<Renderer>().isVisible){
                    seen = true;
                }
            }

            foreach (Transform child in transform){
                if(seen && !child.GetComponent<Renderer>().isVisible){
                    hide();
                }
            }
        }
    }

    void hide(){
        transform.position = holdingStation;
        seen = false;
    }
}
