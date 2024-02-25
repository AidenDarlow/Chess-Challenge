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

            if (!foundCheckmate)
                {
                    foreach (Move move in allMoves)
                    {
                        // Find highest value capture
                        Piece capturedPiece = board.GetPiece(move.TargetSquare);
                        int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
                        Piece myPiece = board.GetPiece(move.StartSquare);
                        int myPieceValue = pieceValues[(int)myPiece.PieceType];
                        int highestValueCapture = myPieceValue - 1;

                        if (capturedPieceValue > highestValueCapture)
                        {
                            moveToPlay = move;
                            highestValueCapture = capturedPieceValue;
                            foundCapture = true;
                            break;
                        }
                    }
                }

            if (!foundCheckmate && !foundCapture)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    if (testpiece.PieceType == PieceType.Pawn)
                    {
                        // Check if the pawn moves to one of the center squares (e4, d4, e5, d5)
                        if ((move.StartSquare.File == 3 && (move.StartSquare.Rank == 1 || move.StartSquare.Rank == 6)) ||
                            (move.StartSquare.File == 4 && (move.StartSquare.Rank == 1 || move.StartSquare.Rank == 6)))
                        {
                            if (move.TargetSquare.Rank == 3 || (move.TargetSquare.Rank == 4))
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

            if (!foundCheckmate && !foundCapture && Endgame == true)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    if (testpiece.PieceType == PieceType.Pawn)
                    {
                        // Check for endgame promotions! 
                        if (move.StartSquare.Rank == 1 || move.StartSquare.Rank == 6 && move.TargetSquare.Rank == 0 || move.TargetSquare.Rank == 7)
                            moveToPlay = move; 
                            return moveToPlay; 
                    }
                }
            }
            
            if (!foundCheckmate && !foundCapture && !foundCenterPawnMove)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    if (testpiece.PieceType == PieceType.Knight)
                    {
                        // If no center pawn moves were found, prioritize knight moves. 
                        {
                            if ((move.StartSquare.File == 1 && (move.StartSquare.Rank == 0 || move.StartSquare.Rank == 8)) ||
                                (move.StartSquare.File == 6 && (move.StartSquare.Rank == 0 || move.StartSquare.Rank == 8)))
                                if (move.TargetSquare.Rank == 2 || move.TargetSquare.Rank == 6)
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

            if (!foundCheckmate && !foundCapture && !foundCenterPawnMove && !foundKnightMove)
            {
                foreach (Move move in allMoves)
                {
                    // Set what piece we're working with. 
                    Piece testpiece = board.GetPiece(move.StartSquare);

                    if (testpiece.PieceType == PieceType.Bishop)
                    {
                        // If no center pawn or knight moves were found then bishops to move. 
                        if ((move.StartSquare.File == 2 && (move.StartSquare.Rank == 0 || move.StartSquare.Rank == 7)) ||
                            (move.StartSquare.File == 5 && (move.StartSquare.Rank == 0 || move.StartSquare.Rank == 7)))
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
            
            if (!foundCheckmate && !foundCapture && !foundCenterPawnMove && !foundKnightMove && !foundBishopMove)
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

            if (!foundCheckmate && !foundCapture && !foundCenterPawnMove && !foundKnightMove && !foundBishopMove && !castles)
            {
                if (Endgame == true)
                {
                    foreach (Move move in allMoves)
                    {
                        // Set what piece we're working with. 
                        Piece testpiece = board.GetPiece(move.StartSquare);

                        if (testpiece.PieceType == PieceType.Knight || testpiece.PieceType == PieceType.Bishop || testpiece.PieceType == PieceType.Rook || testpiece.PieceType == PieceType.Queen || testpiece.PieceType == PieceType.Pawn || testpiece.PieceType == PieceType.King)
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

                        if (testpiece.PieceType == PieceType.Knight || testpiece.PieceType == PieceType.Bishop || testpiece.PieceType == PieceType.Rook || testpiece.PieceType == PieceType.Queen || testpiece.PieceType == PieceType.Pawn)
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

            // Play a random move from the list. 
            if (myMoves.Count > 0)
            {
                int test = myMoves.Count; 
                int yes = rng.Next(0,test); 
                Move selectedMove = myMoves[yes]; 
                moveToPlay = selectedMove;
                myMoves.Remove(moveToPlay);
                return moveToPlay; 
            }

            // If all else fails then play a random move. 
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