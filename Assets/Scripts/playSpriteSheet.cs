using UnityEngine;
using System.Collections;

public class playSpriteSheet : MonoBehaviour
{

public int colCount =  4;
public int rowCount =  4;
public int totalCells = 4;
public int stops = 3;
public bool loop = false;
public bool autoPlay = false;
public bool acceptBass;
public bool acceptKick;
public bool acceptSnare;
public float lowPosition;
public float highPosition;
public string inputKey = "";
private int  rowNumber  =  0;
private int colNumber = 0;
private Vector2 offset;
private int index;
private bool playing = false;

    void Update () {
        SetSpriteAnimation(colCount,rowCount,rowNumber,colNumber,totalCells,stops);
    }

    void SetSpriteAnimation(int colCount ,int rowCount ,int rowNumber ,int colNumber,int totalCells,int stops){
        checkTrigger();
        if(autoPlay){
            playing = true;
        }

        if(!loop && index >= (totalCells - 1)){
            playing = false;
            index = 0;
        }

        if(playing && Time.frameCount % stops == 0){
            index = getNextInLoop(index, totalCells);
        }
        float sizeX = 1.0f / colCount;
        float sizeY = 1.0f / rowCount;
        Vector2 size =  new Vector2(sizeX,sizeY);
        var uIndex = index % colCount;
        var vIndex = index / colCount;
        float offsetX = (uIndex+colNumber) * size.x;
        float offsetY = (1.0f - size.y) - (vIndex + rowNumber) * size.y;
        Vector2 offset = new Vector2(offsetX,offsetY);
        GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", offset);
        GetComponent<Renderer>().material.SetTextureScale  ("_MainTex", size);
    }


    void checkTrigger(){
        if(inputKey != ""){
            if(Input.GetKeyDown(inputKey)){
                trigger();
            }
        }
    }

    public void trigger(){
        playing = true;
        index = 0;
    }


    int getNextInLoop(int i, int max){
        int next = i;
        next++;
        if(next >= max){
            next = 0;
        }
        return next;
    }


}