# FlyBy
MIDI triggered particles and scrolling sprites, outputs to Syphon for VJing

## Keyboard Shortcuts

| Keyboard Shortcut  |  Behavior  |
|---|---|
| M | Toggles a MIDI display so you can test your MIDI input  |
| Space  | Resets the scene, reloads json and assets from json |
| 1  | Triggers sample scrolling sprite  |
| 2 | Triggers sample scrolling trigger |


##JSON

Sample JSON

```
{
    "nextPartCC" : "51",
    "nextPartChannel" : "1",
    "defaultMidiChannel" : "2",
    "parts":[
        {
            "id" : "0",
            "name" : "sample_circle",
            "objs" : [
                {
                    "keyStroke": "1",
                    "midiNote": "36",
                    "name": "circles/circle3.png",
                    "direction" : "right",
                    "scale" : ".8",
                    "y" : ".2",
                    "scrollX" : "-40",
                }
            ],
            "particles" : [
                {
                    "name" : "circles/circle4.png",
                    "keyStroke" : "2",
                    "type" : "1",
                    "midiNote" : "35",
                    "numParticles" : "50",
                    "numInstances" : "12",
                    "z" : "10"
                },
            ]
        }
    ]
}
```

