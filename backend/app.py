
import atexit
import sys
from typing import List

from apscheduler.schedulers.background import BackgroundScheduler
from flask import Flask, jsonify, request
from flask_sock import Server, Sock

from communication_service import CommunicationService
from user import User
from user_service import UserService
import random

app = Flask(__name__)
sock = Sock(app)


@app.route("/alive", methods=["GET"])
def alive():
    return jsonify(message="Up and running!")


@sock.route('/echo')
def echo(ws: Server):

    user: User = UserService().add_new_user(ws)
    print(f"new user: {user.id}", file=sys.stderr)

    #  Recognize the user and share the id 
    CommunicationService().initialize(user)
    # Announce the new player to others
    CommunicationService().alert_new_player(user)
    # Start the listener 
    CommunicationService().run_connection(user)


def main():
    app.run(host='0.0.0.0', port=5013)



if __name__ == '__main__':
    main()
