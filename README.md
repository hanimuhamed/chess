# Unity 2D Chess Engine

A simple 2D chess engine built in Unity.  
This project is a personal experiment to build a functional chess bot using Minimax with Alpha-Beta pruning and a fully working move logic system.

## Engine Strength

- Estimated ELO: around 1000 (based on over 40 manually played games against various levels of **Stockfish**)  
- Search depth: up to 3 plies with acceptable speed  
- Depth 4 works but becomes noticeably slow due to lack of search optimizations  
- Uses predefined book moves and book move responses for the first move  
- Includes positional bonuses so pieces make safer and more stable moves  
- Extensively tested through actual games to verify that no illegal moves occur and all legal moves are playable

## Game Features

- Play as White, Black, or Random  
- Multiple timer options available (similar to common chess time controls)  
- Undo and Reset buttons for testing and casual play  
- Simple 2D interface with fully working move logic

## Technical Details

- Engine Type: Minimax with Alpha-Beta Pruning  
- Evaluation: Material plus positional bonuses based on piece-square placement  
- Depth Limit: Adjustable, practically capped at 3 for performance  
- Implementation: Built in Unity 2D (C#)
- Rules: Supports standard chess rules including castling, en passant, and promotion

## Code Quality Disclaimer

The code works as intended but is messy and not well structured.  
It is not refactored and I have avoided changing it further since it already behaves the way I want.

## How to Run

1. Clone the repository  
   ```bash
   git clone https://github.com/yourusername/your-repo-name.git
