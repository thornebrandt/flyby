using UnityEngine;
using System.Collections;
using MidiJack;

public class MIDIDisplay : MonoBehaviour
{
	private string s;
	private MidiChannel[] channels;
	private bool display = false;
	void Start(){
		setupMidiChannels();
		s = "LISTENING FOR MIDI";
	}

	void Update(){
		testMidiNotes();
		checkInput();
	}

	void checkInput(){
		if(Input.GetKeyDown("m")){
			display = !display;
		}
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


	void testMidiNotes(){
	    for(int i = 0; i < 127; i++){
	        if(MidiMaster.GetKeyDown(i)){
	        	for(int j = 0; j < 16; j++){
	        		if(MidiMaster.GetKeyDown(channels[j], i)){
	        			s = "CHANNEL: " + j + " - NOTE: " + i;
	        		}
	        	}
	        }
	    }
	}

	void OnGUI(){
		if(display){
			int w = Screen.width, h = Screen.height;
			GUIStyle style = new GUIStyle();
			Rect rect = new Rect(0, 0, w, h * 2 / 30);
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = h * 2 / 30;
			style.normal.textColor = new Color (0.0f, 1.0f, 0.5f, 1.0f);
			string text = s;
			GUI.Label(rect, text, style);
		}
	}
}