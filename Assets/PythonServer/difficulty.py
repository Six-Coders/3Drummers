import librosa
import json
from magenta.music import midi_io

# SCORE VARIABLES!
tempo_score_list = [[20,39,10],[40,59,20],[60,79,30],[80,109,40],[110,159,50],[160,199,60],[200,999,70]]
elements_score_dict = {10:[1,2],15:[3,4],20:[5]}
diff_score = {"Very Easy":[100,499], "Easy":[500,999], "Medium":[1000,1749],"Hard": [1750,2499],"Very Hard":[2500,3500]}

def open_files(path_files): #Directorio donde estan todos los archivos de AUDIO
    audio_file = librosa.load(path_files+'/drums.wav')
    midi_file = midi_io.midi_file_to_note_sequence(path_files+"/drums.midi")
    return audio_file,midi_file

def tempo_score(audio_file):
    y, sr = audio_file
    tempo, _ = librosa.beat.beat_track(y=y, sr=sr)
    for min_bpm,max_bpm,score in tempo_score_list:
        if tempo >= min_bpm and tempo <= max_bpm:
            return score,tempo

def elements_score(midi_file):
    elements = set()
    kicks = []
    for note in midi_file.notes:
        if note.pitch == 36:
            kicks.append((note.start_time,note.velocity))
        elements.add(note.pitch)
    for score,values in elements_score_dict.items():
        if len(elements) in values:
            return score,kicks
    return 25,kicks

def double_bass_score(kicks, threshold=0.5, occasional_threshold=4, full_presence_threshold=12):
    # Collect the time intervals between consecutive MIDI events
    time_intervals = []
    prev_time = None

    for kick_time, _ in kicks:
        if prev_time is not None:
            time_intervals.append(kick_time - prev_time)
        prev_time = kick_time

    # Check for consecutive short intervals that would indicate use of double bass
    consecutive_short_intervals = 0
    for interval in time_intervals:
        if interval < threshold:
            consecutive_short_intervals += 1
            if consecutive_short_intervals >= occasional_threshold:
                if consecutive_short_intervals < full_presence_threshold:
                    return 1.5  # Occasional double bass presence detected
                else:
                    return 2 # Full double bass presence detected
        else:
            consecutive_short_intervals = 0
    return 1  # No double bass presence detected

def get_difficulty(files_path):
    audio_file, midi_file = open_files(files_path)
    tempo_sc, tempo = tempo_score(audio_file)
    elements_sc, kicks = elements_score(midi_file)
    double_bass = double_bass_score(kicks)
    print ("Tempo Score: ", tempo_sc, ", Elements Score: ", elements_sc,", Double bass Score: ", double_bass)
    calculated_score = tempo_sc * elements_sc * double_bass

    for diff, intervals in diff_score.items():
        min_interval,max_interval = intervals
        if min_interval <= calculated_score <= max_interval:
            final_diff = diff
            break
    print(calculated_score)        
    # Writing song data on .json
    song_data = {"tempo":round(tempo),"difficulty":final_diff}
    json_object = json.dumps(song_data, indent=4)
    with open(files_path+"/data.json", "w") as outfile:
        outfile.write(json_object)
    return final_diff


