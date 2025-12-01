# player_keyboard.py
import asyncio, json, time, math
import websockets
import pygame

SERVER_URI = "ws://192.168.137.1:5000"   
ROOM       = "sala1"
ENTITY_ID  = "cube_1"

SEND_HZ = 20
SPEED   = 3.0       
ROT_DPS = 90.0      

async def ws_sender(state_iter):
    """Abre socket, hace handshake y envía estados que recibe del iterador."""
    while True:
        try:
            async with websockets.connect(SERVER_URI) as ws:
                
                await ws.send(json.dumps({"id": "player", "room": ROOM}))
                print("[PLAYER] Conectado")
               
                async for msg in state_iter:
                    await ws.send(msg)
        except Exception as e:
            print("[PLAYER] WS error:", e, "-> reconectando en 1s")
            await asyncio.sleep(1)

def main():
    pygame.init()
    screen = pygame.display.set_mode((400, 200))
    pygame.display.set_caption("Player Controller (WASD/QE)")

    clock = pygame.time.Clock()

    x, z, yaw = -5.40000105, 16.3700027, -28.329998

    async def state_generator():
        """Genera JSON de estado a SEND_HZ con la posición/rotación actual."""
        last_send = 0.0
        while True:
            now = time.time()
            if now - last_send >= 1.0 / SEND_HZ:
                last_send = now
            
                rad = math.radians(yaw)
                cy, sy = math.cos(rad * 0.5), math.sin(rad * 0.5)
                qx, qy, qz, qw = 0.0, sy, 0.0, cy

                payload = {
                    "t": "state",
                    "id": ENTITY_ID,
                    "x": x, "y": 0.0, "z": z,
                    "rx": qx, "ry": qy, "rz": qz, "rw": qw
                }
                yield json.dumps(payload)
            await asyncio.sleep(0) 

    # arrancar tarea de envío
    loop = asyncio.get_event_loop()
    state_iter = state_generator()
    asyncio.ensure_future(ws_sender(state_iter))

    # bucle de entrada/movimiento local
    running = True
    while running:
        dt = clock.tick(60) / 1000.0  # segundos

        for ev in pygame.event.get():
            if ev.type == pygame.QUIT:
                running = False

        keys = pygame.key.get_pressed()
        # WASD para mover en XZ
        dx = (keys[pygame.K_d] - keys[pygame.K_a]) * SPEED * dt
        dz = (keys[pygame.K_w] - keys[pygame.K_s]) * SPEED * dt
        x += dx
        z += dz

        # Q/E para rotar (yaw)
        yaw += (keys[pygame.K_e] - keys[pygame.K_q]) * ROT_DPS * dt
        yaw %= 360.0

        # UI mínima
        screen.fill((20, 20, 20))
        font = pygame.font.SysFont(None, 18)
        txt = font.render(f"pos=({x:.2f},{z:.2f}) yaw={yaw:.1f}", True, (230,230,230))
        screen.blit(txt, (10, 10))
        pygame.display.flip()

        # dejar correr el loop asyncio
        loop.run_until_complete(asyncio.sleep(0))

    pygame.quit()

if __name__ == "__main__":
    main()
