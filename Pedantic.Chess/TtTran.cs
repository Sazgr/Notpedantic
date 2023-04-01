﻿// ***********************************************************************
// Assembly         : Pedantic.Chess
// Author           : JoAnn D. Peeler
// Created          : 01-17-2023
//
// Last Modified By : JoAnn D. Peeler
// Last Modified On : 03-26-2023
// ***********************************************************************
// <copyright file="TtTran.cs" company="Pedantic.Chess">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary>
//     A transposition table dedicated to search. 
// </summary>
// ***********************************************************************
using System.Runtime.CompilerServices;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public static class TtTran
    {
        public enum TtFlag : byte
        {
            Exact,
            UpperBound,
            LowerBound
        }

        public const int DEFAULT_SIZE_MB = 64;
        public const int MAX_SIZE_MB = 2048;
        public const int ITEM_SIZE = 16;
        public const int MB_SIZE = 1024 * 1024;
        public const int CAPACITY_MULTIPLIER = MB_SIZE / ITEM_SIZE;

        public struct TtTranItem
        {
            private ulong hash;
            private ulong data;

            public TtTranItem(ulong hash, short score, sbyte depth, byte age, TtFlag ttFlag, ulong bestMove)
            {
                data = (bestMove & 0x0fffffful) |
                       (((ulong)score & 0x0fffful) << 24) |
                       (((ulong)ttFlag & 0x03ul) << 40) |
                       (((byte)depth & 0x0fful) << 42) |
                       (((byte)age & 0x0fful) << 50);

                this.hash = hash ^ data;
            }

            public ulong Hash => hash ^ data;
            public ulong Data => data;
            public ulong BestMove => (ulong)BitOps.BitFieldExtract(data, 0, 24);
            public short Score => (short)BitOps.BitFieldExtract(data, 24, 16);
            public TtFlag Flag => (TtFlag)BitOps.BitFieldExtract(data, 40, 2);
            public sbyte Depth => (sbyte)BitOps.BitFieldExtract(data, 42, 8);
            public byte Age
            {
                get => (byte) BitOps.BitFieldExtract(data, 50, 8);
                set => BitOps.BitFieldSet(data, value, 50, 8);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsValid(ulong hash)
            {
                return (this.hash ^ data)  == hash;
            }

            public static void SetValue(ref TtTranItem item, ulong hash, short score, sbyte depth, byte age, 
                TtFlag flag, ulong bestMove)
            {
                item.data = (Move.ClearScore(bestMove)) |
                            (((ulong)score & 0x0fffful) << 24) |
                            (((ulong)flag & 0x03ul) << 40) |
                            (((byte)depth & 0x0fful) << 42) |
                            (((byte)age & 0x0fful) << 50);
                item.hash = hash ^ item.data;
            }
        }


        private static TtTranItem[] table;
        private static int capacity;
        private static uint mask;

        static TtTran()
        {
            capacity = DEFAULT_SIZE_MB * CAPACITY_MULTIPLIER;
            table = new TtTranItem[capacity];
            mask = (uint)(capacity - 1);
        }

        public static void Add(ulong hash, int depth, int ply, int alpha, int beta, int score, ulong move)
        {
            int index = GetStoreIndex(hash);
            ref TtTranItem item = ref table[index];
            ulong bestMove = move;

            if (item.IsValid(hash))
            {
                if (item.Depth > depth)
                {
                    return; // don't replace
                }

                bestMove = bestMove == 0 ? item.BestMove : bestMove;
            }

            if (Evaluation.IsCheckmate(score))
            {
                score += Math.Sign(score) * ply;
            }

            sbyte itemDepth = (sbyte)depth;
            TtFlag flag = TtFlag.Exact;

            if (score <= alpha)
            {
                flag = TtFlag.UpperBound;
            }
            else if (score >= beta)
            {
                flag = TtFlag.LowerBound;
            }

            TtTranItem.SetValue(ref item, hash, (short)score, itemDepth, 0, flag, bestMove);
        }

        public static void Clear()
        {
            Array.Clear(table);
        }

        public static void Resize(int sizeMb)
        {
            if (!BitOps.IsPow2(sizeMb))
            {
                sizeMb = BitOps.GreatestPowerOfTwoLessThan(sizeMb);
            }
            // resizing also clears the hash table. No attempt to rehash.
            capacity = Math.Min(sizeMb, MAX_SIZE_MB) * CAPACITY_MULTIPLIER;
            table = new TtTranItem[capacity];
            mask = (uint)(capacity - 1);
        }

        public static int Capacity => capacity;

        public static bool TryGetBestMove(ulong hash, out ulong bestMove)
        {
            bestMove = 0ul;
            if (GetLoadIndex(hash, out int index))
            {
                bestMove = table[index].BestMove;
            }

            return bestMove != 0;
            
        }

        public static bool TryLookup(ulong hash, int depth, out TtTranItem item)
        {
            if (GetLoadIndex(hash, out int index) && table[index].Depth > depth)
            {
                item = table[index];
                return true;
            }

            item = default;
            return false;
        }

        public static TtTranItem? Lookup(ulong hash)
        {
            if (GetLoadIndex(hash, out int index))
            {
                return table[index];
            }
            return null;
        }

        public static bool TryGetScore(ulong hash, int depth, int ply, int alpha, int beta, out int score)
        {
            return TryGetScore(hash, depth, ply, ref alpha, ref beta, out score, out ulong _);
        }

        public static bool TryGetScore(ulong hash, int depth, int ply, ref int alpha, ref int beta, out int score,
            out ulong move)
        {
            score = 0;
            move = 0;

            if (GetLoadIndex(hash, out int index))
            {
                ref TtTranItem item = ref table[index];
                move = item.BestMove;

                if (item.Depth <= depth)
                {
                    return false;
                }

                score = item.Score;

                if (Evaluation.IsCheckmate(score))
                {
                    score -= Math.Sign(score) * ply;
                }

                if (item.Flag == TtFlag.Exact)
                {
                    return true;
                }

                if (item.Flag == TtFlag.UpperBound /*&& score <= alpha*/)
                {
                    beta = Math.Min(score, beta);
                }

                if (item.Flag == TtFlag.LowerBound /*&& score >= beta*/)
                {
                    alpha = Math.Max(score, alpha);
                }

                return alpha >= beta;
            }

            return false;
        }

        private static int GetStoreIndex(ulong hash)
        {
            int index = (int)(hash & mask);
            ref TtTranItem item0 = ref table[index];
            ref TtTranItem item1 = ref table[index ^ 1];

            if (item0.IsValid(hash))
            {
                return index;
            }

            if (item1.IsValid(hash))
            {
                return index ^ 1;
            }

            return (++item0.Age - item0.Depth) > (++item1.Age - item1.Depth) ? index : index ^ 1;
        }

        private static bool GetLoadIndex(ulong hash, out int index)
        {
            index = (int)(hash & mask);
            if (!table[index].IsValid(hash))
            {
                index ^= 1;

                if (!table[index].IsValid(hash))
                {
                    return false;
                }
            }

            table[index].Age = 0;
            return true;
        }
    }
}
