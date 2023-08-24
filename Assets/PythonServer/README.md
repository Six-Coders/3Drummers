# Herramientas necesarias
## Instalar mini-forge
    - https://github.com/conda-forge/miniforge/releases/latest/download/Miniforge3-Windows-x86_64.exe



# Configuracion de ambiente (Directorio raiz):
## Crea el ambiente e instala librerias
    - conda env create -f environment.yml
## Activa el ambiente
    - conda activate [NOMBRE_AMBIENTE] #3Drummers
## Setea el ambiente
    - python setup.py install 
## Ejecuta el programa
    -python main.py [FILE_PATH]


--------------------------------------------------------------
# Modulos
## **Visual_midi**
## *midiplayer*
- __*midi_pathfile*__ : Ruta del archivo MIDI, este argumento es OBLIGATORIO.

- __*soundsrc_path*__: Ruta del archivo wav (esta seteado a wav porque el demucs tiene ese tipo de output), argumento es opcional.

- __*timeline_separation*__: Argumento opcional para fijar el tamaño de separación de los elementos en la timeline.

- __*soundoffset*__: Valor para ir sincronizando la         reproduccion del sonido con el MIDI jaajajja, es super al TUN TUN este valor ql.
-------------------------------

## **Onsets_frames_transcription**
## *wav_to_midi*
- __*filename*__ : Ruta del archivo .wav a transcribir, OBLIGATORIO.
- __*final_path*__: Ruta donde desea almacenar el midi.
--------------------------------
## **Demucs**
## *separate*
- __*--out target_path "*__ : Ruta donde se quiere almacenar los archivos separados.
- __*--two-stems "drums"*__ : Solo se obtienen 2 pistas, pista de drums separada, y la segunda pista contiene toda lo demas
- __*file_path*__ : Ruta de archivo que se desea separar. 

--------------------------------------------------------------
# Issues
- Dado que la canción seleccionada no tiene una pista de    batería. Cuando el usuario selecciona “Ver”
Entonces la aplicación muestra un mensaje diciendo que no se puede mostrar la interpretación porque no hay una batería en la canción

- Dado que la canción seleccionada no tiene todos los elementos percusivos de la batería
Cuando el usuario selecciona “Ver”
Entonces la aplicación muestra en color gris en la línea de tiempo el elemento de la batería que no está en la canción.


- **Error de Pygame audio al abrir nueva ventana**
- **No es posible detectar cuando una cancion no tiene bateráa**


    