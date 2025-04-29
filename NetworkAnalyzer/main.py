from fastapi import FastAPI

app = FastAPI()

@app.get("/")
async def root():
    return {"message": "NetworkAnalyzer is alive!"}

@app.post("/analyze")
async def analyze_log(log: dict):
    """
    Simulate log analysis.
    """
    # Here, you could apply AI/ML models in real project
    print(f"Analyzing log: {log}")
    return {"status": "analyzed", "details": "No threat detected."}
