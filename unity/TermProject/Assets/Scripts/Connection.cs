using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;


public class Connection : MonoBehaviour
{
    private bool USE_DELAY = true;
    private string server_url = "ws://localhost:5013/echo";
    private WebSocket websocket;

    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
    public GameObject playerPrefab;
    public GameObject me;

    private Queue<string> sendQueue = new Queue<string>();
    private Queue<Message> inputQueue = new Queue<Message>();


    private int id = -1;
    private int delayLowerLimit = 70; //in miliseconds
    private int delayUpperLimit = 100; //in miliseconds
    private float nextRandom()
    {
        if (USE_DELAY)
        {
            return Random.Range(delayLowerLimit, delayUpperLimit) / 1000.0f;
        }

        return 0f; // not use delay

    }
    async void Start()
    {
        websocket = new WebSocket(server_url);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);
            Message messageObject = JsonUtility.FromJson<Message>(message);
            this.inputQueue.Enqueue(messageObject);
        };
        StartCoroutine(InputCoroutine());
        StartCoroutine(SendCoroutine());


        // waiting for messages
        await websocket.Connect();
    }
    void OnMessage(Message message)
    {
        if (message.type.Equals("initialization"))
        {
            this.id = message.id;
            Debug.Log($"My id is :{this.id}");
            Color color = new Color(r: message.red, g: message.green, b: message.blue, a: 1);
            this.me.GetComponent<Renderer>().material.color = color;
        }
        else if (message.type.Equals("new_player"))
        {
            Color color = new Color(r: message.red, g: message.green, b: message.blue, a: 1);
            this.initializeNewPlayer(message.id, new Vector3(message.x, 1, message.z), color);
        }
        else if (message.type.Equals("update"))
        {
            this.update_location(message.id, new Vector3(message.x, 1, message.z));
        }

    }
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    public void SendWebSocketMessage(string message)
    {
        if (websocket.State == WebSocketState.Open && this.id >= 0)
        {
            string formattedMessage = this.id.ToString() + "_" + message;
            // await websocket.SendText(formattedMessage);
            this.sendQueue.Enqueue(formattedMessage);
            // StartCoroutine(SendCoroutine(formattedMessage));
        }
    }
    private IEnumerator SendCoroutine()
    {
        while (true)
        {
            if (this.sendQueue.Count > 0)
            {
                yield return new WaitForSeconds(nextRandom());
                websocket.SendText(this.sendQueue.Dequeue());
            }
            yield return new WaitForSeconds(0);

        }
    }

    private IEnumerator InputCoroutine()
    {
        while (true)
        {
            if (this.inputQueue.Count > 0)
            {
                yield return new WaitForSeconds(nextRandom());
                OnMessage(inputQueue.Dequeue());
            }
            yield return new WaitForSeconds(0);

        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
    public bool initializeNewPlayer(int id, Vector3 position, Color color)
    {
        GameObject newPlayer;
        newPlayer = Instantiate(this.playerPrefab, position, Quaternion.identity);
        Rigidbody newPlayerBody = newPlayer.GetComponent<Rigidbody>();
        newPlayerBody.velocity = Vector3.zero;
        newPlayerBody.angularVelocity = Vector3.zero;
        newPlayer.GetComponent<Renderer>().material.color = color;
        this.players.Add(id, newPlayer);
        Debug.Log($"player {id} added ");
        return true;
    }
    public void update_location(int id, Vector3 newLocation)
    {
        if (!this.players.ContainsKey(id))
        {
            Debug.Log($"Could not find a user with the id: {id.ToString()}");
        }
        Rigidbody body = this.players[id].GetComponent<Rigidbody>();
        body.MovePosition(newLocation);
    }

}

[System.Serializable]
public class Message
{
    public string type;
    public int id;
    public float x;
    public float z;
    public float red;
    public float green;
    public float blue;

}