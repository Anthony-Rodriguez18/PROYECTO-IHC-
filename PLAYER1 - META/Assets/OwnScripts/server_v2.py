# server.py
import asyncio, json, websockets

clients = set()

def f(x, name):
    if x is None:
        raise ValueError(f"missing field {name}")
    return float(x)

async def handler(ws):
    print(">> Nuevo cliente conectado")
    clients.add(ws)
    try:
        async for raw in ws:
            try:
                msg = json.loads(raw)
                t    = msg.get("t")
                typ  = msg.get("type")
                role = msg.get("role")
                pid  = msg.get("id")

                if typ == "transform":
                    px = f(msg.get("px"), "px")
                    py = f(msg.get("py"), "py")
                    pz = f(msg.get("pz"), "pz")
                    rx = f(msg.get("rx"), "rx")
                    ry = f(msg.get("ry"), "ry")
                    rz = f(msg.get("rz"), "rz")

                    print(f"[RX] {typ} {role} {pid} t={t:.3f} "
                          f"p=({px:.3f},{py:.3f},{pz:.3f}) "
                          f"r=({rx:.1f},{ry:.1f},{rz:.1f})")
                else:
                    print(f"[RX] {typ} {role} {pid} t={t}")

            except Exception as e:
                print("!! Parse error:", e, "raw:", raw)

            # reenvía a todos (eco)
            dead = []
            for c in clients:
                try:    await c.send(raw)
                except: dead.append(c)
            for d in dead:
                clients.discard(d)

    except Exception as e:
        print("Error en cliente:", e)
    finally:
        clients.discard(ws)
        print(">> Cliente desconectado")

async def main():
    async with websockets.serve(handler, "0.0.0.0", 5000, ping_interval=20, ping_timeout=20):
        print("WebSocket server on ws://0.0.0.0:5000")
        await asyncio.Future()

asyncio.run(main())