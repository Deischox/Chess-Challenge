using ChessChallenge.API;
using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using static ChessChallenge.Application.ConsoleHelper;

public class EvilBot4 : IChessBot
{
    public Move best;
    public Board board;
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
     {0,  0,  0,  0,  0,  0,  0,  0},
     {0,  2,  3,  4,  4,  2,  3,  0},
     {0,  4,  6, 10, 10,  6,  4,  0},
     {0,  6,  9, 10, 10,  9,  6,  0},
     {4,  8, 12, 16, 16, 12,  8,  4},
     {5, 10, 15, 20, 20, 15, 10,  5},
     {99,99, 99, 99, 99, 99, 99, 99}
    };

    // I created this, should be just a flipped
    // version of the white array
    /*
    private int[,] pawnposBlack = new int[,]
    {
     {99,99, 99, 99, 99, 99, 99, 99},
     {5, 10, 15, 20, 20, 15, 10,  5},
     {4,  8, 12, 16, 16, 12,  8,  4},
     {0,  6,  9, 10, 10,  9,  6,  0},
     {0,  4,  6, 10, 10,  0,  0,  0},
     {0,  2,  3,  4,  4,  0,  0,  0},
     {0,  0,  0,  0,  0,  0,  0,  0},
     {0,  0,  0,  0,  0,  0,  0,  0}
     };
    */

    public Move Think(Board board, Timer timer)
    {
        //TODO i think a problem is if BOT has M2 but enemy M1, then does the bot thinks he is winning
        /*
         * 1) Run Evaluation till all caputre moves are done
         * 2) Implement King Safty
         * 3) Implement EndGame
         * 4) Reduce to fit Size
         * 
         */

        test();
        this.board = board;
        Move bestMove = GetBestMove(board);
        return bestMove;
        //return Search(board,2,float.NegativeInfinity, float.PositiveInfinity).Item1;
    }

    void test()
    {
        Board newBoard = Board.CreateBoardFromFEN("3kr3/8/3p1p2/Q3p2q/3P1P2/8/4R3/6K1 w - - 0 1");
        this.board = newBoard;
        Move bestMove = GetBestMove(newBoard);
        Log(bestMove.ToString());

        Log(Evaluate().ToString());
        Log(CountMaterial(true).ToString());
        Log(CountMaterial(false).ToString());
    }

    public int getPieceValue(PieceType piece)
    {
        switch (piece)
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
        for (var i = 0; i < moves.Length; i++)
        {
            int moveScoreGuess = 0;
            PieceType movePieceType = board.GetPiece(moves[i].StartSquare).PieceType;
            PieceType capturePieceType = board.GetPiece(moves[i].TargetSquare).PieceType;

            if (capturePieceType != PieceType.None)
            {
                moveScoreGuess = -10 * getPieceValue(capturePieceType) + getPieceValue(movePieceType);
            }

            if (moves[i].IsPromotion)
            {
                moveScoreGuess -= getPieceValue(moves[i].PromotionPieceType);
            }
            /*board.MakeMove(moves[i]);
            if (board.IsInCheck()) moveScoreGuess = -10;
            board.UndoMove(moves[i]);*/
            moveScore[i] = moveScoreGuess;
        }
        Array.Sort(moveScore, moves);
        return moves;
    }

    public int Evaluate()
    {
        int whiteMaterial = CountMaterial(true);
        int blackMaterial = CountMaterial(false);

        if (!board.IsWhiteToMove && board.IsInCheck()) blackMaterial -= 50;
        if (board.IsWhiteToMove && board.IsInCheck()) whiteMaterial -= 50;
        if (!board.IsWhiteToMove && board.IsInCheckmate())
        {
            blackMaterial = -10000;
        }
        if (board.IsWhiteToMove && board.IsInCheckmate())
        {
            whiteMaterial = -10000;
        }
        int eval = whiteMaterial - blackMaterial;
        return board.IsWhiteToMove ? eval : -eval;


    }

    int CountMaterial(bool white)
    {
        int material = 0;
        //material += board.GetPieceList(PieceType.Pawn, white).Count * getPieceValue(PieceType.Pawn);
        foreach (Piece pawn in board.GetPieceList(PieceType.Pawn, white))
        {
            if (white) material += getPieceValue(PieceType.Pawn) + pawnposWhite[pawn.Square.Rank, pawn.Square.File];
            if (!white) material += getPieceValue(PieceType.Pawn) + pawnposWhite[7 - pawn.Square.Rank, pawn.Square.File];
        }
        foreach (Piece pawn in board.GetPieceList(PieceType.Knight, white))
        {
            material += getPieceValue(PieceType.Knight) + knightpos[pawn.Square.Rank, pawn.Square.File];
        }
        foreach (Piece pawn in board.GetPieceList(PieceType.Bishop, white))
        {
            material += getPieceValue(PieceType.Bishop) + bishoppos[pawn.Square.Rank, pawn.Square.File];
        }
        //material += board.GetPieceList(PieceType.Bishop, white).Count * getPieceValue(PieceType.Bishop);
        //material += board.GetPieceList(PieceType.Knight, white).Count * getPieceValue(PieceType.Knight);
        material += board.GetPieceList(PieceType.Rook, white).Count * getPieceValue(PieceType.Rook);
        material += board.GetPieceList(PieceType.Queen, white).Count * getPieceValue(PieceType.Queen);

        return material;
    }


    /*public int Search(int depth)
    {
        Move[] moves = OrderMoves(board, board.GetLegalMoves());
        if(depth == 0)
        {
            return Evaluate();
        }
        if(moves.Length == 0)
        {
            if (board.IsInCheck())
            {
                return int.MinValue;
            }
            return 0;
        }
        int maxValue = int.MinValue;
        foreach(Move move in moves)
        {
            board.MakeMove(move);
            int eval = -Search(depth - 1);
            board.UndoMove(move);
            maxValue = Math.Max(maxValue, eval);
        }
        return maxValue;
        
    }*/

    public int SearchCaptures(int alpha, int beta)
    {
        int eval = Evaluate();
        if (eval >= beta) return beta;
        alpha = Math.Max(alpha, eval);

        Move[] moves = board.GetLegalMoves(true);

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            eval = -SearchCaptures(-beta, -alpha);
            board.UndoMove(move);

            if (eval >= beta) return beta;
            alpha = Math.Max(alpha, eval);
        }
        return alpha;
    }

    public int Search(int depth, int alpha, int beta)
    {

        if (depth == 0)
        {
            return SearchCaptures(alpha, beta);
        }

        Move[] moves = OrderMoves(board, board.GetLegalMoves());
        if (moves.Length == 0)
        {
            if (board.IsInCheck())
            {
                return -10000 - depth * 1000;
            }
            return 0;
        }

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha);
            board.UndoMove(move);
            if (eval >= beta) return beta;
            alpha = Math.Max(eval, alpha);
        }
        return alpha;

    }

    public int Search(int depth, int alpha, int beta, bool maximize)
    {
        Move[] moves = OrderMoves(board, board.GetLegalMoves());
        if (depth == 0)
        {
            return Evaluate();
        }
        if (moves.Length == 0)
        {
            if (board.IsInCheck())
            {
                return -10000 - depth * 1000;
            }
            return 0;
        }
        if (maximize)
        {
            int maxEval = int.MinValue;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int eval = 0;
                eval = Search(depth - 1, alpha, beta, false);
                board.UndoMove(move);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return maxEval;

        }
        else
        {
            int minVal = int.MaxValue;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int eval = 0;
                //if (board.IsRepeatedPosition()) eval = 100000;
                eval = Search(depth - 1, alpha, beta, true);
                board.UndoMove(move);
                minVal = Math.Min(minVal, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return minVal;
        }

    }

    // Helper function to evaluate all capture moves
    private Move GetBestMove(Board board)
    {
        float bestValue = float.MinValue;
        Move bestMove = Move.NullMove;

        Move[] moves = OrderMoves(board, board.GetLegalMoves());
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int value = 0;
            if (board.IsRepeatedPosition()) value = -100000000;
            else value = -Search(4, int.MinValue, int.MaxValue, true);

            board.UndoMove(move);
            //TODO Problem is that M4 equally good to M1 is 
            if (value >= bestValue)
            {
                bestValue = value;
                bestMove = move;
            }
        }
        return bestMove;

    }
}