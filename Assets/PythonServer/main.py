import os
import sys 

from onsets_frames_transcription import wav_to_midi
from demucs import separate
from visual_midi import midiplayer

def main(argv):
    FILE_PATH = argv[1] #"TEST\OTHERSIDE.mp3"
    FILE_NAME = os.path.basename(FILE_PATH)
    NAME = os.path.splitext(FILE_NAME)[0]

    TEST_PATH = "TEST/"
    TARGET_PATH = "TARGET/"

    FINAL_DIR = TARGET_PATH + NAME
    DRUMS_PATH = FINAL_DIR + "/drums.wav"


    MIDI_PATH = FINAL_DIR + "/drums.midi"

    print("Separando audio ...")
    separate.main(["--out", TARGET_PATH, "--two-stems", "drums", FILE_PATH])
    print("Separacion finalizada con exito!")

    print("Convirtiendo WAV DRUMS a MIDI ...")
    wav_to_midi.main([DRUMS_PATH, FINAL_DIR])
    print("Conversion finalizada con exito!")

    print("Reproduciendo MIDI ...")
    midiplayer.main([MIDI_PATH,DRUMS_PATH])
    print("Reproduccion finalizada con exito!")


if __name__ == "__main__":
    main(sys.argv)





