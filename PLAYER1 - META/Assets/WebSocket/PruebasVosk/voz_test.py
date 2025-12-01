import sounddevice as sd
import queue
import json
import asyncio
import websockets
from vosk import Model, KaldiRecognizer

MODEL_PATH = "vosk-model-small-es-0.42"
SERVER_URI = "ws://localhost:5000"   # ⚠️ cambia por la IP del servidor
ACTIVATION_WORDS = {"arbol", "roca", "espada", "escudo", "piedra", "numero", "uno", "dos", "tres", "reset", "reiniciar", "reinicia"} 


model = Model(MODEL_PATH)
recognizer = KaldiRecognizer(model, 16000)
q = queue.Queue()

NUMEROS = {
    "cero": "0", "uno": "1", "dos": "2", "tres": "3", "cuatro": "4",
    "cinco": "5", "seis": "6", "siete": "7", "ocho": "8", "nueve": "9"
}

def callback(indata, frames, time, status):
    if status:
        print(status, flush=True)
    q.put(bytes(indata))

async def send_command(word):
    """Envía una palabra al servidor y luego se desconecta."""
    try:
        async with websockets.connect(SERVER_URI) as ws:
            await ws.send(json.dumps({"id": "voz"}))
            await asyncio.sleep(0.1)  # pequeña espera para handshake
            await ws.send(word)
            print(f"📨 Enviado: {word}")
    except Exception as e:
        print(f"⚠️ Error al conectar o enviar: {e}")

def listen_loop():
    """Mantiene el micrófono activo todo el tiempo y envía solo cuando detecta palabra relevante."""
    with sd.RawInputStream(
        samplerate=16000, blocksize=8000, dtype='int16',
        channels=1, callback=callback
    ):
        print("🎧 Micrófono activo (di algo como 'árbol', 'roca', 'espada', 'escudo')...")

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
                            print(f"✅ Activación detectada: {w}")
                            loop.run_until_complete(send_command(w))
                            break  # evita enviar varias veces por la misma frase

if __name__ == "__main__":
    listen_loop()
