using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyboard_move : MonoBehaviour
{
    public Rigidbody rb;
    private Connection connection;
    private int speed = 10;
    private Vector3 lastSentPosition;


    void Start()

    {
        rb = GetComponent<Rigidbody>();
        this.connection = GetComponent<Connection>();

    }


    void Update()
    {
    }
    void FixedUpdate()
    {

        Vector3 Movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        rb.transform.position += Movement * speed * Time.fixedDeltaTime;
        this.SendPosition();

    }
    void SendPosition()
    {  
        Vector3 position = rb.transform.position;

        if(this.lastSentPosition != null && Vector3.Distance(position,lastSentPosition)>0.1){
            this.lastSentPosition = position;
            this.connection.SendWebSocketMessage(position.ToString());
        }
    }
}
