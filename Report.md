# Term Project Report - 2018400192 Muhammet Şen
## Description
I have chosen to demonstrate the technical difficulties of having many players in a multiplayer game world. I have focused on consequences of network delay and packet losses in gaming experience. I have created a world with many players. Position change of one player is sent to other players. 

## Implementation
### Server
I have used [Flask](https://flask.palletsprojects.com/en/2.1.x/) with [Flask-sock](https://flask-sock.readthedocs.io/en/latest/) to create a server that provides websocket communication with players. All of the communication between server and player are built upon websocket protocol. Server can send different types of messages to player, which will be defined later, while player only sends the position data if necessary.
### Unity
In order to create many game objects with scripts, I have created a prefab from my player object. The main difference is that prefab does not have scripts to handle keyboard input and communication with the server. Let's assume we have `x` players in the world for a given moment. `x-1` of those players have been created from the prefab while reamining one is the main player of that Unity instance. 

### Communication 
I have created a message pattern that is viable for all the communication I need between game instances and the server. Each message from server to clients composed of a user id with x and z coordinates that indicates the position of the regarding player.
In initialization process, message object also includes red, green, and blue values to transfer the color of the player. 

#### Initialization
When a new player joins to our game world, server creates a `User` object that hold the state of the player. After creating the User object, server sends the list of the existing players to the new player with the `initialization` type so that new player can see all of the other players. Similarly, server broadcasts the new player to the existing players with the same type. After ensuring all of the clients are in sync, server launches the infinite loop to listen for incoming messages from websocket. 

#### Updates
When a player's position change, client must send the new position to the server. Since sending every one of the updates would result on way too many network packages, the messages is sent only if the distance between last sent and current position is greater than 0.1.  Server broadcasts this change to all of the players.
I wanted to implement a mechanism that reduces the overall network usage by sending the update to players that can see the moved player. This may be possible by measuring the distance between players, dividing the map into chunks, or being aware of the players that can be seen in the screen. This is one of the remarks that will improve the project. 

#### Network 
I have created 3 different scenarios to see the effects of network related problems to gaming experience. First of all, I created a synthetic network delay and package drop to test by manually adjusting by providing random number.
In order to simulate network delay, I have implemented a queueing system in Unity that manage incoming and outgoing network packages. In the `Connection` class, there are two coroutines, one for sending and one for receiving, that run in an infinite loop. For example, `SendCoroutine` checks if there are a message waiting in the `sendQueue` and sends the message via websocket connection. For simulation purposes I have added a random wait time, I will describe those random values in another section below. By this queueing mechanism, I can simulate network delays.
Other than delays, another type of network related challenge is package drops. Randomly, some of the packages sent may not reach to the target. In order to simulate this, I have added a basic check to server before sending messages to clients. I could add a similar mechanism for messages from client to server; however, observing effect of drops would be harder. 
Other than this local environment, I have deployed my game server to a machine in Istanbul and to an EC2 in Frankfurt. (I am living in Istanbul)
### Results
- Local environment, no delays and packet losses (Best case)
![](https://i.imgur.com/eaYjl6D.gif)
#### Local Environment
##### Network Delay 
In each case, network delay applies both for incoming and outgoing messages. For example, if the sending delay for moved player is 52 ms and incoming delay for one of the clients is 48 ms, there will be a 100 ms time difference between the player pressing the key and the other player see the update. Random intervals below do not represent the total delay.
- Uniform distribution in 10-40 ms for delay
![](https://i.imgur.com/RVZCdqj.gif)
- Uniform distribution in 30-60 ms for delay
![](https://i.imgur.com/gpIpMZe.gif)
- Uniform distribution in 70-100 ms for delay
![](https://i.imgur.com/4YKYFyl.gif)

##### Packet loss
- 10% probability of packet loss
![](https://i.imgur.com/DhCuacL.gif)
- 30% probability of packet loss
![](https://i.imgur.com/1pdmcR2.gif)


#### Server in Istanbul
Average ping: 13.85 ms
![](https://i.imgur.com/rXZOEsN.gif)

#### EC2 Instance in Frankfurt
Average ping: 39.77 ms
![](https://i.imgur.com/0HPodvT.gif)

### Analysis of the Results
**Packet loss**
First of all, packet loss is not crucial for update messages, when a player change position. Update message broadcast gets triggered multiple times even the key got pressed for a very short time interval. Dropping 10% of the packages does not have an obvious impact on gaming experience. However, dropping packages like I sent during the initialization process creates a crucial problem. As you can see in packet loss gifs, some players do not update their screen accordingly since they could not get the initialization messages. In other words, they do not know the moving player. This is a very serious problem and there should be precautions, like making sure the list of players is in sync with the server, must be taken.
As you can see, connecting to a remote server did not result in a similar situation. 10% was a very pessimistic guess for packet loss simulation.
**Network Delay**
Delay is more noticeable than packet loss in update messages. As you can observe in the gifs above, delay above 50 ms starts to affect gaming experience. My communication design, using queues in coroutines, has a role in this effect. A new message has to enter the queue and wait for previously created messages. This results in a cumulative delay time. The same methodology applies for incoming messages.
Another challenge is fixing incorrect order of incoming packages. A message may arrive earlier than previously sent one. This results in player moving back and forward suddenly which looks worse than cumulative delay. 