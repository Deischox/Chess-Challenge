using ChessChallenge.API;
using System;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using static ChessChallenge.Application.ConsoleHelper;

public class MyBot3 : IChessBot
{
    public Move best;
    public int searchAmount = 0;
    public float[] moveScore;

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
     {0,  2,  3,  4,  4,  -5,  -5,  -5},
     {0,  4,  6, 10, 10,  -5,  -5,  -5},
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
     {0,  4,  6, 10, 10,  0,  0,  0},
     {0,  2,  3,  4,  4,  0,  0,  0},
     {0,  0,  0, -5, -5,  10,  10,  10},
     {0,  0,  0,  0,  0,  0,  0,  0}
     };


    public void visualize(int[,] type, Board board)
    {
        for(var i = 0 ; i < 8; i++)
        {
            var s = "";
            for(var j = 7; j >= 0; j--)
            {
                if(board.GetPiece(new Square(j,i)).PieceType == PieceType.Pawn){
                    s += " " +type[i, j].ToString() + " ";
                }
                else
                {
                    s += " 0 ";
                }
            }
            Log(s);
        }
        Log("------------------------------------");
    }
    public Move Think(Board board, Timer timer)
    {
        //test();
        Move bestMove = alphaBeta(board, 4, float.NegativeInfinity, float.PositiveInfinity, true).Item1;
        Log(searchAmount.ToString() + " alphaBeta" + bestMove.ToString() + ": " + board.IsRepeatedPosition());
        Log("-------------");
        searchAmount = 0;
        
        return bestMove;
        //return Search(board,2,float.NegativeInfinity, float.PositiveInfinity).Item1;
    }

    public void test()
    {
        Board board = Board.CreateBoardFromFEN("r3k2r/p1ppqpb1/Bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPB1PPP/R3K2R b - - 0 1");
        Move bestMove = minimaxTest(board, 4).Item1;
        Log(searchAmount.ToString() + " minimax " + bestMove.ToString());
        searchAmount = 0;
        bestMove = alphaBeta(board, 4, float.NegativeInfinity, float.PositiveInfinity, true).Item1;
        Log(searchAmount.ToString() + " alphaBeta" + bestMove.ToString());
    }

    public int getPieceValue(PieceType piece)
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
        moveScore = new float[moves.Length];
        for(var i = 0; i < moves.Length; i++)
        {
            int moveScoreGuess = 0;
            PieceType movePieceType = board.GetPiece(moves[i].StartSquare).PieceType;
            PieceType capturePieceType = board.GetPiece(moves[i].TargetSquare).PieceType;

            if(capturePieceType != PieceType.None)
            {
                moveScoreGuess = -10 * getPieceValue(capturePieceType) + getPieceValue(movePieceType);
            }

            if (moves[i].IsPromotion)
            {
                moveScoreGuess -= getPieceValue(moves[i].PromotionPieceType);
            }
            moveScore[i] = moveScoreGuess;
        }
        Array.Sort(moveScore, moves);
        return moves;
    }
    
    
    
    public float evalPosition(Board board, bool white)
    {
        float eval = 0;
        if ((board.IsWhiteToMove == white) && board.IsInCheckmate()) return -10000;
        eval += board.GetPieceList(PieceType.Queen, white).Count * 900;
        eval += board.GetPieceList(PieceType.Rook, white).Count * 500;
        foreach (Piece piece in board.GetPieceList(PieceType.Pawn, white))
        {
            if(white) eval += 100 + 1 * pawnposWhite[piece.Square.Rank, piece.Square.File];
            else eval += 100 + 1 * pawnposBlack[piece.Square.Rank, piece.Square.File];
        }
        foreach (Piece piece in board.GetPieceList(PieceType.Knight, white))
        {
            eval += 300 + 1 * knightpos[piece.Square.Rank, piece.Square.File];
        }
        foreach (Piece piece in board.GetPieceList(PieceType.Bishop, white))
        {
            eval += 300 + 1 * bishoppos[piece.Square.Rank, piece.Square.File];
        }
        if ((board.IsWhiteToMove == white) && board.IsInCheck()) eval -= 100;
        Square square = new Square(6,0);
        Square squareBlack = new Square(6, 7);
        if (board.GetKingSquare(white) == square || board.GetKingSquare(white) == squareBlack) eval += 10;
        return eval;
    }

    public float EvalPositionTest(Board board, bool white)
    {
        float evalWhite = evalPosition(board, true);
        float evalBlack = evalPosition(board, false);
        float eval =  white ? evalWhite-evalBlack : evalBlack - evalWhite;
        if (board.IsRepeatedPosition())
        {
            return -1000000;
        }
        return eval;
    }

    public (Move,float) minimaxTest(Board board, float depth) {
        Move[] moves = board.GetLegalMoves();
        if(depth == 0 || moves.Length == 0)
        {
            return (Move.NullMove, EvalPositionTest(board, board.IsWhiteToMove));
            
        }
        Move bestMove = Move.NullMove;
        float maxValue = float.NegativeInfinity;
        foreach(Move move in moves)
        {
            searchAmount += 1;
            board.MakeMove(move);
            float e = -minimaxTest(board, depth-1).Item2;
            board.UndoMove(move);
            if (e > maxValue)
            {
                maxValue = e;
                bestMove = move;
            }
        }
        return (bestMove, maxValue);
    }

    public (Move,float) alphaBeta(Board board, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        Move[] moves = OrderMoves(board, board.GetLegalMoves());
        Move bestMove = Move.NullMove;
        if (depth == 0 || moves.Length == 0)
        {
            return (Move.NullMove, EvalPositionTest(board, board.IsWhiteToMove));
            //return (Move.NullMove, SearchAllCaptures(board, alpha, beta, maximizingPlayer));
            
        }
        if (maximizingPlayer)
        {
            float hValue = float.MinValue;

            foreach(Move move in moves)
            {
                searchAmount += 1;
                board.MakeMove(move);
                float e = alphaBeta(board, depth - 1, alpha, beta, !maximizingPlayer).Item2;
                board.UndoMove(move);

                if(e >= hValue)
                {
                    hValue = e;
                    bestMove = move;    
                }
                if(hValue > beta)
                {
                    //Maybe return alpha
                    return (bestMove, hValue);
                }
                alpha = Math.Max(alpha, hValue);

            }
            return (bestMove, hValue);
        }
        else
        {
            float hValue = float.MaxValue;

            foreach (Move move in moves)
            {
                searchAmount += 1;
                board.MakeMove(move);
                float e = alphaBeta(board, depth - 1, alpha, beta, !maximizingPlayer).Item2;
                //if (board.IsRepeatedPosition()) e -= 10000;
                board.UndoMove(move);

                if (e <= hValue)
                {
                    hValue = e;
                    //bestMove = move;
                }
                if (hValue < alpha)
                {
                    //Maybe return alpha
                    return (bestMove, hValue);
                }
                beta = Math.Min(alpha, hValue);

            }
            return (bestMove, hValue);

        }
    }

    // Helper function to evaluate all capture moves
    private float SearchAllCaptures(Board board, float alpha, float beta, bool maximizingPlayer)
    {
        Move[] captures = OrderMoves(board, board.GetLegalMoves(true)); // Implement a function to get all capture moves
        if (captures.Length == 0)
        {
            // No captures available, return the current board evaluation
            return EvalPositionTest(board, board.IsWhiteToMove); // Implement your position evaluation function
        }

        float hValue;
        if (maximizingPlayer)
        {
            hValue = float.MinValue;
            foreach (Move capture in captures)
            {
                board.MakeMove(capture);
                float e = SearchAllCaptures(board, alpha, beta, !maximizingPlayer);
                board.UndoMove(capture);
                hValue = Math.Max(hValue, e);
                alpha = Math.Max(alpha, hValue);
                if (hValue >= beta)
                {
                    // Prune the search
                    return beta;
                }
            }
        }
        else
        {
            hValue = float.MaxValue;
            foreach (Move capture in captures)
            {
                board.MakeMove(capture);
                float e = SearchAllCaptures(board, alpha, beta, !maximizingPlayer);
                board.UndoMove(capture);
                hValue = Math.Min(hValue, e);
                beta = Math.Min(beta, hValue);
                if (hValue <= alpha)
                {
                    // Prune the search
                    return alpha;
                }
            }
        }
        return hValue;
    }
}