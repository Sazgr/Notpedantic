﻿using System.Numerics;
using LiteDB;
using Pedantic.Collections;
using Pedantic.Utilities;
using System.Runtime.CompilerServices;
using Pedantic.Genetics;

namespace Pedantic.Chess
{
    public sealed class EvalFeatures
    {
        // values required to determine phase and for mopup eval
        private readonly short fullMoveCounter;
        private readonly Color sideToMove;
        private readonly short[] material = new short[Constants.MAX_COLORS];
        private readonly sbyte[] kingIndex = new sbyte[Constants.MAX_COLORS];
        private readonly sbyte totalPawns;

        /*
         * Array/Vector of features (one per game phase)
         * [0]          # game phase material (not used in dot-product)
         * [1]          # pawns
         * [2]          # knights
         * [3]          # bishops
         * [4]          # rooks
         * [5]          # queens
         * [6]          # kings
         * [7 - 70]     0-1 pawn on square
         * [71 - 134]   0-1 knight on square
         * [135 - 198]  0-1 bishop on square
         * [199 - 262]  0-1 rook on square
         * [263 - 326]  0-1 queen on square
         * [327 - 390]  0-1 king on square
         * [391]        # knight mobility
         * [392]        # bishop mobility
         * [393]        # rook mobility
         * [394]        # queen mobility
         * [395]        # king mobility
         * [396 - 398]  # king attack (d0 - d2)
         * [399 - 401]  # pawn shield (d0 - d2)
         * [402]        # isolated pawns
         * [403]        # backward pawns
         * [404]        # doubled pawns
         * [405]        # connected/adjacent pawns
         * [406]        # passed pawns
         * [407]        # knights on outpost
         * [408]        # bishops on outpost
         * [409]        0-1 bishop pair
         * [410]        # rooks on open file
         */
        public const int FEATURE_SIZE = 411;
        public const int GAME_PHASE_BOUNDARY = 0;
        public const int MATERIAL = 1;
        public const int PIECE_SQUARE_TABLES = 7;
        public const int MOBILITY = 391;
        public const int KING_ATTACK = 396;
        public const int PAWN_SHIELD = 399;
        public const int ISOLATED_PAWNS = 402;
        public const int BACKWARD_PAWNS = 403;
        public const int DOUBLED_PAWNS = 404;
        public const int ADJACENT_PAWNS = 405;
        public const int PASSED_PAWNS = 406;
        public const int KNIGHTS_ON_OUTPOST = 407;
        public const int BISHOPS_ON_OUTPOST = 408;
        public const int BISHOP_PAIR = 409;
        public const int ROOK_OPEN_FILE = 410;


        private readonly SparseArray<short>[] sparse = { new(), new() };
		private readonly short[][] features = { Array.Empty<short>(), Array.Empty<short>() };
		private readonly int[][] indexMap = { Array.Empty<int>(), Array.Empty<int>() };
        private readonly short[][] openingWts = { Array.Empty<short>(), Array.Empty<short>() };
        private readonly short[][] endGameWts = { Array.Empty<short>(), Array.Empty<short>() };

        public EvalFeatures(Board bd)
        {
            short[] mobility = new short[Constants.MAX_PIECES];
            short[] kingAttacks = new short[3];

            fullMoveCounter = (short)bd.FullMoveCounter;
            totalPawns = (sbyte)BitOps.PopCount(bd.Pieces(Color.White, Piece.Pawn) | bd.Pieces(Color.Black, Piece.Pawn));
            sideToMove = bd.SideToMove;

            for (Color color = Color.White; color <= Color.Black; color++)
            {
                int c = (int)color;
                var v = sparse[c];

                for (int index = 0; index < Constants.MAX_SQUARES; index++)
                {
                    Square square = bd.PieceBoard[index];
                    int pstIndex = Index.NormalizedIndex[c][index];
                    if (!square.IsEmpty && square.Color == color)
                    {
                        IncrementPieceCount(v, square.Piece);
                        SetPieceSquare(v, square.Piece, pstIndex);
                    }
                }

                bd.GetPieceMobility(color, mobility, kingAttacks);
                for (Piece pc = Piece.Knight; pc <= Piece.King; pc++)
                {
                    int p = (int)pc;
                    if (mobility[p] > 0)
                    {
                        SetMobility(v, pc, mobility[p]);
                    }
                }

                for (int d = 0; d < 3; d++)
                {
                    if (kingAttacks[d] > 0)
                    {
                        SetKingAttack(v, d, kingAttacks[d]);
                    }
                }

                material[c] = bd.Material(color);
                kingIndex[c] = (sbyte)BitOps.TzCount(bd.Pieces(color, Piece.King));
                int ki = kingIndex[c];
                Color other = (Color)(c ^ 1);
                int o = (int)other;
                ulong pawns = bd.Pieces(color, Piece.Pawn);
                ulong otherPawns = bd.Pieces(other, Piece.Pawn);
                ulong myKing = bd.Pieces(color, Piece.King);

                for (ulong p = pawns; p != 0; p = BitOps.ResetLsb(p))
                {
                    int sq = BitOps.TzCount(p);

                    if ((otherPawns & Evaluation.PassedPawnMasks[c][sq]) == 0)
                    {
                        IncrementPassedPawns(v);
                    }

                    if ((pawns & Evaluation.IsolatedPawnMasks[sq]) == 0)
                    {
                        IncrementIsolatedPawns(v);
                    }

                    if ((pawns & Evaluation.BackwardPawnMasks[c][sq]) == 0)
                    {
                        IncrementBackwardPawns(v);
                    }

                    if ((pawns & Evaluation.AdjacentPawnMasks[sq]) != 0)
                    {
                        IncrementAdjacentPawns(v);
                    }
                }

                for (int file = 0; file < Constants.MAX_COORDS && pawns != 0; file++)
                {
                    short count = (short)BitOps.PopCount(pawns & Board.MaskFiles[file]);
                    if ( count > 1)
                    {
                        IncrementDoubledPawns(v, --count);
                    }
                }

                int bishopCount = BitOps.PopCount(bd.Pieces(color, Piece.Bishop));
                if (bishopCount >= 2)
                {
                    SetBishopPair(v);
                }

                ulong knights = bd.Pieces(color, Piece.Knight);
                for (ulong bb = knights; bb != 0; bb = BitOps.ResetLsb(bb))
                {
                    int sq = BitOps.TzCount(bb);
                    int normalRank = Index.GetRank(Index.NormalizedIndex[c][sq]);
                    if (normalRank > 3 && (Board.PawnDefends(color, sq) & pawns) != 0)
                    {
                        IncrementKnightsOnOutpost(v);
                    }
                }

                ulong bishops = bd.Pieces(color, Piece.Bishop);
                for (ulong bb = bishops; bb != 0; bb = BitOps.ResetLsb(bb))
                {
                    int sq = BitOps.TzCount(bb);
                    int normalRank = Index.GetRank(Index.NormalizedIndex[c][sq]);
                    if (normalRank > 3 && (Board.PawnDefends(color, sq) & pawns) != 0)
                    {
                        IncrementBishopsOnOutpost(v);
                    }
                }

                for (int d = 0; d < 3; d++)
                {
                    short count = (short)BitOps.PopCount(pawns & Evaluation.KingProximity[d][ki]);
                    if (count > 0)
                    {
                        SetPawnShield(v, d, count);
                    }
                }

                ulong allPawns = pawns | otherPawns;
                for (ulong bb = bd.Pieces(color, Piece.Rook); bb != 0; bb = BitOps.ResetLsb(bb))
                {
                    int sq = BitOps.TzCount(bb);
                    if ((Board.MaskFiles[sq] & allPawns) == 0)
                    {
                        IncrementRookOnOpenFile(v);
                    }
                }

				
				features[c] = new short[sparse[c].Count];
				indexMap[c] = new int[sparse[c].Count];
                openingWts[c] = new short[sparse[c].Count];
                endGameWts[c] = new short[sparse[c].Count];

				int i = 0;
				foreach (var kvp in sparse[c])
				{
					features[c][i] = kvp.Value;
					indexMap[c][i++] = kvp.Key;
				}
            }
        }

        public short Compute(ReadOnlySpan<short> opWeights, ReadOnlySpan<short> egWeights)
        {
            try
            {
                Span<short> opScore = stackalloc short[2];
                Span<short> egScore = stackalloc short[2];
                opScore.Clear();
                egScore.Clear();

                MapWeights(opWeights, egWeights);

                for (Color color = Color.White; color <= Color.Black; color++)
                {
                    int c = (int)color;
                    opScore[c] = DotProduct(features[c], openingWts[c]);
                    egScore[c] = DotProduct(features[c], endGameWts[c]);
                }

                GamePhase gamePhase = GetGamePhase(opWeights[GAME_PHASE_BOUNDARY],
                    egWeights[GAME_PHASE_BOUNDARY], out int opWt, out int egWt);

                short score = (short)((((opScore[0] - opScore[1]) * opWt) >> 7 /* / 128 */) +
                                      (((egScore[0] - egScore[1]) * egWt) >> 7 /* / 128 */));

                return sideToMove == Color.White ? score : (short)-score;
            }
            catch (Exception ex)
            {
                Util.TraceError(ex.ToString());
                throw new Exception("EvalFeatures.Compute error occurred.", ex);
            }
        }

        public Color SideToMove => sideToMove;

		private void MapWeights(ReadOnlySpan<short> opWeights, ReadOnlySpan<short> egWeights)
		{
			for (int c = 0; c < Constants.MAX_COLORS; c++)
			{
				for (int n = 0; n < indexMap[c].Length; n++)
				{
					openingWts[c][n] = opWeights[indexMap[c][n]];
                    endGameWts[c][n] = egWeights[indexMap[c][n]];
				}
			}
		}
        
        public static short GetOptimizationIncrement(int index)
        {
            const int eg = FEATURE_SIZE;
            return (short)(index == GAME_PHASE_BOUNDARY || index == (GAME_PHASE_BOUNDARY + eg) ? 50 : 1);
        }

        private short DotProduct(ReadOnlySpan<short> f, ReadOnlySpan<short> weights)
        {
            int results = 0;
            if (f.Length >= Vector<short>.Count)
            {
                int remaining = f.Length % Vector<short>.Count;

                for (int i = 0; i < f.Length - remaining; i += Vector<short>.Count)
                {
                    var v1 = new Vector<short>(f[i..]);
                    var v2 = new Vector<short>(weights[i..]);
                    results += Vector.Dot(v1, v2);
                }

                for (int i = f.Length - remaining; i < f.Length; i++)
                {
                    results += (short)(f[i] * weights[i]);
                }
            }
            else
            {
                for (int i = 0; i < f.Length; i++)
                {
                    results += (short)(f[i] * weights[i]);
                }
            }

            return (short)results;
        }

        private GamePhase GetGamePhase(short openingMaterial, short endGameMaterial, out int opWt, out int egWt)
        {
            GamePhase phase = GamePhase.Opening;
            opWt = 128;
            egWt = 0;
            int totalMaterial = material[0] + material[1];


            if (totalMaterial < endGameMaterial)
            {
                phase = totalPawns == 0 ? GamePhase.EndGameMopup : GamePhase.EndGame;
                opWt = 0;
                egWt = 128;
            }
            else if (totalMaterial < openingMaterial && totalMaterial >= endGameMaterial)
            {
                phase = GamePhase.MidGame;
                int rngMaterial = openingMaterial - endGameMaterial;
                int curMaterial = totalMaterial - endGameMaterial;
                opWt = (curMaterial * 128) / rngMaterial;
                egWt = 128 - opWt;
            }

            return phase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IncrementPieceCount(IDictionary<int, short> v, Piece piece)
        {
            if (piece == Piece.King)
            {
                return;
            }

            int index = MATERIAL + (int)piece;
            if (v.ContainsKey(index))
            {
                v[index]++;
            }
            else
            {
                v.Add(index, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetPieceSquare(IDictionary<int, short> v, Piece piece, int square)
        {
            int index = PIECE_SQUARE_TABLES + ((int)piece << 6) + square;
            v[index] = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetMobility(IDictionary<int, short> v, Piece piece, short mobility)
        {
            int p = (int)piece - 1;
            v[MOBILITY + p] = mobility;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetKingAttack(IDictionary<int, short> v, int d, short count)
        {
            v[KING_ATTACK + d] = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetPawnShield(IDictionary<int, short> v, int d, short count)
        {
            v[PAWN_SHIELD + d] = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IncrementIsolatedPawns(IDictionary<int, short> v)
        {
            if (v.ContainsKey(ISOLATED_PAWNS))
            {
                v[ISOLATED_PAWNS]++;
            }
            else
            {
                v.Add(ISOLATED_PAWNS, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IncrementBackwardPawns(IDictionary<int, short> v)
        {
            if (v.ContainsKey(BACKWARD_PAWNS))
            {
                v[BACKWARD_PAWNS]++;
            }
            else
            {
                v.Add(BACKWARD_PAWNS, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IncrementDoubledPawns(IDictionary<int, short> v, short count)
        {
            if (v.ContainsKey(DOUBLED_PAWNS))
            {
                v[DOUBLED_PAWNS] += count;
            }
            else
            {
                v.Add(DOUBLED_PAWNS, count);
            }
        }

        private static void IncrementPassedPawns(IDictionary<int, short> v)
        {
            if (v.ContainsKey(PASSED_PAWNS))
            {
                v[PASSED_PAWNS]++;
            }
            else
            {
                v.Add(PASSED_PAWNS, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IncrementAdjacentPawns(IDictionary<int, short> v)
        {
            if (v.ContainsKey(ADJACENT_PAWNS))
            {
                v[ADJACENT_PAWNS]++;
            }
            else
            {
                v.Add(ADJACENT_PAWNS, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IncrementKnightsOnOutpost(IDictionary<int, short> v)
        {
            if (v.ContainsKey(KNIGHTS_ON_OUTPOST))
            {
                v[KNIGHTS_ON_OUTPOST]++;
            }
            else
            {
                v.Add(KNIGHTS_ON_OUTPOST, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IncrementBishopsOnOutpost(IDictionary<int, short> v)
        {
            if (v.ContainsKey(BISHOPS_ON_OUTPOST))
            {
                v[BISHOPS_ON_OUTPOST]++;
            }
            else
            {
                v.Add(BISHOPS_ON_OUTPOST, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetBishopPair(IDictionary<int, short> v)
        {
            v[BISHOP_PAIR] = 1;
        }

        public static void IncrementRookOnOpenFile(IDictionary<int, short> v)
        {
            if (v.ContainsKey(ROOK_OPEN_FILE))
            {
                v[ROOK_OPEN_FILE]++;
            }
            else
            {
                v.Add(ROOK_OPEN_FILE, 1);
            }
        }
    }
}
