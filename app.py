from fastapi import FastAPI
from pydantic import BaseModel

from groq import Groq

from dotenv import load_dotenv
import os

load_dotenv()

client = Groq(
    api_key=os.getenv("GROQ_API_KEY")
)

app = FastAPI()

SYSTEM_PROMPT = open(
    "npc.txt",
    encoding="utf-8"
).read()

class ChatRequest(BaseModel):
    message: str

@app.get("/")
def health():
    return {
        "status": "running"
    }

@app.post("/chat")
def chat(req: ChatRequest):

    completion = client.chat.completions.create(
        model="llama-3.3-70b-versatile",
        messages=[
            {
                "role": "system",
                "content": SYSTEM_PROMPT
            },
            {
                "role": "user",
                "content": req.message
            }
        ]
    )

    return {
        "response":
        completion.choices[0].message.content
    }