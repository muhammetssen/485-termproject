import json
import sys
import time

from flask_sock import ConnectionClosed
from user import User
from user_service import UserService
from flask_sock import Server
import random

PACKET_LOSS_PROBABILITY = 0.3

def send_message(socket: Server, data: dict) -> None:
    if socket is None:
        return

    is_package_dropped = random.uniform(0, 1)
    if(is_package_dropped < PACKET_LOSS_PROBABILITY):
        return
    
    socket.send(json.dumps(data))


class CommunicationService():
    __instance = None

    def __new__(cls, *args, **kwargs):
        if cls.__instance is None:
            cls.__instance = object.__new__(cls)
            cls.__instance._initialized = False
        return cls.__instance

    def __init__(self) -> None:
        if self._initialized:
            return
        self._initialized = True

    def run_connection(self, user: User):
        while True:
            data = user.socket.receive()

            user_id_string, position_string = data.split("_")
            position: tuple = eval(position_string)
            user_id = int(user_id_string)
            user: User = UserService().get_user(user_id)
            user.update_position(x=position[0], z=position[2])

            print(f"{data} from {user.id}", file=sys.stderr)
            for u in UserService().users.values():
                if user_id == u.id:
                    continue
                try:
                    if u.socket.connected:
                        guide_data = {
                            "type": "update",
                            "id": user_id,
                            "x": position[0],
                            "z": position[2]
                        }
                        send_message(socket=u.socket, data=guide_data)
                except ConnectionClosed as e:
                    UserService().remove_user(u)
                    print(str(e), file=sys.stderr)

    def initialize(self, user: User):
        send_message(socket=user.socket, data={
            "type": "initialization",
            "id": user.id,
            "x": user.position.x,
            "z": user.position.z,
            "red": user.color.red,
            "blue": user.color.blue,
            "green": user.color.green,
        })
        # announce current players to the new player
        for u in UserService().users.values():
            if u.id == user.id:
                continue
            send_message(socket=user.socket, data={
                "type": "new_player",
                "id": u.id,
                "x": u.position.x,
                "z": u.position.z,
                "red": u.color.red,
                "blue": u.color.blue,
                "green": u.color.green,
            })

    def alert_new_player(self, user: User):
        # alert new player to current players
        for u in UserService().users.values():
            if u.id == user.id:
                continue
            try:
                send_message(socket=u.socket, data={
                    "type": "new_player",
                    "id": user.id,
                    "x": user.position.x,
                    "z": user.position.z,
                    "red": user.color.red,
                    "blue": user.color.blue,
                    "green": user.color.green,
                })
            except Exception as e:
                print(str(e))
                UserService().remove_user(u)
