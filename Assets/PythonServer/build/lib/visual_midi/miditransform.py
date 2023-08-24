from magenta.music import midi_io

def magenta_get_notes(file_path,delay_time):
    sequence = midi_io.midi_file_to_note_sequence(file_path)
    percussion_used = set()
    notes = []
    fix_notes = {40:38, 37:38, 26:46, 42:46, 22:46, 44:46, 50:48, 45:48, 47:48, 43:48, 58:48, 55:49, 57:49, 52:49, 59:51, 53:51}
    drums_items = [51,49,48,46,38,36]
    for note in sequence.notes:
        start_time = note.start_time + delay_time
        pitch = note.pitch
        if pitch not in drums_items:
            if pitch in fix_notes.keys():
                pitch = fix_notes[pitch]
            else:
                pitch = 38
        velocity = note.velocity
        notes.append((start_time, pitch, velocity))
        percussion_used.add(pitch)
    return notes,percussion_used


def set_color_intensity(color,intensity):
    def exponential_mapping(value):
        value = max(0, min(127, value))
        exponent = 0.5
        mapped_value = ((value / 127) ** exponent) * 255
        mapped_value = max(0, min(255, round(mapped_value)))
        return mapped_value
    
    red,green,blue = color
    transformed_red = exponential_mapping(intensity) * (red / 255)
    transformed_green = exponential_mapping(intensity) * (green / 255)
    transformed_blue = exponential_mapping(intensity) * (blue / 255)

    transformed_red = max(0, min(255, round(transformed_red)))
    transformed_green = max(0, min(255, round(transformed_green)))
    transformed_blue = max(0, min(255, round(transformed_blue)))

    transformed_color = (transformed_red, transformed_green, transformed_blue)
    return transformed_color

def check_drums(array):
    instrument_count = {}
    for _,note,velocity in array:
        if velocity <= 50:
            continue
        if note not in instrument_count:
            instrument_count[note] = 1
        else:
            instrument_count[note] += 1
    kick_count = 0
    other_count = 0
    for item in instrument_count.items():
        note,cantidad = item
        if note == 36:
            kick_count = cantidad
        else:
            other_count += cantidad
    
    if kick_count < 10:
        return False
    else:
        resta = kick_count - other_count
        if resta < 40:
            if resta < 0:
                return True
            return False
        else:
            return True