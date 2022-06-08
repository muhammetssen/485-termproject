## How to run
### With Docker
```bash
docker build -t websocket-server:latest .
docker run -p 5013:5013 -d websocket-server:latest 
```
### Without Docker
Use a Python version newer than 3.7
```bash
pip install -r requirements.txt 
python3 app.py
```