import asyncio
import websockets

clients = set()

async def handler(websocket):
    print(">> Nuevo cliente conectado")
    clients.add(websocket)
    try:
        async for message in websocket:
            print(f"Recibido: {message}")
            for client in list(clients):
                try:
                    await client.send(message)
                except:
                    clients.remove(client)
    except Exception as e:
        print("Error en cliente:", e)
    finally:
        clients.remove(websocket)
        print(">> Cliente desconectado")

async def main():
    async with websockets.serve(handler, "0.0.0.0", 5000):
        print("Servidor WebSocket corriendo en ws://localhost:5000")
        await asyncio.Future()

asyncio.run(main())
