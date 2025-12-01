# voz.py
import sounddevice as sd
import queue
import json
import asyncio
import websockets
import time
from vosk import Model, KaldiRecognizer

MODEL_PATH = "vosk-model-small-es-0.42"
SERVER_URI = "ws://localhost:5000"   # URI del servidor WebSocket

# Palabras que activan envío
ACTIVATION_WORDS = {
    "árbol", "roca", "espada", "piedra",
    "numero", "uno", "dos", "tres",
    "escudo"
}

model = Model(MODEL_PATH)
recognizer = KaldiRecognizer(model, 16000)
q = queue.Queue()

NUMEROS = {
    "cero": "0", "uno": "1", "dos": "2", "tres": "3", "cuatro": "4",
    "cinco": "5", "seis": "6", "siete": "7", "ocho": "8", "nueve": "9"
}


def callback(indata, frames, time_info, status):
    if status:
        print(status, flush=True)
    q.put(bytes(indata))


async def send_command(word: str):
    """
    Envía un JSON al servidor:
      - si word == "escudo"  -> target = "meta"  (directo al gigante)
      - si word == "espada"  -> target = "mago"  (PC mago)
    """
    if word == "escudo":
        target = "meta"
    elif word == "espada":
        target = "mago"
    else:
        
        print(f"(info) palabra activada pero sin routing especial: {word}")
        return

    hello = {
        "type": "hello",
        "role": "voz",
        "id": "voz_pc",
        "t": time.time()
    }

    payload = {
        "type": "spawn_item",
        "item": word,      
        "target": target,  
        "from": "voz",
        "role": "voz",
        "id": "voz_pc",
        "t": time.time()
    }

    try:
        async with websockets.connect(SERVER_URI) as ws:
           
            await ws.send(json.dumps(hello))
        
            await ws.send(json.dumps(payload))
            print(f" Enviado al server: {payload}")
    except Exception as e:
        print(f" Error al conectar o enviar: {e}")


def listen_loop():
    """Mantiene el micrófono activo y envía solo cuando detecta palabra relevante."""
    with sd.RawInputStream(
        samplerate=16000,
        blocksize=8000,
        dtype='int16',
        channels=1,
        callback=callback
    ):
        print("🎧 Micrófono activo (di 'espada' o 'escudo')...")

        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        while True:
            data = q.get()
            if recognizer.AcceptWaveform(data):
                result = recognizer.Result()
                text = json.loads(result)["text"].strip().lower()

                if text:
                    print(f"🗣️ Detectado: {text}")
                    words = text.split()
                    for w in words:
                        if w in NUMEROS:
                            w = NUMEROS[w]
                        if w in ACTIVATION_WORDS:
                            print(f" Activación detectada: {w}")
                            loop.run_until_complete(send_command(w))
                            break  # evita enviar varias veces por la misma frase


if __name__ == "__main__":
    listen_loop()
