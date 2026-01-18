# Project Status and Roadmap

## Overview
This document tracks the implementation status of features defined in the Functional Specifications and verifies alignment with the project goals.

## Core Features Status

### 1. User Management
- [x] **User Identification**: Simple login screen asking for a name.
- [x] **Profile Selection**: Display list of existing users.
- [ ] **History & Progress**: Tracking individual user progress (Database implemented, UI pending).

### 2. Content & Knowledge
- [ ] **Wikipedia Article Fetching**: Integration with Wikipedia API.
- [ ] **Quiz Generation**: LLM integration to generate questions.
- [ ] **Spaced Repetition System**: Algorithm to schedule reviews.

### 3. User Interface (UI/UX)
- [x] **Project Structure**: Frontend (React/Vite) and Backend (.NET) initialized.
- [x] **Theme System**: Dynamic switching between 5 themes (Curiosity Loop, Derot Brain, Knowledge Core, Mind Lab, Neo-Wikipedia).
- [x] **Theme Persistence**: Save user preference.
- [ ] **Responsive Design**: Ensure mobile compatibility (ongoing).

## Backlog
- [ ] Implement Backend Service for Wikipedia API.
- [ ] Implement LLM Service (Ollama/OpenAI) for Quiz Generation.
- [ ] Create "Learn" mode UI (Reading + Quiz).
- [ ] Create "Review" mode UI (Spaced Repetition).
