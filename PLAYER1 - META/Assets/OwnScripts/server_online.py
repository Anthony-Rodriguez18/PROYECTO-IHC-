import asyncio
import json
import websockets

# Diccionario: cliente -> {"role": "player"/"meta"/"unity", "room": "default"}
clients = {}

async def handler(websocket):
    role = "anon"
    room = "default"
    clients[websocket] = {"role": role, "room": room}
    print(">> Nuevo cliente conectado")

    try:
        # Primer mensaje opcional: handshake (para rol o room)
        try:
            raw = await asyncio.wait_for(websocket.recv(), timeout=2.0)
            try:
                data = json.loads(raw)
                role = data.get("id") or data.get("role") or "anon"
                room = data.get("room", "default")
                clients[websocket] = {"role": role, "room": room}
                print(f">> Handshake: rol={role}, room={room}")
            except json.JSONDecodeError:
                # No era JSON, se reenvía igual
                await process_message(websocket, raw)
        except asyncio.TimeoutError:
            print(">> Sin handshake inicial, rol=anon")

        # Bucle principal
        async for message in websocket:
            await process_message(websocket, message)

    except Exception as e:
        print("Error en cliente:", e)

    finally:
        clients.pop(websocket, None)
        print(f">> Cliente {role} desconectado")

async def process_message(sender_ws, message):
    """Procesa y reenvía el mensaje recibido"""
    meta = clients.get(sender_ws, {"role": "anon", "room": "default"})
    s_role = meta["role"]
    s_room = meta["room"]

    print(f"[{s_role}@{s_room}] -> {message[:100]}")

    # Broadcast a todos excepto el emisor, misma room
    dead = []
    for ws, info in clients.items():
        if ws is sender_ws:
            continue
        if info["room"] != s_room:
            continue
        try:
            await ws.send(message)
        except:
            dead.append(ws)

    # Limpieza de clientes muertos
    for ws in dead:
        clients.pop(ws, None)

async def main():
    async with websockets.serve(handler, "0.0.0.0", 5000, ping_interval=None):
        print("Servidor WebSocket corriendo en ws://0.0.0.0:5000")
        await asyncio.Future()  # no termina nunca

if __name__ == "__main__":
    asyncio.run(main())