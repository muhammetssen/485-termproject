import random

from flask_sock import Server


class Position:
    def __init__(self, x=0, z=0) -> None:
        self.x = x
        self.z = z


class Color():
    def __init__(self, red=0, green=0, blue=0) -> None:
        self.red = round(random.uniform(0, 1), 2)
        self.green = round(random.uniform(0, 1), 2)
        self.blue = round(random.uniform(0, 1), 2)


class User:
    def __init__(self, name, socket: Server, id: int) -> None:
        self.name = name
        self.socket: Server = socket
        self.position: Position = Position()
        self.color: Color = Color()
        self.id: int = id

    def update_position(self, x, z):
        self.position.x = x
        self.position.z = z
