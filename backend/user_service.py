from typing import Dict

from flask_sock import Server, Sock

from user import User


class UserService():
    __instance = None
    count = 0

    def __new__(cls, *args, **kwargs):
        if cls.__instance is None:
            cls.__instance = object.__new__(cls)
            cls.__instance._initialized = False
        return cls.__instance

    def __init__(self) -> None:
        if self._initialized:
            return
        self._initialized = True
        self.users: Dict[int, User] = {}

    def add_new_user(self, socket: Server) -> User:
        user: User = User(name="name", socket=socket, id=self.count)
        self.users[self.count] = user
        self.count += 1
        return user

    def remove_user(self,  id: int):
        if id in self.users.keys():
            del self.users[id]

    def get_user(self, id: int) -> User:
        return self.users[id]
