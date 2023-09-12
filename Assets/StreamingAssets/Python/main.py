import os
import sys 

from onsets_frames_transcription import wav_to_midi
from demucs import separate
from difficulty import get_difficulty

def main(argv):
    SONG_PATH = argv[1]
    LIB_PATH = argv[2]
    CHECKPOINT_PATH = argv[3]
    SONG_NAME = os.path.basename(SONG_PATH)
    song_fix = SONG_NAME.split(".")[0]
    FINAL_DIR = LIB_PATH + "/"+song_fix
    DRUMS_PATH = FINAL_DIR
    separate.main(["--out", LIB_PATH, "--two-stems", "drums", SONG_PATH])
    wav_to_midi.main([FINAL_DIR+"/drums.wav", FINAL_DIR, CHECKPOINT_PATH])
    get_difficulty(DRUMS_PATH)
    exit()

if __name__ == "__main__":
    main(sys.argv)

