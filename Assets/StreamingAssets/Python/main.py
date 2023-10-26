import os
import sys 

from onsets_frames_transcription import wav_to_midi
from demucs import separate
from difficulty import get_difficulty

def main(argv):
    print("Pase por aqui")
    SONG_PATH = argv[1]
    LIB_PATH = argv[2]
    CHECKPOINT_PATH = argv[3]

    SONG_NAME = os.path.basename(SONG_PATH)
    song_fix = SONG_NAME.split(".")[0]

    print("LA SONGNAME: "+song_fix)
    
    FINAL_DIR = LIB_PATH + "/"+song_fix

    print("LA FINAL_DIR: "+FINAL_DIR)

    DRUMS_PATH = FINAL_DIR
    
    separate.main(["--out", LIB_PATH, "--two-stems", "drums", SONG_PATH])

    print("DRUM PATH: ",DRUMS_PATH)
    wav_to_midi.main([FINAL_DIR+"/drums.wav", FINAL_DIR, CHECKPOINT_PATH])
    get_difficulty(DRUMS_PATH)
    print("Termine")

if __name__ == "__main__":
    main(sys.argv)

