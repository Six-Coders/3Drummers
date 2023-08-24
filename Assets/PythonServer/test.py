import socket
import json

def send_ready_check():
    host = "127.0.0.1"
    port = 12345

    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect((host, port))

    message = {
        "command": "ready_check"
    }

    message_json = json.dumps(message)
    client_socket.send(message_json.encode())

    response = client_socket.recv(1024).decode()
    response_data = json.loads(response)
    print("Respuesta del servidor:", response_data["response"])

    client_socket.close()

if __name__ == "__main__":
    send_ready_check()