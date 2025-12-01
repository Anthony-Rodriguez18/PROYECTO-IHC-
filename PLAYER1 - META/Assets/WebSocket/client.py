import asyncio
import websockets

async def send():
    uri = "ws://localhost:5000"
    async with websockets.connect(uri) as websocket:
        while True:
            text = input("Escribe comando (arbol/roca/espada): ")
            await websocket.send(text)

asyncio.run(send())
