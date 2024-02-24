using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using ChessChallenge.API;

public class MyBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            // First checkmate, then capture, then develop. 
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)]; 
            bool foundCheckmate = false;
            bool foundCapture = false;
            bool foundCenterPawnMove = false;
            bool foundKnightMove = false;
            bool foundBishopMove = false;

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
                        if ((move.StartSquare.File == 3 && (move.StartSquare.Rank == 1 || move.StartSquare.Rank == 7)) ||
                            (move.StartSquare.File == 4 && (move.StartSquare.Rank == 1 || move.StartSquare.Rank == 7)))
                        {
                            if (move.TargetSquare.Rank == 3 || (move.TargetSquare.Rank == 6))
                            {
                                moveToPlay = move;
                                foundCenterPawnMove = true;
                                break;
                            }
                        }
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
                                {
                                    moveToPlay = move;
                                    foundKnightMove = true;
                                    break;
                                }
                        }
                    }
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
                        if ((move.StartSquare.File == 2 && (move.StartSquare.Rank == 0 || move.StartSquare.Rank == 8)) ||
                            (move.StartSquare.File == 5 && (move.StartSquare.Rank == 0 || move.StartSquare.Rank == 8)))
                            {
                                moveToPlay = move;
                                foundBishopMove = true;
                                break;
                            }
                    }
                }
            }
            
            // If all else fails then play a random move. 
            if (!foundCheckmate && !foundCapture && !foundCenterPawnMove && !foundKnightMove && !foundBishopMove)
            {
                return moveToPlay;
            }
            
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