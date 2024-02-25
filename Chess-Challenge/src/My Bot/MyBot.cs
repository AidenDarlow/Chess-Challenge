using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using ChessChallenge.API;

public class MyBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();
            List<Move> myMoves = new(); 
            List<Move> pieces = new(); 

            // First checkmate, then capture, then develop. 
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)]; 
            bool Endgame = false;
            bool foundCheckmate = false;
            bool underAttack = false;
            bool foundCapture = false;
            bool foundCenterPawnMove = false;
            bool foundKnightMove = false;
            bool foundBishopMove = false;
            bool castles = false;

            PieceList Knight1 = board.GetPieceList(PieceType.Knight, true);
            PieceList Knight2 = board.GetPieceList(PieceType.Knight, false);
            PieceList Bishop1 = board.GetPieceList(PieceType.Bishop, true);
            PieceList Bishop2 = board.GetPieceList(PieceType.Bishop, false);
            PieceList Rook1 = board.GetPieceList(PieceType.Rook, true);
            PieceList Rook2 = board.GetPieceList(PieceType.Rook, false);
            PieceList Queen1 = board.GetPieceList(PieceType.Queen, true);
            PieceList Queen2 = board.GetPieceList(PieceType.Queen, false);

            int total = Knight1.Count + Knight2.Count + Bishop1.Count + Bishop2.Count + Rook1.Count + Rook2.Count + Queen1.Count + Queen2.Count;
                
            if (total < 6)
            {
                Endgame = true;
            }

            foreach (Move move in allMoves)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(board, move))
                {
                    moveToPlay = move;
                    foundCheckmate = true;
                    break;
                }
            }

            ulong blackControlledSquares = GetControlledSquares(board, false);
            ulong whiteControlledSquares = GetControlledSquares(board, true);

            // Visualize the controlled squares (for debugging)
            //BitboardHelper.VisualizeBitboard(blackControlledSquares);
            //BitboardHelper.VisualizeBitboard(whiteControlledSquares);

            ulong GetControlledSquares(Board board, bool isWhite)
            {
                ulong controlledSquares = 0;

                // Iterate over each pawn on the board
                foreach (var pawn in board.GetPieceList(PieceType.Pawn, isWhite))
                {
                    // Get the controlled squares for the current pawn and add them to the bitboard
                    controlledSquares |= BitboardHelper.GetPieceAttacks(PieceType.Pawn, pawn.Square, board, isWhite);
                }

                // Iterate over each knight
                foreach (var knight in board.GetPieceList(PieceType.Knight, isWhite))
                {
                    controlledSquares |= BitboardHelper.GetPieceAttacks(PieceType.Knight, knight.Square, board, !isWhite);
                }

                // Iterate over each bishop
                foreach (var bishop in board.GetPieceList(PieceType.Bishop, isWhite))
                {
                    controlledSquares |= BitboardHelper.GetPieceAttacks(PieceType.Bishop, bishop.Square, board, !isWhite);
                }

                // Iterate over each rook
                foreach (var rook in board.GetPieceList(PieceType.Rook, isWhite))
                {
                    controlledSquares |= BitboardHelper.GetPieceAttacks(PieceType.Rook, rook.Square, board, !isWhite);
                }

                // Iterate over each queen
                foreach (var queen in board.GetPieceList(PieceType.Queen, isWhite))
                {
                    controlledSquares |= BitboardHelper.GetPieceAttacks(PieceType.Queen, queen.Square, board, !isWhite);
                }

                // Iterate over each king
                foreach (var king in board.GetPieceList(PieceType.King, isWhite))
                {
                    controlledSquares |= BitboardHelper.GetPieceAttacks(PieceType.King, king.Square, board, !isWhite);
                }

                return controlledSquares;
            }

            // Define the attacking piece. 
            Piece attackingPiece;

            // Find the attacking piece. 
            Move[] history = board.GameMoveHistory; 
            int gameLength = history.Length;

            //Beforce capture... Am I under attack? If so, move away! 
            if (!foundCheckmate && gameLength > 0)
            {
                // Get the last move made by the opponent
                Move opponentLastMove = history[gameLength - 1]; 

                // Get the piece on the target square (the attacking piece)
                attackingPiece = board.GetPiece(opponentLastMove.TargetSquare);

                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece myPiece = board.GetPiece(move.StartSquare);

                    // Find the target square. 
                    Piece targetSquare = board.GetPiece(move.TargetSquare);

                    // Check and set integers for piece values & the square is defended. 
                    int attackingPieceValue = pieceValues[(int)attackingPiece.PieceType];
                    int myPieceValue = pieceValues[(int)myPiece.PieceType];

                    // Print the values (for debugging)
                    //Console.WriteLine("My piece value is: " + myPieceValue);
                    //Console.WriteLine("My opponents piece value is: " + attackingPieceValue);

                    // Find my color & set attacks to relevant bitboard. 
                    bool myColor = myPiece.IsWhite;
                    ulong attacks;
                    ulong defends;
                    if (myColor)
                    {
                        attacks = blackControlledSquares;
                        defends = whiteControlledSquares;
                    }
                    else
                    {
                        attacks = whiteControlledSquares;
                        defends = blackControlledSquares;
                    }
                        
                    // Check if the starting square is under attack. 
                    bool isattacked = BitboardHelper.SquareIsSet(attacks, move.StartSquare);

                    // Check if the target square is defended. 
                    bool targetisdefended = BitboardHelper.SquareIsSet(attacks, targetSquare.Square);

                    // Check if my starting square is defended. 
                    bool startisdefended = BitboardHelper.SquareIsSet(defends, targetSquare.Square);

                    if (myPieceValue - 1 > attackingPieceValue && isattacked && !targetisdefended || isattacked && !startisdefended && !targetisdefended)
                    {
                        underAttack = true;
                        myMoves.Add(move);
                    }

                    if (myMoves.Count > 0)
                    {
                        int test = myMoves.Count; 
                        int yes = rng.Next(0,test); 
                        Move selectedMove = myMoves[yes]; 
                        moveToPlay = selectedMove; 
                        myMoves.Remove(moveToPlay); 
                        return moveToPlay; 
                    }
                }
            }
            
            //Then Capture! 
            if (!foundCheckmate && !underAttack)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece myPiece = board.GetPiece(move.StartSquare);
                    
                    // Find what piece we're capturing. 
                    Piece capturedPiece = board.GetPiece(move.TargetSquare);

                    // Check and set integers for piece values & the square is defended. 
                    int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
                    int myPieceValue = pieceValues[(int)myPiece.PieceType];
                    int highestValueCapture = myPieceValue - 1;

                    if (capturedPieceValue > highestValueCapture)
                    {
                        moveToPlay = move;
                        foundCapture = true;
                        break;
                    }
                }
            }

            // Then develop a centre pawn! 
            if (!foundCheckmate && !underAttack && !foundCapture)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    // Find the target square. 
                    Piece targetSquare = board.GetPiece(move.TargetSquare);

                    // Find my color & set attacks to relevant bitboard. 
                    bool myColor = testpiece.IsWhite;
                    ulong attacks;
                    if (myColor)
                    {
                        attacks = blackControlledSquares;
                    }
                    else
                    {
                        attacks = whiteControlledSquares;
                    }
                        
                    // Check if the square is defended. 
                    bool isdefended = BitboardHelper.SquareIsSet(attacks, targetSquare.Square);

                    if (testpiece.PieceType == PieceType.Pawn)
                    {
                        // Check if the pawn moves to one of the center squares (e4, d4, e5, d5)
                        if ((move.StartSquare.File == 3 && (move.StartSquare.Rank == 1 || move.StartSquare.Rank == 6)) ||
                            (move.StartSquare.File == 4 && (move.StartSquare.Rank == 1 || move.StartSquare.Rank == 6)))
                        {
                            if (move.TargetSquare.Rank == 3 && isdefended == false || (move.TargetSquare.Rank == 4 && isdefended == false))
                            {
                                foundCenterPawnMove = true;
                                myMoves.Add(move);
                            }
                        }
                    }
                }
                
                if (myMoves.Count > 0)
                {
                int test = myMoves.Count; 
                int yes = rng.Next(0,test); 
                Move selectedMove = myMoves[yes]; 
                moveToPlay = selectedMove; 
                myMoves.Remove(moveToPlay); 
                return moveToPlay; 
                }
            }

            // Then endgame promotions! 
            if (!foundCheckmate && !underAttack && !foundCapture && Endgame == true)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    // Find the target square. 
                    Piece targetSquare = board.GetPiece(move.TargetSquare);

                    // Find my color & set attacks to relevant bitboard. 
                    bool myColor = testpiece.IsWhite;
                    ulong attacks;
                    if (myColor)
                    {
                        attacks = blackControlledSquares;
                    }
                    else
                    {
                        attacks = whiteControlledSquares;
                    }
                        
                    // Check if the square is defended. 
                    bool isdefended = BitboardHelper.SquareIsSet(attacks, targetSquare.Square);

                    if (testpiece.PieceType == PieceType.Pawn)
                    {
                        // Check for endgame promotions! 
                        if (move.StartSquare.Rank == 1 && move.TargetSquare.Rank == 0 && isdefended == false || move.StartSquare.Rank == 6 && move.TargetSquare.Rank == 7 && isdefended == false)
                            moveToPlay = move; 
                            return moveToPlay; 
                    }
                }
            }
            
            // Then develop Knights! 
            if (!foundCheckmate && !underAttack && !foundCapture && !foundCenterPawnMove)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    // Find the target square. 
                        Piece targetSquare = board.GetPiece(move.TargetSquare);

                    // Find my color & set attacks to relevant bitboard. 
                    bool myColor = testpiece.IsWhite;
                    ulong attacks;
                    if (myColor)
                    {
                        attacks = blackControlledSquares;
                    }
                    else
                    {
                        attacks = whiteControlledSquares;
                    }
                        
                    // Check if the square is defended. 
                    bool isdefended = BitboardHelper.SquareIsSet(attacks, targetSquare.Square);

                    if (testpiece.PieceType == PieceType.Knight)
                    {
                        // If no center pawn moves were found, prioritize knight moves. 
                        {
                            if ((move.StartSquare.File == 1 && (move.StartSquare.Rank == 0 || move.StartSquare.Rank == 8)) ||
                                (move.StartSquare.File == 6 && (move.StartSquare.Rank == 0 || move.StartSquare.Rank == 8)))
                                if (move.TargetSquare.Rank == 2 && isdefended == false || move.TargetSquare.Rank == 6 && isdefended == false)
                                {
                                    foundKnightMove = true;
                                    myMoves.Add(move);
                                }
                        }
                    }
                }

                if (myMoves.Count > 0)
                {
                int test = myMoves.Count; 
                int yes = rng.Next(0,test); 
                Move selectedMove = myMoves[yes]; 
                moveToPlay = selectedMove; 
                myMoves.Remove(moveToPlay); 
                return moveToPlay; 
                }
            }

            // Then develop Bishops! 
            if (!foundCheckmate && !underAttack && !foundCapture && !foundCenterPawnMove && !foundKnightMove)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    // Find the target square. 
                    Piece targetSquare = board.GetPiece(move.TargetSquare);

                    // Find my color & set attacks to relevant bitboard. 
                    bool myColor = testpiece.IsWhite;
                    ulong attacks;
                    if (myColor)
                    {
                        attacks = blackControlledSquares;
                    }
                    else
                    {
                        attacks = whiteControlledSquares;
                    }
                        
                    // Check if the square is defended. 
                    bool isdefended = BitboardHelper.SquareIsSet(attacks, targetSquare.Square);

                    if (testpiece.PieceType == PieceType.Bishop)
                    {
                        // If no center pawn or knight moves were found then bishops to move. 
                        if ((move.StartSquare.File == 2 && (move.StartSquare.Rank == 0 && isdefended == false || move.StartSquare.Rank == 7 && isdefended == false)) ||
                            (move.StartSquare.File == 5 && (move.StartSquare.Rank == 0 && isdefended == false || move.StartSquare.Rank == 7 && isdefended == false)))
                            {
                                myMoves.Add(move);
                                foundBishopMove = true;
                            }
                    }
                }

                if (myMoves.Count > 0)
                {
                int test = myMoves.Count; 
                int yes = rng.Next(0,test); 
                Move selectedMove = myMoves[yes]; 
                moveToPlay = selectedMove; 
                myMoves.Remove(moveToPlay); 
                return moveToPlay; 
                }
            }
            
            // Castles! 
            if (!foundCheckmate && !underAttack && !foundCapture && !foundCenterPawnMove && !foundKnightMove && !foundBishopMove)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    if (testpiece.PieceType == PieceType.King && move.StartSquare.File == 4)
                    {
                        // Castle! 
                        if (move.TargetSquare.File == 6 || move.TargetSquare.File == 2)
                        {
                            castles = true;
                            moveToPlay = move;
                            break;
                        }
                    }
                }
            }

            // Then play random move! 
            if (!foundCheckmate && !underAttack && !foundCapture && !foundCenterPawnMove && !foundKnightMove && !foundBishopMove && !castles)
            {
                if (Endgame == true)
                {
                    foreach (Move move in allMoves)
                    {
                        // Set what piece we're working with. 
                        Piece testpiece = board.GetPiece(move.StartSquare);

                        // Find the target square. 
                        Piece targetSquare = board.GetPiece(move.TargetSquare);

                        // Find my color & set attacks to relevant bitboard. 
                        bool myColor = testpiece.IsWhite;
                        ulong attacks;
                        if (myColor)
                        {
                            attacks = blackControlledSquares;
                        }
                        else
                        {
                            attacks = whiteControlledSquares;
                        }
                        
                        // Check if the square is defended. 
                        bool isdefended = BitboardHelper.SquareIsSet(attacks, targetSquare.Square);

                        if (testpiece.PieceType == PieceType.Knight && isdefended == false || testpiece.PieceType == PieceType.Bishop && isdefended == false || testpiece.PieceType == PieceType.Rook && isdefended == false || testpiece.PieceType == PieceType.Queen && isdefended == false || testpiece.PieceType == PieceType.Pawn && isdefended == false || testpiece.PieceType == PieceType.King && isdefended == false)
                        {
                            myMoves.Add(move);
                        }
                    }
                }
                else 
                {
                    foreach (Move move in allMoves)
                    {
                        // Set what piece we're working with. 
                        Piece testpiece = board.GetPiece(move.StartSquare);

                        // Find what piece we're capturing. 
                        Piece targetSquare = board.GetPiece(move.TargetSquare);

                        // Find my color & set attacks to relevant bitboard. 
                        bool myColor = testpiece.IsWhite;
                        ulong attacks;
                        if (myColor)
                        {
                            attacks = blackControlledSquares;
                        }
                        else
                        {
                            attacks = whiteControlledSquares;
                        }
                        
                        // Check if the square is defended. 
                        bool isdefended = BitboardHelper.SquareIsSet(attacks, targetSquare.Square);

                        if (testpiece.PieceType == PieceType.Knight && isdefended == false || testpiece.PieceType == PieceType.Bishop && isdefended == false || testpiece.PieceType == PieceType.Rook && isdefended == false || testpiece.PieceType == PieceType.Queen && isdefended == false || testpiece.PieceType == PieceType.Pawn && isdefended == false)
                        {
                            myMoves.Add(move);
                        }
                    }
                }
                
                if (myMoves.Count > 0)
                {
                int first = myMoves.Count; 
                int yes = rng.Next(0,first); 
                Move selectedMove = myMoves[yes]; 
                moveToPlay = selectedMove; 
                myMoves.Remove(moveToPlay); 
                return moveToPlay; 
                }
            }

            // If all else fails then play a random move from the list. 
            if (myMoves.Count > 0)
            {
                int test = myMoves.Count; 
                int yes = rng.Next(0,test); 
                Move selectedMove = myMoves[yes]; 
                moveToPlay = selectedMove;
                myMoves.Remove(moveToPlay);
                return moveToPlay; 
            }

            // If all else fails then play a true random move. 
            return moveToPlay; 
        }
            
            // Test if this move gives checkmate
            bool MoveIsCheckmate(Board board, Move move)
            {
                board.MakeMove(move);
                bool isMate = board.IsInCheckmate();
                board.UndoMove(move);
                return isMate;
            }

    }