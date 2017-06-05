using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Newtonsoft.Json;
using MidiJack;
//a Part is a section of a song
//a Set is a shared of objs that share the same input


public class scrollController : MonoBehaviour {
    public string jsonName;
    public GameObject camObj;
    public GameObject particles;
    public GameObject quadPrefab;
    private Dictionary<string, Texture2D> textureLibrary;
    private Dictionary<string, Set> sets;
    private bool JSONParsed = false;
    public RootSongObject rootSongObject;
    private Vector3 holdingStation = new Vector3(-999, -999, -999);
    private Vector3 bottomLeft;
    private Vector3 topRight;
    [HideInInspector]
    public Rect cameraRect;
    private float cameraMargin;
    private float cameraOffsetY;
    private float cameraOffsetX;
    private float cameraOffsetZ;
    private int currentPart = 0;
    private int numParts;
    private float z;
    private MidiChannel[] channels;


    void Start () {
        setupMidiChannels();
        textureLibrary = new Dictionary<string, Texture2D>();
        sets = new Dictionary<string, Set>();
        getCameraRect();
        getJSON();
    }

    void setupMidiChannels(){
        channels = new MidiChannel[17];
        channels[0] = MidiChannel.Ch1;
        channels[1] = MidiChannel.Ch2;
        channels[2] = MidiChannel.Ch3;
        channels[3] = MidiChannel.Ch4;
        channels[4] = MidiChannel.Ch5;
        channels[5] = MidiChannel.Ch6;
        channels[6] = MidiChannel.Ch7;
        channels[7] = MidiChannel.Ch8;
        channels[8] = MidiChannel.Ch9;
        channels[9] = MidiChannel.Ch10;
        channels[10] = MidiChannel.Ch11;
        channels[11] = MidiChannel.Ch12;
        channels[12] = MidiChannel.Ch13;
        channels[13] = MidiChannel.Ch14;
        channels[14] = MidiChannel.Ch15;
        channels[15] = MidiChannel.Ch16;
        channels[16] = MidiChannel.All;
    }

    void Update () {
        if(JSONParsed){
            checkInput();
        }
        //testMidiNotes();
    }

    void testMidiNotes(){
        for(int i = 0; i < 127; i++){
            if(MidiMaster.GetKeyDown(i)){
                print("midi note down: " + i);
            }
        }
    }


    void getJSON(){
        StreamReader sr = new StreamReader("Assets/json/"+jsonName+".json");
        string line = "";
        string json = "";
        int i = 0;
        while((line = sr.ReadLine())!=null){
            json+= line;
            i++;
        }
        rootSongObject = JsonConvert.DeserializeObject<RootSongObject>(json);
        JSONParsed = parseJSON();
    }

    void deleteExistingQuads(){
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Respawn");
        foreach(GameObject g in objs){
            Destroy(g);
        }
    }

    bool parseJSON(){
        numParts = rootSongObject.parts.Count;
        foreach(Part songPart in rootSongObject.parts){
            parseObjs(songPart);
            parseParticles(songPart);
        }
        return true;
    }

    void parseParticles(Part songPart){
        if(songPart.particles != null){
            foreach(Particle p in songPart.particles){
                //GameObject particlesClone = (GameObject).Instantiate(particles);
                p.particleObjects = new List<GameObject>();
                parseParticleSet(p);
                for(int i = 0; i < p.numInstances; i++){
                    GameObject particleObject = createParticleObject(p);
                    p.particleObjects.Add(particleObject);
                }
            }
        }
    }

    void parseObjs(Part songPart){
        foreach(Obj o in songPart.objs){
            //TODO: If type = sprit
            o.gameObjects = new List<GameObject>();
            parseSet(o);
            for(int i = 0; i < o.numInstances; i++){
                GameObject quad = createQuadObject(o);
                o.gameObjects.Add(quad);
            }
            o.i = 0;
        }
    }


    GameObject createParticleObject(Particle p){
        //TODO - address different types of particles
        GameObject particlesClone;
        switch(p.type){
            case 0:
            case 1:
            case 2:
            default:
                particlesClone = Instantiate(particles) as GameObject;
                break;
        }

        if(p.rotation == "gravity"){
            ParticleSystem particleSystem =  particlesClone.GetComponent<ParticleSystem>();
            ParticleSystemRenderer pr = (ParticleSystemRenderer)particleSystem.GetComponent<Renderer>();
            pr.renderMode = ParticleSystemRenderMode.Stretch;
        }

        Texture2D texture = LoadAsset(p.name) as Texture2D;
        particlesClone.GetComponent<particleBurst>().setTexture(texture);
        particlesClone.transform.parent = transform;
        return particlesClone;
    }

    void randomizeLocation(GameObject obj){
        //written for particles,  larger than screen
        float frameScale = .7f;
        float randomYOffset = Random.Range(-cameraRect.height, cameraRect.height);
        float randomXOffset = Random.Range(-cameraRect.width, cameraRect.width);
        Vector3 offset = new Vector3(randomXOffset * frameScale, randomYOffset * frameScale, 0);
        obj.transform.position = offset;
    }

    GameObject createQuadObject(Obj o){
        string name = o.name;
        GameObject quadObject = new GameObject("QuadObject");
        quadObject.transform.parent = transform;
        GameObject quad = Instantiate(quadPrefab) as GameObject;
        quad.transform.parent = quadObject.transform;
        quadObject.AddComponent<Scroll>();
        quadObject.tag = "Respawn";
        Texture2D texture = LoadAsset(name) as Texture2D;
        float screenScale = (Screen.height / 2.0f) / Camera.main.orthographicSize;
        Vector3 newScale = quad.transform.localScale;
        newScale.x = texture.width / screenScale;
        newScale.y = texture.height / screenScale;
        //check for sprites
        if(o.spriteRows > 0 && o.spriteCols > 0 && o.numFrames > 0){
            quad.AddComponent<playSpriteSheet>();
            playSpriteSheet spriteSheet = quad.GetComponent<playSpriteSheet>();
            spriteSheet.colCount = o.spriteCols;
            spriteSheet.rowCount = o.spriteRows;
            spriteSheet.totalCells = o.numFrames;
            spriteSheet.stops = o.stops;
            spriteSheet.loop = o.loop;
            newScale.x = newScale.x * .5f;
        }

        quad.transform.localScale = newScale;
        quadObject.transform.position = holdingStation;
        quad.GetComponent<Renderer>().material.mainTexture = texture;
        //quad.GetComponent<Renderer>().material.shader = Shader.Find("Legacy Shaders/Diffuse"); //UNCOMMENT FOR REAL TEXTURE
        return quad;
    }

    void parseParticleSet(Particle p){
        //TODO - refactor the two parse Sets int one function;
        if(p.set != null){
            string setName = p.set;
            Set set;
            if(!sets.TryGetValue(setName, out set)){
                set = new Set();
                set.length = 1;
                set.i = 0;
                sets.Add(setName, set);
            } else {
                set.length++;
            }
            int setIndex = sets[setName].length - 1;
            p.setIndex = setIndex;
        }
    }


    void parseSet(Obj o){
        if(o.set != null){
            string setName = o.set;
            Set set;
            if(!sets.TryGetValue(setName, out set)){
                set = new Set();
                set.length = 1;
                set.i = 0;
                sets.Add(setName, set);
            } else {
                set.length++;
            }
            int setIndex = sets[setName].length - 1;
            o.setIndex = setIndex;
        }
    }


    Texture2D LoadAsset(string name){
        Texture2D texture;
        if(!textureLibrary.TryGetValue(name, out texture)){
            texture = LoadPNG(name);
            textureLibrary.Add(name, texture);
        }
        return texture;
    }


    Texture2D LoadPNG(string filePath){
        filePath = "Assets/Resources/" + filePath;
        Texture2D tex = null;
        byte[] fileData;
        if(File.Exists(filePath)){
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }


    void getCameraRect(){
        Camera cam = camObj.GetComponent<Camera>();
        bottomLeft = cam.ScreenToWorldPoint(Vector3.zero);
        topRight = cam.ScreenToWorldPoint(new Vector3(
            cam.pixelWidth, cam.pixelHeight));
        cameraRect = new Rect(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x - bottomLeft.x,
            topRight.y - bottomLeft.y
        );
        cameraOffsetX = camObj.transform.position.x;
        cameraOffsetY = camObj.transform.position.y;
        cameraOffsetZ = camObj.transform.position.z;
        cameraMargin = cameraRect.width * .5f;
    }

    Bounds getParentSize(GameObject parent){
        Vector3 center = Vector3.zero;
        foreach (Transform child in parent.transform)
        {
            center += child.gameObject.GetComponent<Renderer>().bounds.center;
        }
        center /= parent.transform.childCount; //center is average center of children
        //Now you have a center, calculate the bounds by creating a zero sized 'Bounds',
        Bounds bounds = new Bounds(center,Vector3.zero);
        foreach (Transform child in parent.transform)
        {
            bounds.Encapsulate(child.gameObject.GetComponent<Renderer>().bounds);
        }
        return bounds;
    }

    void checkInput(){
        //TODO -- check for doubles and then do them as random -

        if(Input.GetKeyDown("space")){
            restartLevel();
        }

        if(Input.GetKeyDown("right")){
            getNextPart();
        }

        if(Input.GetKeyDown("left")){
            getPreviousPart();
        }


        foreach(Part songPart in rootSongObject.parts){
            if(currentPart == songPart.id){
                checkObjInput(songPart);
                checkParticleInput(songPart);
            }
        }
    }

    void checkObjInput(Part songPart){
        bool updatedSet = false;
        Set set = null;
        foreach(Obj o in songPart.objs){
            float vel;
            if(Input.GetKeyDown(o.keyStroke) || MidiMaster.GetKeyDown(channels[o.midiChannel], o.midiNote)){
                if( MidiMaster.GetKeyDown(channels[o.midiChannel], o.midiNote) && o.touch){
                    vel = MidiMaster.GetKey(o.midiNote);
                } else {
                    vel = Random.Range(.1f, 1f);
                }
                if(o.set == null){
                    popupQuad(o, vel);
                } else {
                    int currentSetIndex = sets[o.set].i;
                    if(o.setIndex == currentSetIndex){
                        updatedSet = true;
                        set = sets[o.set];
                        popupQuad(o, vel);
                    }
                }
            }
        }
        if(updatedSet){
            set.i++;
            set.i = (int)Mathf.Repeat(set.i, set.length);
        }
    }

    void checkParticleInput(Part songPart){
        bool updatedSet = false;
        Set set = null;
        if(songPart.particles != null){
            foreach(Particle p in songPart.particles){
                float vel;
                if(Input.GetKeyDown(p.keyStroke) || MidiMaster.GetKeyDown(channels[p.midiChannel], p.midiNote)){
                    if( MidiMaster.GetKeyDown(channels[p.midiChannel], p.midiNote) && p.touch){
                        vel = MidiMaster.GetKey(p.midiNote);
                    } else {
                        vel = Random.Range(.1f, 1f);
                    }
                    if(p.set == null){
                        emitParticle(p, vel);
                    } else {
                        int currentSetIndex = sets[p.set].i;
                        if(p.setIndex == currentSetIndex){
                            updatedSet = true;
                            set = sets[p.set];
                            emitParticle(p, vel);
                        }
                    }
                }
            }
        }
        if(updatedSet){
            set.i++;
            set.i = (int)Mathf.Repeat(set.i, set.length);
        }
    }


    void restartLevel(){
        Application.LoadLevel(0);
    }

    void getNextPart(){
        currentPart++;
        currentPart = (int)Mathf.Repeat(currentPart, numParts);
    }

    void getPreviousPart(){
        currentPart--;
        currentPart = (int)Mathf.Repeat(currentPart, numParts);
    }


    public void emitParticle(Particle p, float vel){
        int index = (int)Mathf.Repeat(p.i, p.numInstances);
        GameObject particleObject = p.particleObjects[index];
        particleBurst pScript = particleObject.GetComponent<particleBurst>();

            //randomizeLocation(p.particlesObj);
        float xPos = cameraOffsetX;
        float yPos = cameraOffsetY;
        float zPos = cameraOffsetZ;

        if(p.x != 0){
            xPos = cameraOffsetX + p.x;
        } else {
            xPos = Random.Range(-cameraRect.width + cameraMargin, cameraRect.width - cameraMargin);
        }

        if(p.y != 0){
            yPos = cameraOffsetY + p.y;
        } else {
            yPos = Random.Range(-cameraRect.height + cameraMargin, cameraRect.height - (cameraMargin * .3f));
        }

        if(p.z != 0){
            zPos = cameraOffsetZ + p.z;
        }

        particleObject.transform.position = new Vector3(xPos, yPos, zPos);
        pScript.trigger(p.numParticles, vel);
        p.i++;
    }

    void resetObj(GameObject obj){
        obj.transform.rotation = Quaternion.identity; //reset rotation
        obj.transform.localPosition = Vector3.zero;
    }

    public void popupQuad(Obj o, float vel){
        int index = (int)Mathf.Repeat(o.i, o.numInstances);
        GameObject obj = o.gameObjects[index];
        resetObj(obj);
        GameObject parent = obj.transform.parent.gameObject;
        float objWidth = obj.GetComponent<Renderer>().bounds.size.x;
        float objHeight = obj.GetComponent<Renderer>().bounds.size.y;
        float newHeight = cameraRect.height;

        if(objWidth > 0 && objHeight > 0){
            float newWidth = objWidth*(newHeight/objHeight);
            //scale
            Vector3 scale;
            float velScale = .5f;
            if(o.velScale){
                velScale = vel;
            }
            scale = new Vector3(newWidth * velScale * o.scale, newHeight * velScale * o.scale, 1);

            obj.transform.localScale = scale;

            //rotation
            float rotateRandom = 0;
            if(o.rotateRandom != 0){
                rotateRandom = Random.Range(-o.rotateRandom, o.rotateRandom);
            }
            obj.transform.Rotate(new Vector3(0, 0, o.rotateX + rotateRandom));

            //scrolling
            Scroll scrollScript = (Scroll) parent.GetComponent(typeof(Scroll));

            float velSpeed = .5f;
            if(o.velSpeed){
                velSpeed = vel;
            }
            scrollScript.xSpeed = o.scrollX * velSpeed;
            scrollScript.ySpeed = o.scrollY * velSpeed;
            triggerAnimation(obj);

            switch(o.direction){
                case "down":
                    flyFromTop(obj, o, vel);
                    break;
                case "top":
                    popupFromTop(obj, vel);
                    break;
                case "bottom":
                    popupFromBottom(obj, vel);
                    break;
                case "right":
                    popupFromRight(obj, o, vel);
                    break;
                case "center":
                    popupFromCenter(obj, o, vel);
                    break;
                default:
                    popupFromBottom(obj, vel);
                    break;
            }
            o.i++;
        }
    }

    void triggerAnimation(GameObject obj){
        if(obj.GetComponent<playSpriteSheet>()){
            obj.GetComponent<playSpriteSheet>().trigger();
        }
    }

    void flyFromTop(GameObject obj, Obj o, float vel){
        //TODO - put particle direction in here
        GameObject parent = obj.transform.parent.gameObject;
        Scroll scrollScript = (Scroll) parent.GetComponent(typeof(Scroll));
        scrollScript.xSpeed = o.scrollX * vel;
        scrollScript.ySpeed = Mathf.Min(o.scrollY * vel, -10f);
        if(o.spinSpeed != 0){
            if(o.rotateRandom != 0){
                scrollScript.spinSpeed = Random.Range(-o.spinSpeed, o.spinSpeed);
            }else{
                scrollScript.spinSpeed = o.spinSpeed;
            }
        }
        Vector3 velScale = obj.GetComponent<Renderer>().bounds.size;
        float randomXOffset = Random.Range(-cameraRect.width/3, cameraRect.width/3);
        float xPos = randomXOffset + cameraOffsetX;
        float yPos = cameraOffsetY + (cameraRect.height/2) + velScale.y/2;
        float zPos = 100f-(99f*vel) + cameraOffsetZ;
        Vector3 origin = new Vector3(xPos, yPos, zPos);
        parent.transform.position = origin;
    }


    void popupFromTop(GameObject obj, float vel){
        GameObject parent = obj.transform.parent.gameObject;
        obj.transform.Rotate(new Vector3(0, 0, 180)); //flips
        Vector3 velScale = obj.GetComponent<Renderer>().bounds.size;
        Vector3 flipScale = obj.transform.localScale;
        flipScale.x *= -1;
        obj.transform.localScale = flipScale;
        float randomXOffset = Random.Range(0, cameraRect.width/2f);
        float xPos = randomXOffset + cameraOffsetX;
        float yPos = cameraOffsetY + (cameraRect.height/2) + velScale.y/2;
        float zPos = 100f-(99f*vel) + cameraOffsetZ;
        float randomOffset = Random.Range(.4f, 1f);
        float destinationY = yPos - (velScale.y * randomOffset);
        Vector3 origin = new Vector3(xPos, yPos, zPos);
        parent.transform.position = origin;
        parent.transform.DOMoveY(destinationY, .2f);
    }

    void popupFromCenter(GameObject obj, Obj o, float vel){
        GameObject parent = obj.transform.parent.gameObject;
        Vector3 velScale = obj.GetComponent<Renderer>().bounds.size;
        float randomXOffset = Random.Range(-2f, 5f);
        float randomYOffset = Random.Range(-5f, 5f);
        float xPos;
        float yPos;
        float zPos;
        if(o.y != 0){
            yPos = o.y + cameraOffsetY;
        } else {
            yPos = randomYOffset + cameraOffsetY;
        }
        if(o.x != 0){
            xPos = -cameraRect.width/2f + (o.x * cameraRect.width) + cameraOffsetX;
        } else {
            xPos = randomXOffset + cameraOffsetX;
        }
        if(o.z != 0){
            zPos = o.z + cameraOffsetZ;
        } else {
            zPos = 100f-(99f*vel) + cameraOffsetZ;
        }
        Vector3 origin = new Vector3(xPos, yPos, zPos);
        parent.transform.position = origin;

        if(o.growSpeed != 0){
            obj.transform.localScale = new Vector3(0, 0, 0);
            obj.transform.DOScale(velScale, o.growSpeed);
        }
        //obj.transform.DOScale(velScale, .4f).SetEase(Ease.OutBounce);
    }


    void popupFromBottom(GameObject obj, float vel){
        GameObject parent = obj.transform.parent.gameObject;
        Vector3 velScale = obj.GetComponent<Renderer>().bounds.size;
        float zPos = 100f-(99f*vel) + cameraOffsetZ;
        float randomXOffset = Random.Range(0, cameraRect.width/2f);
        float xPos = randomXOffset + cameraOffsetX;
        float yPos = cameraOffsetY - velScale.y/2 - cameraRect.height/2 - (cameraMargin * .2f);
        Vector3 origin = new Vector3(xPos, yPos, zPos);
        //Vector3 origin = new Vector3(0, 0, 0); //testing to see something is happening....?
        parent.transform.position = origin;
        float randomOffset = Random.Range(.6f, 1f);
        float destinationY = yPos + (velScale.y * randomOffset);
        parent.transform.DOMoveY(destinationY, .2f);
        //parent.transform.position = origin;
    }

    void popupFromRight(GameObject obj, Obj o, float vel){
        //meant for objects scrolling left
        GameObject parent = obj.transform.parent.gameObject;
        Vector3 velScale = obj.GetComponent<Renderer>().bounds.size;
        float randomYOffset = Random.Range(-4f, 4f);
        float margin = Mathf.Clamp((velScale.x/8f), .01f, cameraMargin);
        float xPos = cameraRect.x + cameraRect.width + (velScale.x/2f) - margin;
        float yPos;
        float zPos;
        if(o.y != 0){
            yPos = o.y + cameraOffsetY;
        } else {
            yPos = cameraOffsetY + randomYOffset;
        }
        if(o.z != 0){
            zPos = o.z + cameraOffsetZ;
        } else {
            zPos = 100f-(99f*vel) + cameraOffsetZ;
        }
        Vector3 origin = new Vector3(xPos, yPos, zPos);
        parent.transform.position = origin;
    }
}


public class Obj{
    public int i { get; set; }
    public int numInstances { get; set; }
    public string name { get; set; }
    public string ext { get; set; }
    public string direction { get; set; }
    public string gameObj { get; set; }
    public string keyStroke { get; set; }
    public int midiNote { get; set; }
    public int midiChannel { get; set; }
    public float scrollX { get; set; }
    public float scrollY { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public float scale { get; set; }
    public bool velScale { get; set; }
    public bool velSpeed { get; set; }
    public float growSpeed { get; set; }
    public float spinSpeed { get; set; }
    public float rotateX { get; set; }
    public float rotateRandom { get; set; }
    public bool loop { get; set; }
    public int spriteRows { get; set; }
    public int spriteCols { get; set; }
    public int numFrames { get; set; } //num frames for animation
    public string set { get; set; } //multiple functions for one input.
    public int setIndex { get; set; } //corresponds to updating set order
    public int stops { get; set; } //speed of animation
    public bool touch { get; set; } //touch sensitive
    public List<GameObject> gameObjects { get; set; }
    public Obj(){
        this.numInstances = 30;
        this.ext = ".png";
        this.scale = 1.5f;
        this.velScale = true;
        this.velSpeed = true;
        this.loop = true;
        this.rotateX = 0f;
        this.rotateRandom = 0f;
        this.scrollX = -4f;
        this.scrollY = 0f;
        this.midiChannel = 2;
        this.stops = 2;
        this.keyStroke = "1";
        this.touch = false;
    }
}

public class Particle{
    public int i { get; set; }
    public int numInstances { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public string name { get; set; }
    public string keyStroke { get; set; }
    public string rotation { get; set; }
    public int type { get; set; }
    public int midiNote { get; set; }
    public int midiChannel { get; set; }
    public int numParticles { get; set; }
    public string set { get; set; }
    public int setIndex { get; set; }
    public bool touch { get; set; }
    public List<GameObject> particleObjects { get; set; }
    public Particle(){
        this.type = 0;
        this.numInstances = 3;
        this.midiChannel = 2;
        this.numParticles = 24;
        this.keyStroke = "";
    }
}

public class RootSongObject{
    public List<Part> parts{get; set;}
    public int nextPartCC {get; set;}
    public int nextPartChannel { get; set; }
    public int noVelocityChannel { get; set; }
    public int defaultMidiChannel { get; set; }
    public RootSongObject(){
        this.nextPartCC = 51;
        this.nextPartChannel = 1;
    }
}

public class Part{
    public List<Obj> objs { get; set; }
    public List<Particle> particles {get; set;}
    public string name { get; set; }
    public int id { get; set; }
    public Part(){
        this.name = "";
        this.id = -1;
    }
}

public class Set{
    public int length{ get; set;}
    public int i { get; set;}
}



