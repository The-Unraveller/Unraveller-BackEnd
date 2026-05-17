# Design Document: LLM Integration & Dialogue Engine
## The Unraveller - Backend System

### 1. Understanding Summary
* **What is being built:** Backend components for "The Unraveller", an English learning app using real-time LLMs to simulate real-life scenarios as NPCs.
* **Why it exists:** To replace dry grammar exercises with engaging, interactive role-play environments for Gen Z in Vietnam.
* **Who it is for:** Vietnamese Gen Z learners looking to improve English communication and fluency.
* **Key Constraints:** 
  * "Suspicion Meter" acts as a survival mechanic and fluency indicator.
  * System must process responses in under 5 seconds.
  * Must prevent Prompt Injection from users.
* **Explicit Non-goals:** Not a pre-scripted dialogue game. Not a traditional multiple-choice app.

### 2. Assumptions
* **Technology:** C# .NET 9 Web API, EF Core, SQLite.
* **Integration:** Real-time LLM API (OpenAI/Gemini) is used.
* **Scale & Cost:** Rate limiting will be enforced to manage API costs.
* **Scope:** Backend must handle JSON parsing, dialogue saving, and Suspicion Level updating dynamically.

### 3. Decision Log
* **Decision 1: Single-Pass Structured Output**
  * *Alternatives considered:* Multi-pass (one call for grammar, one for response), Hybrid (LanguageTool + LLM).
  * *Reasoning:* Meets the < 5s latency constraint while keeping code complexity manageable and saving API costs. Modern LLMs are capable of returning reliable JSON.
* **Decision 2: Prompt Injection Mitigation via Delimiters**
  * *Alternatives considered:* Pre-filtering input with another model.
  * *Reasoning:* Adding delimiters (e.g., `[USER_TEXT]`) and strict negative prompt rules is the most cost-effective and fastest way for MVP.
* **Decision 3: Fallback Mechanism for Malformed JSON / Timeout**
  * *Alternatives considered:* Crashing/Throwing 500 error to user.
  * *Reasoning:* Graceful degradation is crucial for user experience. If LLM fails, we return a default "NPC is confused" response with 0 Suspicion penalty.

### 4. Final Architecture Design
**Components:**
1. `IGameEngineService`: Core business logic. Handles fetching chat history, calling LLM, updating DB, and evaluating Win/Lose conditions based on the current Suspicion Level.
2. `ILLMProviderService`: Dedicated HTTP client wrapper for the LLM API. Handles serialization, deserialization, markdown stripping (`SanitizeJson`), and timeouts.
3. `PromptBuilderFactory`: Utility to assemble System Prompts with Mission context, NPC persona, and scoring rules.

**Data Flow:**
`DialogueRequestDto` -> API Controller -> `IGameEngineService` -> `PromptBuilderFactory` -> `ILLMProviderService` -> LLM API -> (JSON response) -> `IGameEngineService` updates DB -> Returns `DialogueResponseDto`.
