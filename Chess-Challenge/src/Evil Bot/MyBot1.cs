using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        public Move best;

        private int[,] bishoppos = new int[,]
       {
     {-5, -5, -5, -5, -5, -5, -5, -5},
     {-5, 10,  5,  8,  8,  5, 10, -5},
     {-5,  5,  3,  8,  8,  3,  5, -5},
     {-5,  3, 10,  3,  3, 10,  3, -5},
     {-5,  3, 10,  3,  3, 10,  3, -5},
     {-5,  5,  3,  8,  8,  3,  5, -5},
     {-5, 10,  5,  8,  8,  5, 10, -5},
     {-5, -5, -5, -5, -5, -5, -5, -5}
       };
        private int[,] knightpos = new int[,]
        {
     {-10, -5, -5, -5, -5, -5, -5,-10},
     { -8,  0,  0,  3,  3,  0,  0, -8},
     { -8,  0, 10,  8,  8, 10,  0, -8},
     { -8,  0,  8, 10, 10,  8,  0, -8},
     { -8,  0,  8, 10, 10,  8,  0, -8},
     { -8,  0, 10,  8,  8, 10,  0, -8},
     { -8,  0,  0,  3,  3,  0,  0, -8},
     {-10, -5, -5, -5, -5, -5, -5,-10}
        };

        private int[,] pawnposWhite = new int[,]
        {
     {0,  0,  0,  0,  0,  0,  0,  0},
     {0,  0,  0, -5, -5,  0,  0,  0},
     {0,  2,  3,  4,  4,  3,  2,  0},
     {0,  4,  6, 10, 10,  6,  4,  0},
     {0,  6,  9, 10, 10,  9,  6,  0},
     {4,  8, 12, 16, 16, 12,  8,  4},
     {5, 10, 15, 20, 20, 15, 10,  5},
     {0,  0,  0,  0,  0,  0,  0,  0}
        };

        // I created this, should be just a flipped
        // version of the white array
        private int[,] pawnposBlack = new int[,]
        {
     {0,  0,  0,  0,  0,  0,  0,  0},
     {5, 10, 15, 20, 20, 15, 10,  5},
     {4,  8, 12, 16, 16, 12,  8,  4},
     {0,  6,  9, 10, 10,  9,  6,  0},
     {0,  4,  6, 10, 10,  6,  4,  0},
     {0,  2,  3,  4,  4,  3,  2,  0},
     {0,  0,  0, -5, -5,  0,  0,  0},
     {0,  0,  0,  0,  0,  0,  0,  0}
         };
        public Move Think(Board board, Timer timer)
        {
            return minimaxTest(board, 3).Item1;
            //return Search(board,2,float.NegativeInfinity, float.PositiveInfinity).Item1;
        }

        /*public int getPieceValue(PieceType piece)
        {
            switch(piece)
            {
                case PieceType.Pawn: 
                    return 100;
                case PieceType.Knight:
                    return 300;
                case PieceType.Bishop:
                    return 300;
                case PieceType.Rook:
                    return 500;
                case PieceType.Queen:
                    return 900;
                case PieceType.King:
                    return 10000;
                default: return 0;

            }
        }

        public Move[] OrderMoves(Board board, Move[] moves)
        {
            float[] moveScore = new float[moves.Length];
            for(var i = 0; i < moves.Length; i++)
            {
                int moveScoreGuess = 0;
                PieceType movePieceType = board.GetPiece(moves[i].StartSquare).PieceType;
                PieceType capturePieceType = board.GetPiece(moves[i].TargetSquare).PieceType;

                if(capturePieceType != PieceType.None)
                {
                    moveScoreGuess = 10 * getPieceValue(capturePieceType) - getPieceValue(movePieceType);
                }

                if (moves[i].IsPromotion)
                {
                    moveScoreGuess += getPieceValue(moves[i].PromotionPieceType);
                }
                moveScore[i] = moveScoreGuess;
                /*if(board.SquareIsAttackedByOpponent(move.TargetSquare))
                {

                }
            }
            Array.Sort(moveScore, moves);
            return moves;
        }

        float SearchAllCaptures(Board board, float alpha, float beta)
        {
            float eval = EvalPosition(board);
            if(eval >= beta) return beta;
            alpha = Math.Max(alpha, eval);

            Move[] moves = board.GetLegalMoves(true);
            moves = OrderMoves(board, moves);
            foreach(Move move in moves)
            {
                board.MakeMove(move);
                eval -= SearchAllCaptures(board, -beta, -alpha);
                board.UndoMove(move);
                if(eval >= beta) return beta;
                alpha = Math.Max(alpha, eval);
            }
            return alpha;
        }
        */
        public float evalPosition(Board board, bool white)
        {
            float eval = 0;
            eval += board.GetPieceList(PieceType.Queen, white).Count * 900;
            eval += board.GetPieceList(PieceType.Rook, white).Count * 500;
            foreach (Piece piece in board.GetPieceList(PieceType.Pawn, white))
            {
                eval += 100 + 1 * pawnposWhite[piece.Square.File, piece.Square.Rank];
            }
            foreach (Piece piece in board.GetPieceList(PieceType.Knight, white))
            {
                eval += 300 + 1 * knightpos[piece.Square.File, piece.Square.Rank];
            }
            foreach (Piece piece in board.GetPieceList(PieceType.Bishop, white))
            {
                eval += 300 + 1 * bishoppos[piece.Square.File, piece.Square.Rank];
            }
            return eval;
        }

        public float EvalPositionTest(Board board, bool white)
        {
            float evalWhite = evalPosition(board, true);
            float evalBlack = evalPosition(board, false);
            if (board.IsWhiteToMove && board.IsInCheck()) evalWhite -= 10;
            if (!board.IsWhiteToMove && board.IsInCheck()) evalBlack -= 10;
            if (board.IsWhiteToMove && board.IsInCheckmate()) evalWhite -= 100000;
            if (!board.IsWhiteToMove && board.IsInCheckmate()) evalBlack -= 100000;
            return white ? evalWhite - evalBlack : evalBlack - evalWhite;
        }

        public (Move, float) minimaxTest(Board board, float depth)
        {
            Move[] moves = board.GetLegalMoves();
            if (depth == 0 || moves.Length == 0)
            {
                return (Move.NullMove, EvalPositionTest(board, board.IsWhiteToMove));
            }
            float maxValue = float.NegativeInfinity;
            Move bestMove = Move.NullMove;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                float e = -minimaxTest(board, depth - 1).Item2;
                board.UndoMove(move);
                if (e >= maxValue)
                {
                    bestMove = move;
                    maxValue = e;
                }
            }
            return (bestMove, maxValue);
        }
    }
    }