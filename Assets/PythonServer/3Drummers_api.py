import socket
import json
from demucs import separate
from onsets_frames_transcription import wav_to_midi
from difficulty import *

imports_done = False

def process_command(command,parameters):
    if command == "separate_tracks":
        song_path = parameters["parameter1"]
        lib_path = parameters["parameter2"]
        #separate.main(["--out", lib_path,song_path])
        separate.main(["--out", lib_path, "--two-stems", "drums", song_path])
        return {"response": "ok"}
    
    elif command == "transform_to_midi":
        directorie = parameters["parameter1"]
        checkpoint_dir = parameters["parameter2"]
        print(checkpoint_dir)
        wav_to_midi.main([directorie+"/drums.wav",directorie,checkpoint_dir])
        print("directorio: "+directorie)
        get_difficulty(directorie)
        return {"response": "ok"}
    elif command == "shutdown":
        exit()
    else:
        return {"response": "error"}

def handle_client(client_socket):
    data = client_socket.recv(1024).decode()
    message = json.loads(data)
    print("Mensaje: ",message)
    command = message["command"]
    parameters = message.get("parameters", {})

    if command == "ready_check":
        response_data = {"response": "ready"}
    else:
        response_data = process_command(command,parameters)

    response_json = json.dumps(response_data)
    client_socket.send(response_json.encode())
    client_socket.close()


def start_server():
    host = "127.0.0.1"  # Dirección IP del servidor (localhost)
    port = 2444  # Puerto en el que el servidor escucha

    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((host, port))
    server_socket.listen()

    print(f"Server listening {host}:{port}")

    while True:
        client_socket, client_address = server_socket.accept()
        print("Conexión: ",client_address)
        handle_client(client_socket)
        
    server_socket.close()

if __name__ == "__main__":
    start_server()