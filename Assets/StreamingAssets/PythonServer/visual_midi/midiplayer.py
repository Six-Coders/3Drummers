import pygame       #Se instala
from pygame import mixer
import numpy as np  #Se instala
import time
import random
import os
from .miditransform import magenta_get_notes, set_color_intensity, check_drums

os.environ['PYGAME_HIDE_SUPPORT_PROMPT'] = "hide"

clock = pygame.time.Clock()
particles = []
bpm = 120
hud_x = 32
hud_y = 32

class Particle:
    def __init__(self, position, color):
        self.x, self.y = position
        self.color = color
        self.vx = random.uniform(-1, 1)  # Velocidad horizontal
        self.vy = random.uniform(-1, 1)  # Velocidad vertical
        self.lifetime = random.randint(10, 20)  # Tiempo de vida de la partícula

    def update(self):
        self.x += self.vx
        self.y += self.vy
        self.lifetime -= 1

    def draw(self, surface):
        pygame.draw.circle(surface, self.color, (int(self.x), int(self.y)), 2)

class Drums(): #Le puse Drums a los cuadraditos del inicio xd
    def __init__(self,x,y,color1):
        self.x = x
        self.y = y
        self.color1 = color1
        self.rect = pygame.Rect(self.x, self.y, 10, 10)

class Drums_Note(): #Las notas qlas que aparecen :c
    def __init__(self,x,y,color,move_speed,pitch):
        self.x = x
        self.y = y
        self.color = color
        self.move_speed = move_speed
        self.pitch = pitch
    
    def update(self):
        self.x -= self.move_speed
        if self.x <= 100 + 1 + self.move_speed and self.x >= 100 - (1 + self.move_speed) :
            # Crear particulas
            for _ in range(10):
                particles.append(Particle((self.x + 8, self.y+4), self.color))

    def draw(self,screen):
        if self.x >= 100:
            pygame.draw.rect(screen, self.color, pygame.Rect(self.x, self.y, 10, 10))

def set_drums_hud(drums_order, percussions_used, drums_color, timeline_x, timeline_y, hud_separation_y = 50, xoffset=42, yoffset=16):
    drums_start = []
    drum_element_y = {}
    drums_icons_positions = []
    for note in drums_order:
        color = drums_color[note]
        if note not in percussions_used:
            color = (100,100,100) #Setear a gris los elementos no usados.
        drums_start.append(Drums(timeline_x,timeline_y,color))
        drums_icons_positions.append((timeline_x-xoffset, timeline_y-yoffset))
        drum_element_y[note] = timeline_y
        timeline_y += hud_separation_y
    return drums_start,drum_element_y,drums_icons_positions

def setting_hud(rect_starts,drums_icons,drums_hud,drums_names,screen): #Setear elementos del HUD de la bateria y rectangulos xd
    for i in range(len(rect_starts)):
        rect = rect_starts[i]
        x,y = drums_icons[i]
        screen.blit(drums_hud[i], drums_icons[i])
        screen.blit(drums_names[i], (x-42,y+8))
        pygame.draw.line(screen,(220,220,220),(100,drums_icons[i][1]+20),(800,drums_icons[i][1]+20),1)
        pygame.draw.rect(screen,rect.color1,rect.rect)

def notes_controller(notes,transcurred_time):
    notes_used = []
    for i in notes:
        start_time, _,_ = i
        if transcurred_time >= start_time:
            notes_used.append(i)
    for i in notes_used:
        notes.remove(i)
    return notes,notes_used

def create_notes(appearing_notes,move_speed,drums_color,drums_elements_positions):
    notes_object = []
    for _,note,velocity in appearing_notes:
        color = drums_color[note]
        color_fixed = set_color_intensity(color,velocity)
        new_note = Drums_Note(800,drums_elements_positions[note],color_fixed,move_speed,note)
        notes_object.append(new_note)
    return notes_object

# Aqui empieza lo bueno...
def midiplayer(midi_pathfile, soundsrc_path="",timeline_separation = 50, soundoffset = 40):
    sync_fps = 75
    delay_time = 2 # Delay en SEGUNDOS!
    note_speed = 6
    drums_color = {36: (0,255,100), 38: (255,100,0), 46:(100,0,255), 48:(255,255,100), 49: (255,100,255), 51:(255,127,127)}

    notes,percussions_used = magenta_get_notes(midi_pathfile, delay_time)
    if notes == []:
        print("No hay notas en la batería")
        return False
    if not check_drums(notes):
        print("Al parecer la canción no tiene batería")
        return False
    # Setear posicionamiento de la timeline
    timeline_y = 200
    timeline_x = 100

    #Setear elementos HUD
    drums_hud_position = [51,49,48,46,38,36] #Setear orden de elementos del HUD

    rect_starts,drums_elements_positions,drums_icons = set_drums_hud(drums_hud_position,percussions_used,drums_color,timeline_x,timeline_y,timeline_separation)

    screen = pygame.display.set_mode((800,600))
    pygame.display.set_caption("3Drummers - Visual MIDI")
    icon = pygame.image.load('icon.png')
    pygame.display.set_icon(icon)
    
    #Cargar elementos del HUD
    snare_image = pygame.image.load('visual_midi/icons/snare.png')
    snare = pygame.transform.scale(snare_image, (hud_x,hud_y))

    kick_image = pygame.image.load('visual_midi/icons/kick.png')
    kick = pygame.transform.scale(kick_image, (hud_x,hud_y))

    hihat_image = pygame.image.load('visual_midi/icons/hi_hat.png')
    hihat = pygame.transform.scale(hihat_image, (hud_x,hud_y))

    cymbal_image = pygame.image.load('visual_midi/icons/cymbal.png')
    cymbal = pygame.transform.scale(cymbal_image, (hud_x,hud_y))

    ride_image = pygame.image.load('visual_midi/icons/ride.png')
    ride = pygame.transform.scale(ride_image, (hud_x,hud_y))

    tom_image = pygame.image.load('visual_midi/icons/tom.png')
    tom = pygame.transform.scale(tom_image, (hud_x,hud_y))

    drums_hud = [ride,cymbal,tom,hihat,snare,kick]

    pygame.font.init()
    font = pygame.font.Font(None, 18)
    text_snare = font.render("Caja", True, (240, 240, 240))
    text_kick = font.render("Bombo", True, (240, 240, 240))
    text_cymbal = font.render("Platillo", True, (240, 240, 240))
    text_hihat = font.render("Hi hat", True, (240, 240, 240))
    text_ride = font.render("Ride", True, (240, 240, 240))
    text_tom = font.render("Tom", True, (240, 240, 240))

    drums_names = [text_ride,text_cymbal,text_tom,text_hihat,text_snare,text_kick]
    
    #Reproducir canción
    transcurred_time = 0.0
    is_played = False
    if soundsrc_path != "":
        mixer.init()
        mixer.music.load(soundsrc_path)

    notes_objects = []
    factor = note_speed/2
    while True:
        screen.fill((0,0,0))
        dt = clock.tick(sync_fps) / 1000.0
        transcurred_time += dt
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                return
        
        setting_hud(rect_starts,drums_icons,drums_hud,drums_names,screen) 
        #Sync!
        notes,appearing_notes = notes_controller(notes,transcurred_time)
        objects = create_notes(appearing_notes,note_speed,drums_color,drums_elements_positions)
        notes_objects.extend(objects)

        #Dibujar notas:
        for note in notes_objects:
            note.update()
            note.draw(screen)

        for particle in particles:
            particle.update()
            particle.draw(screen)
            if particle.lifetime <= 1:
                particles.remove(particle)
                
        offset = 700 / (note_speed / dt)
        if transcurred_time >= delay_time + offset:
            if is_played == False:
                mixer.music.play()
                is_played = True

        pygame.display.update()


def main(argv):
    midi_pathfile = argv[0]
    soundsrc_path = argv[1]
    return midiplayer(midi_pathfile,soundsrc_path)


if __name__ == "__main__":
    main()