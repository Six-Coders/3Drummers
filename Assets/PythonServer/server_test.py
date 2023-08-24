import socket

def send_command(command):
    host = "127.0.0.1"  # Dirección IP del servidor (localhost)
    port = 12345       # Puerto en el que el servidor escucha

    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect((host, port))

    client_socket.send(command.encode())
    response = client_socket.recv(1024).decode()

    client_socket.close()

    return response

if __name__ == "__main__":
    response = send_command('separate_tracks')
    print("Respuesta del servidor:", response)