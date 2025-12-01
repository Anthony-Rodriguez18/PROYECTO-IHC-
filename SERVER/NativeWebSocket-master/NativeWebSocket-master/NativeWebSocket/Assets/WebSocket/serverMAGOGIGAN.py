# server.py
import asyncio
import json
import time
import websockets

clients = set()          # conjunto de websockets conectados
client_roles = {}       # mapa websocket -> role
client_ids = {}         


def f(x, name):
    if x is None:
        raise ValueError(f"missing field {name}")
    return float(x)


async def handler(ws):
    print(">> Nuevo cliente conectado")
    clients.add(ws)
    try:
        async for raw in ws:
            msg = None
            try:
                msg = json.loads(raw)
            except Exception as e:
                print("!! Parse error JSON:", e, "| raw:", raw)
                continue

            typ  = msg.get("type")
            role = msg.get("role")
            pid  = msg.get("id")
            t    = msg.get("t", 0.0)

            # registro
            if typ == "hello":
                client_roles[ws] = role or "unknown"
                client_ids[ws]   = pid  or "unknown"
                print(f">> Registrado cliente role={role} id={pid}")
                # no se reenvía el hello
                continue

            # 2) Logs básicos
            if typ == "transform":
                try:
                    px = f(msg.get("px"), "px")
                    py = f(msg.get("py"), "py")
                    pz = f(msg.get("pz"), "pz")
                    rx = f(msg.get("rx"), "rx")
                    ry = f(msg.get("ry"), "ry")
                    rz = f(msg.get("rz"), "rz")
                    print(
                        f"[RX] {typ} {role} {pid} t={t:.3f} "
                        f"p=({px:.3f},{py:.3f},{pz:.3f}) "
                        f"r=({rx:.1f},{ry:.1f},{rz:.1f})"
                    )
                except Exception as e:
                    print("!! Error leyendo transform:", e)

            elif typ == "spawn_item":
                item   = msg.get("item")
                target = msg.get("target")
                print(f"[RX] SPAWN_ITEM item='{item}' target={target} from={role} id={pid}")

            else:
                print(f"[RX] {typ} {role} {pid} t={t}")

            # 3) Routing 
            target = msg.get("target", "all") 

            if target == "all" or target is None:
                recipients = list(clients)
            else:
                
                recipients = [
                    c for c in clients
                    if client_roles.get(c) == target or client_ids.get(c) == target
                ]

            dead = []
            for c in recipients:
                try:
                    await c.send(raw)
                except Exception:
                    dead.append(c)

            for d in dead:
                clients.discard(d)
                client_roles.pop(d, None)
                client_ids.pop(d, None)

    except Exception as e:
        print("Error en cliente:", e)
    finally:
        print(">> Cliente desconectado:", client_ids.get(ws))
        clients.discard(ws)
        client_roles.pop(ws, None)
        client_ids.pop(ws, None)


async def main():
    async with websockets.serve(
        handler,
        "0.0.0.0",
        5000,
        ping_interval=20,
        ping_timeout=20
    ):
        print("WebSocket server on ws://0.0.0.0:5000")
        await asyncio.Future()


if __name__ == "__main__":
    asyncio.run(main())
