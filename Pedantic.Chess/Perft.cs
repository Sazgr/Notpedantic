﻿// ***********************************************************************
// Assembly         : Pedantic.Chess
// Author           : JoAnn D. Peeler
// Created          : 01-17-2023
//
// Last Modified By : JoAnn D. Peeler
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="Perft.cs" company="Pedantic.Chess">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary>
//     Class that implements the standard Perft test with a couple of 
//     variants. 
// </summary>
// ***********************************************************************

using Pedantic.Collections;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public sealed class Perft
    {
        private readonly Board board = new(Constants.FEN_START_POS);
        private readonly ObjectPool<MoveList> moveListPool = new(Constants.MAX_PLY, 10);

        public struct Counts
        {
            public uint Nodes ;
            public uint Captures;
            public uint EnPassants; 
            public uint Castles;
            public uint Checks;
            public uint Checkmates;
            public uint Promotions;

            public Counts()
            {
                Nodes = 0;
                Captures = 0;
                EnPassants = 0;
                Castles = 0;
                Checks = 0;
                Checkmates = 0;
                Promotions = 0;
            }

            public static Counts Default { get; } = new();

            public static Counts operator +(Counts c1, Counts c2)
            {
                Counts counts;
                counts.Nodes = c1.Nodes + c2.Nodes;
                counts.Captures = c1.Captures + c2.Captures;
                counts.EnPassants = c1.EnPassants + c2.EnPassants;
                counts.Castles = c1.Castles + c2.Castles;
                counts.Checks = c1.Checks + c2.Checks;
                counts.Checkmates = c1.Checkmates + c2.Checkmates;
                counts.Promotions = c1.Promotions + c2.Promotions;
                return counts;
            }
        }

        public struct DivideCount
        {
            public ulong Move;
            public ulong Count;

            public DivideCount(ulong move)
            {
                Move = move;
                Count = 0;
            }

            public ulong Sum(ulong count)
            {
                Count += count;
                return Count;
            }

        }

        public Perft(string? startingPosition = null)
        {
            board.LoadFenPosition(startingPosition ?? Constants.FEN_START_POS);
        }

        public void Initialize(string fen = Constants.FEN_START_POS)
        {
            board.LoadFenPosition(fen);
        }

        public ulong Divide(int depth, out DivideCount[] counts)
        {
            counts = Array.Empty<DivideCount>();

            if (depth == 0)
            {
                return 1;
            }

            ValueList<DivideCount> divCounts = new(Constants.MAX_PLY);

            ulong nodes = 0;
            MoveList moveList = moveListPool.Rent();
            board.PushBoardState();
            board.GenerateMoves(moveList);
            ReadOnlySpan<ulong> moves = moveList.ToSpan();
            for (int n = 0; n < moves.Length; ++n)
            {
                ulong move = moves[n];
                if (!board.MakeMoveNs(move))
                {
                    continue;
                }

                DivideCount divCnt = new(move);
                if (depth == 1)
                {
                    divCnt.Sum(1);
                    nodes++;
                }
                else
                {
                    ulong result = Execute(depth - 1);
                    nodes += result;
                    divCnt.Sum(result);
                }

                divCounts.Add(divCnt);

                board.UnmakeMoveNs();
            }

            moveListPool.Return(moveList);
            board.PopBoardState();
            counts = divCounts.ToArray();
            return nodes;
        }

        public ulong Execute(int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            ulong nodes = 0;
            MoveList moveList = moveListPool.Rent();
            board.PushBoardState();
            board.GenerateMoves(moveList);

            ReadOnlySpan<ulong> moves = moveList.ToSpan();
            for (int n = 0; n < moves.Length; ++n)
            {
                if (!board.MakeMoveNs(moves[n]))
                {
                    continue;
                }

                if (depth > 1)
                {
                    nodes += Execute(depth - 1);
                }
                else
                {
                    nodes++;
                }

                board.UnmakeMoveNs();
            }

            moveListPool.Return(moveList);
            board.PopBoardState();
            return nodes;
        }

        public Counts ExecuteWithDetails(int depth)
        {
            Counts counts = Counts.Default;

            if (depth == 0)
            {
                return GetCounts();
            }

            MoveList moveList = moveListPool.Rent();
            board.PushBoardState();
            board.GenerateMoves(moveList);
            var moves = moveList.ToSpan();

            for (int n = 0; n < moves.Length; ++n)
            {
                if (!board.MakeMoveNs(moves[n]))
                {
                    continue;
                }

                counts += ExecuteWithDetails(depth - 1);
                board.UnmakeMoveNs();
            }

            moveListPool.Return(moveList);
            board.PopBoardState();
            return counts;
        }

        private Counts GetCounts()
        {
            Counts counts = Counts.Default;
            counts.Nodes = 1;
            bool inCheck = board.IsChecked();
            bool mate = false;
            if (inCheck)
            {
                counts.Checks = 1;
                mate = true;
                MoveList moveList = moveListPool.Rent();
                board.PushBoardState();
                board.GenerateMoves(moveList);
                var moves = moveList.ToSpan();

                for (int n = 0; n < moves.Length; ++n)
                {
                    if (!board.MakeMoveNs(moves[n]))
                    {
                        continue;
                    }

                    board.UnmakeMoveNs();
                    mate = false;
                    break;
                }

                moveListPool.Return(moveList);
                board.PopBoardState();
            }

            if (mate && inCheck)
            {
                counts.Checkmates = 1;
            }

            MoveType type = Move.GetMoveType(board.LastMove);
            switch (type)
            {
                case MoveType.Capture:
                    counts.Captures = 1;
                    break;

                case MoveType.EnPassant:
                    counts.Captures = 1;
                    counts.EnPassants = 1;
                    break;

                case MoveType.Castle:
                    counts.Castles = 1;
                    break;

                case MoveType.Promote:
                    counts.Promotions = 1;
                    break;

                case MoveType.PromoteCapture:
                    counts.Captures = 1;
                    counts.Promotions = 1;
                    break;
            }

            return counts;
        }
    }
}
