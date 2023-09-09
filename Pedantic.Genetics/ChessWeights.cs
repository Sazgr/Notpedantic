﻿// ***********************************************************************
// Assembly         : Pedantic.Genetics
// Author           : JoAnn D. Peeler
// Created          : 03-12-2023
//
// Last Modified By : JoAnn D. Peeler
// Last Modified On : 03-28-2023
// ***********************************************************************
// <copyright file="ChessWeights.cs" company="Pedantic.Genetics">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary>
//     Mapped POCO class for maintaining the weights in the LiteDB 
//     database.
// </summary>
// ***********************************************************************
using Pedantic.Utilities;

namespace Pedantic.Genetics
{
    public sealed class ChessWeights
    {
        public static readonly Guid DEFAULT_IMMORTAL_ID = new("da5e310e-b0dc-4c77-902c-5a46cc81bb73");
        public const int MAX_WEIGHTS = 3750;
        public const int ENDGAME_WEIGHTS = 1875;
        public const int PIECE_WEIGHT_LENGTH = 6;
        public const int PIECE_SQUARE_LENGTH = 1536;
        public const int PIECE_VALUES = 0;
        public const int PIECE_SQUARE_TABLE = 6;
        public const int PIECE_MOBILITY = 1542;
        public const int KING_ATTACK = 1546;
        public const int PAWN_SHIELD = 1549;
        public const int ISOLATED_PAWN = 1552;
        public const int BACKWARD_PAWN = 1553;
        public const int DOUBLED_PAWN = 1554;
        public const int CONNECTED_PAWN = 1555;
        public const int KNIGHT_OUTPOST = 1619;
        public const int BISHOP_OUTPOST = 1620;
        public const int BISHOP_PAIR = 1621;
        public const int ROOK_ON_OPEN_FILE = 1622;
        public const int ROOK_ON_HALF_OPEN_FILE = 1623;
        public const int ROOK_BEHIND_PASSED_PAWN = 1624;
        public const int DOUBLED_ROOKS_ON_FILE = 1625;
        public const int KING_ON_OPEN_FILE = 1626;
        public const int KING_ON_HALF_OPEN_FILE = 1627;
        public const int CASTLING_AVAILABLE = 1628;
        public const int CASTLING_COMPLETE = 1629;
        public const int CENTER_CONTROL = 1630;
        public const int QUEEN_ON_OPEN_FILE = 1632;
        public const int QUEEN_ON_HALF_OPEN_FILE = 1633;
        public const int ROOK_ON_7TH_RANK = 1634;
        public const int PASSED_PAWN = 1635;
        public const int BAD_BISHOP_PAWN = 1699;
        public const int BLOCK_PASSED_PAWN = 1763;
        public const int SUPPORTED_PAWN = 1811;

        public ChessWeights(Guid _id, bool isActive, bool isImmortal, string description, short[] weights, 
            float fitness, int sampleSize, float k, short totalPasses, DateTime updatedOn, DateTime createdOn)
        {
            Id = _id;
            IsActive = isActive;
            IsImmortal = isImmortal;
            Description = description;
            Weights = weights;
            Fitness = fitness;
            SampleSize = sampleSize;
            K = k;
            TotalPasses = totalPasses;
            UpdatedOn = updatedOn;
            CreatedOn = createdOn;
        }

        public ChessWeights(ChessWeights other)
        {
            Id = Guid.NewGuid();
            IsActive = other.IsActive;
            IsImmortal = other.IsImmortal;
            Description = other.Description;
            Weights = ArrayEx.Clone(other.Weights);
            Fitness = other.Fitness;
            SampleSize = other.SampleSize;
            K = other.K;
            TotalPasses = other.TotalPasses;
            UpdatedOn = DateTime.UtcNow;
            CreatedOn = DateTime.UtcNow;
        }

        public ChessWeights(short[] weights)
        {
            Id = Guid.NewGuid();
            IsActive = true;
            IsImmortal = false;
            Description = "Anonymous";
            Weights = ArrayEx.Clone(weights);
            Fitness = 0;
            SampleSize = 0;
            K = 0;
            TotalPasses = 0;
            UpdatedOn = DateTime.UtcNow;
            CreatedOn = DateTime.UtcNow;
        }

        public ChessWeights()
        {
            Id = Guid.NewGuid();
            IsActive = false;
            IsImmortal = false;
            Description = string.Empty;
            Weights = Array.Empty<short>();
            Fitness = default;
            SampleSize = default;
            K = default;
            TotalPasses = default;
            UpdatedOn = DateTime.UtcNow;
            CreatedOn = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsImmortal { get; set; }
        public string Description { get; set; }
        public short[] Weights { get; init; }
        public float Fitness { get; set; }
        public int SampleSize { get; set; }
        public float K { get; set; }
        public short TotalPasses { get; set; }
        public DateTime UpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public static ChessWeights Empty { get; } = new ChessWeights();

        public static ChessWeights CreateParagon()
        {
            ChessWeights paragon = new(paragonWeights)
            {
                Id = DEFAULT_IMMORTAL_ID,
                IsImmortal = true,
                Fitness = 0,
                SampleSize = 0,
                K = 0,
                TotalPasses = 0,
                UpdatedOn = DateTime.UtcNow
            };
            return paragon;
        }

        public static bool LoadParagon(out ChessWeights paragon)
        {
            var rep = new ChessDb();
            ChessWeights? p = rep.Weights.GetAll().FirstOrDefault(w => w.IsActive && w.IsImmortal);
            if (p == null)
            {
                p = CreateParagon();
                rep.Weights.Insert(p);
                rep.Save();
                paragon = p;
            }
            else
            {
                paragon = p;
            }
            return true;
        }

        // Solution sample size: 12000000, generated on Fri, 08 Sep 2023 20:20:01 GMT
        // Solution error: 0.119095, accuracy: 0.5160, seed: -1567230396
        private static readonly short[] paragonWeights =
        {
            /*------------------- OPENING WEIGHTS -------------------*/

            /* opening piece values */
            93, 437, 488, 591, 1439, 0,

            /* opening piece square values */

            #region opening piece square values

            /* pawns: KK */
               0,    0,    0,    0,    0,    0,    0,    0,
             -22,  -28,  -23,   -7,  -32,   51,   43,   -8,
             -25,  -23,  -28,  -34,  -18,   -5,   12,  -29,
              -4,  -11,    6,   -9,   21,   30,   13,    8,
               5,   -4,    2,    7,   31,   38,   33,   31,
              -3,   -2,   11,   55,   50,  143,  137,   67,
             107,  147,   85,  117,  120,   69,   51,   45,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* pawns: KQ */
               0,    0,    0,    0,    0,    0,    0,    0,
             -47,  -58,  -42,  -57,  -23,   41,   46,    7,
             -37,  -55,  -41,  -48,  -11,   -9,   19,   -2,
             -14,    1,   -1,   -7,    4,   -4,    3,   33,
              -1,   25,   17,   13,   -2,  -20,   -7,   33,
              66,   90,  105,   53,   57,   22,   15,    4,
              51,   32,   52,   89,  120,  110,  189,  160,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* pawns: QK */
               0,    0,    0,    0,    0,    0,    0,    0,
               8,   34,   18,  -25,  -32,  -14,  -27,  -42,
              22,   33,  -13,  -24,  -26,  -16,  -38,  -61,
              25,   22,    9,   -7,    0,    5,    0,  -12,
              14,   -9,   -2,    6,   25,   25,   14,    7,
              12,   16,   11,   37,   63,  106,   93,   22,
             128,  152,  142,  158,   94,   46,   91,   16,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* pawns: QQ */
               0,    0,    0,    0,    0,    0,    0,    0,
             -28,    7,   36,  -15,  -20,    2,  -17,   -8,
             -23,   -8,  -14,  -39,  -15,  -28,  -34,  -33,
               1,  -10,   27,   -4,   17,   19,    0,   -8,
              43,   68,   40,   24,    2,    2,   -8,   27,
              82,   42,   68,   66,   -4,    9,  -11,    6,
              54,   53,   75,   52,  141,   46,  177,  125,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* knights: KK */
            -112,  -17,  -39,  -26,   -3,    0,  -11,  -55,
             -35,  -37,   -5,   12,   11,   29,  -10,   -3,
             -21,   -9,   -2,   14,   30,    2,    6,   -4,
              -8,    3,   12,   16,   24,   30,   35,   11,
              -2,   -1,   24,   40,   -5,   15,  -23,   50,
             -26,   35,   37,   50,  103,  155,   30,  -14,
             -10,  -16,   21,   72,   42,   66,   39,   -4,
            -208,    4,  -20,   54,   48,  -65,  -25, -164,

            /* knights: KQ */
             -63,  -41,  -36,  -43,  -37,  -25,  -26,  -33,
             -60,  -24,   -2,   16,   -9,   -1,  -57,  -46,
             -40,  -11,  -11,   14,   24,   11,  -24,  -22,
               5,   28,   29,   49,   32,    5,   13,  -10,
              30,   -1,   22,   30,   64,   39,   10,    8,
              46,   44,   90,   64,   60,   65,   28,    9,
              22,  -10,   16,   34,   32,   43,   -9,  -13,
            -245,   -1,  -10,   22,  -38,    0,  -35,  -87,

            /* knights: QK */
             -81,  -51,  -46,  -54,  -31,   -8,  -49,  -76,
             -47,  -12,    8,   -7,  -10,  -10,  -43,   -6,
             -15,   -8,   12,   52,   44,  -24,   -3,  -45,
              38,   24,   26,   30,   37,   15,   28,   -5,
               0,   24,   58,  102,   57,   37,    8,   46,
              31,   18,   29,   61,   84,  122,   51,   12,
             -18,  -23,   35,   54,    6,   51,    5,   57,
             -97,  -29,   11,  -19,  -14,   -1,  -57,  -42,

            /* knights: QQ */
               7,  -41,   19,  -58,  -35,  -46,  -47,  -55,
             -38,  -12,    0,   11,  -22,    2,   18,  -90,
             -12,   -8,    4,   24,   20,  -29,  -20,  -27,
              27,   16,   51,   51,   14,   25,   26,  -29,
              21,  -17,   28,   42,   44,   41,   27,    9,
              72,  -21,   24,   85,   69,   24,   50,    0,
               1,   10,    2,   22,   57,   31,  -12,  -30,
            -126,  -16,   -6,   -8,  -15,    6,   17,  -97,

            /* bishops: KK */
               8,   18,   -9,  -20,   -7,    2,  -10,   -3,
              18,    9,   16,    0,    8,   17,   39,   21,
              -7,   12,   12,    6,    3,   12,    9,    0,
             -11,  -18,    3,   34,   30,  -11,    2,   15,
             -17,   -2,   -4,   43,   15,   34,  -12,  -11,
               1,    5,   -4,   -8,    5,   75,   40,  -16,
             -51,  -12,  -11,  -35,  -25,   -5,  -58,  -46,
             -78,  -58,  -44,  -40,  -14,  -30,    6,  -60,

            /* bishops: KQ */
             -44,   -1,  -33,  -34,    0,   -6,   28,  -36,
             -25,  -16,  -17,   -2,    5,   30,   57,   -8,
             -38,  -16,   11,    8,   15,   31,   32,   35,
              13,   -3,    5,   39,   49,   17,  -15,  -12,
              -4,   12,   10,   63,   52,   18,    8,  -11,
             -14,   65,   90,   19,   38,   16,    5,    4,
             -45,   25,   -8,  -17,  -11,  -19,  -28,  -49,
             -18,  -20,   -9,    0,  -16,    8,  -13,  -43,

            /* bishops: QK */
               3,    4,   -8,  -19,  -40,  -24,  -33, -103,
              27,   56,   20,   13,   -4,  -21,  -33,  -12,
              -1,   34,   28,   21,    2,    5,  -10,  -20,
               2,   -4,   11,   60,   40,   -7,  -28,  -16,
             -36,   11,   21,   70,   26,   21,   12,   -9,
             -22,  -11,   34,   18,   27,   33,   52,    9,
             -21,  -52,   -8,  -17,  -56,  -16,  -11,  -30,
             -33,  -27,  -29,  -14,  -13,  -47,   34,    5,

            /* bishops: QQ */
             -94,  -54,  -10,  -53,  -14,  -28,   16,  -29,
              27,   41,  -19,   10,  -42,   10,  -21,   46,
               7,   12,   12,    3,    7,    6,   27,   -4,
              25,   17,  -13,   36,    6,   29,   -7,  -21,
              10,    6,   -2,   46,   35,    9,   -8,   -3,
             -43,   64,   65,   35,   14,    1,   10,  -11,
             -33,  -22,   15,   14,   26,   -6,   13,  -38,
             -21,   18,  -21,    8,  -13,  -13,   16,  -26,

            /* rooks: KK */
             -13,   -4,    0,   10,   18,   13,   16,  -12,
             -28,  -13,  -10,   -3,   -2,   15,   40,  -10,
             -37,  -32,  -27,  -14,    9,    1,   28,    4,
             -29,  -24,  -19,   -3,  -18,  -30,   21,  -24,
              -8,    1,   11,   40,   -1,   32,   53,   28,
              14,   52,   54,   57,   82,  125,  119,   70,
             -25,  -25,   -1,   32,    1,   68,   81,  112,
              88,   82,   70,   68,   51,   44,  102,  119,

            /* rooks: KQ */
             -22,   16,    3,   -2,  -10,    0,  -11,  -19,
             -44,   -4,  -25,  -26,  -29,  -33,  -40,  -28,
             -24,  -11,  -43,  -32,  -43,  -27,  -14,  -31,
             -54,  -11,  -49,   -3,  -14,  -48,  -15,    0,
              -4,   33,   -5,    1,   45,   17,   14,   18,
              71,   67,   26,   51,   70,   29,  103,   39,
              11,    1,   58,   24,    8,    8,   17,   59,
              30,   40,   62,   61,   45,   37,   47,   99,

            /* rooks: QK */
             -56,   -2,  -13,   -3,   11,    0,   22,  -15,
             -12,  -38,  -37,  -19,  -46,  -39,  -12,  -13,
              -4,   12,  -24,  -32,  -53,  -45,  -11,  -19,
              13,    5,  -27,  -18,  -40,  -35,   23,    8,
              37,    2,   49,   -4,  -10,  -17,   11,   18,
              50,   71,    4,    7,   60,   87,   95,   63,
              17,   47,   34,   11,   -5,   50,   42,   22,
             145,   63,   32,   64,   22,   79,   70,   54,

            /* rooks: QQ */
             -26,  -21,    9,   14,   -4,  -14,  -15,  -39,
             -21,  -40,  -26,   -1,  -32,  -31,  -17,  -21,
             -58,    9,  -10,  -21,  -18,  -33,  -19,  -24,
             -18,   20,  -26,  -32,  -48,   -3,  -51,   -8,
              29,   47,    9,   54,   13,   -8,  -18,   11,
              40,   60,  109,   51,   50,   57,   14,   29,
              49,   29,   17,   49,   93,   16,   27,    9,
              43,   41,   26,   25,   30,   42,   53,   38,

            /* queens: KK */
              12,    7,   23,   26,   41,   12,  -60,  -23,
              -3,   16,   14,   20,   22,   38,   27,    0,
              -6,   -3,    0,   -9,    8,   -5,    6,    4,
              -5,  -28,  -16,  -26,  -20,  -21,   -3,  -32,
              -9,  -24,  -45,  -29,  -43,  -31,  -30,  -27,
              -1,    4,  -37,  -44,   -4,   61,   33,   -1,
             -28,  -51,  -27,  -23,  -55,   55,   33,   84,
             -28,   21,   17,   17,   24,   84,   86,   80,

            /* queens: KQ */
              21,    8,   19,   18,    3,   -6,  -41,  -32,
              26,   20,   28,   20,   13,    1,  -51,  -34,
               7,   -1,   16,    3,   14,   16,   -9,  -27,
              -3,   34,  -22,  -29,   -6,  -11,   -3,  -20,
               7,   19,  -30,   -7,    4,    4,   -7,   -5,
              23,   60,   29,   12,    9,   16,   18,   12,
              48,   70,   32,   44,   31,   14,    1,   24,
              66,   92,   77,   70,   24,   50,   37,    6,

            /* queens: QK */
             -75,  -94,  -25,   -8,    3,   -3,  -23,  -17,
             -63,  -40,   23,   12,   12,   -1,   23,    6,
             -46,  -11,   -4,  -22,    4,  -21,    4,    1,
             -14,  -56,  -46,  -23,   -9,  -17,   -4,  -29,
             -10,  -31,  -23,  -33,  -35,   -3,  -18,   -5,
              -1,   13,   -2,  -16,   19,   39,   30,   35,
             -10,   13,  -16,  -16,   -8,   26,   45,   34,
             -11,   22,   26,    9,   -6,   76,   72,   67,

            /* queens: QQ */
             -62,  -82,  -27,  -16,   -4,  -12,  -38,  -26,
             -54,  -23,   15,   15,  -31,  -19,    6,  -23,
              -2,   -5,   -5,  -15,  -13,  -35,   -9,  -15,
             -38,   41,  -26,   -7,   -9,  -10,   -7,  -33,
             -34,    7,   27,   30,   18,    1,   12,    4,
              25,   54,   83,   47,   18,  -17,  -22,   18,
              80,   74,   61,   68,   26,  -16,    1,    2,
              11,   97,   20,   43,   55,   32,   60,   26,

            /* kings: KK */
               0,    0,    0,    0,  -43,  -72,    0,  -10,
               0,    0,    0,    0,  -18,   -7,   14,   -4,
               0,    0,    0,    0,   25,   -7,  -12,  -56,
               0,    0,    0,    0,  111,   88,   36,  -74,
               0,    0,    0,    0,  146,  236,  112,   19,
               0,    0,    0,    0,  190,  183,  215,  110,
               0,    0,    0,    0,  175,   87,   73,  -13,
               0,    0,    0,    0,   26,   83,   35,  -62,

            /* kings: KQ */
               0,    0,    0,    0,  -77,  -96,    7,  -10,
               0,    0,    0,    0,   -1,    3,    1,   -5,
               0,    0,    0,    0,   -2,   21,   -3,  -37,
               0,    0,    0,    0,   78,   30,   27,  -33,
               0,    0,    0,    0,  122,   70,   73,   10,
               0,    0,    0,    0,  145,  101,   93,   22,
               0,    0,    0,    0,  170,  188,   49,   30,
               0,    0,    0,    0,   15,   54,  101,   18,

            /* kings: QK */
             -31,   -3,  -45, -118,    0,    0,    0,    0,
              13,  -22,  -26,  -44,    0,    0,    0,    0,
             -37,  -32,  -11,  -35,    0,    0,    0,    0,
             -45,   30,   35,   61,    0,    0,    0,    0,
              13,   70,   92,  133,    0,    0,    0,    0,
             -11,   94,  146,  128,    0,    0,    0,    0,
              51,   92,  103,   59,    0,    0,    0,    0,
              -5,   38,  125,   96,    0,    0,    0,    0,

            /* kings: QQ */
             -90,  -10,  -45,  -95,    0,    0,    0,    0,
             -26,  -18,    2,  -25,    0,    0,    0,    0,
             -27,   10,    1,    5,    0,    0,    0,    0,
             -19,   49,   56,   71,    0,    0,    0,    0,
              39,  159,  197,  146,    0,    0,    0,    0,
              24,  123,  162,   75,    0,    0,    0,    0,
              -3,   77,  116,   83,    0,    0,    0,    0,
             -60,   17,   25,   50,    0,    0,    0,    0,

            #endregion

            /* opening mobility weights */

               9, // knights
               5, // bishops
               3, // rooks
               0, // queens

            /* opening squares attacked near enemy king */
              25, // attacks to squares 1 from king
              21, // attacks to squares 2 from king
               7, // attacks to squares 3 from king

            /* opening pawn shield/king safety */
              21, // # friendly pawns 1 from king
               7, // # friendly pawns 2 from king
               6, // # friendly pawns 3 from king

            /* opening isolated pawns */
            -9,

            /* opening backward pawns */
            7,

            /* opening doubled pawns */
            -14,

            /* opening adjacent/connected pawns */
               0,    0,    0,    0,    0,    0,    0,    0,
               9,    1,   -1,   18,   22,   -1,  -17,    4,
               8,   19,   13,   16,   47,    7,   16,    2,
              -2,   15,    7,   26,   46,   37,   11,   27,
              45,   24,   74,   37,  118,   84,   58,   22,
              69,  117,  209,  191,  158,  244,  150,  112,
             194,  208,  191,  282,  249,  183,  105,  104,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* opening knight on outpost */
            9,

            /* opening bishop on outpost */
            12,

            /* opening bishop pair */
            42,

            /* opening rook on open file */
            35,

            /* opening rook on half-open file */
            11,

            /* opening rook behind passed pawn */
            -4,

            /* opening doubled rooks on file */
            9,

            /* opening king on open file */
            -74,

            /* opening king on half-open file */
            -35,

            /* opening castling rights available */
            28,

            /* opening castling complete */
            20,

            /* opening center control */
               2, // D0
               3, // D1

            /* opening queen on open file */
            -9,

            /* opening queen on half-open file */
            12,

            /* opening rook on seventh rank */
            38,

            /* opening passed pawn */
               0,    0,    0,    0,    0,    0,    0,    0,
             -12,  -17,  -22,  -41,   10,  -12,   -2,   35,
              -9,    0,  -23,  -17,  -30,  -30,  -29,   13,
               9,   11,   -7,   19,  -12,  -11,  -40,   -6,
              58,   74,   66,   51,   33,   37,   61,   33,
             131,  136,  120,   67,   70,  115,   61,   73,
             207,  158,  164,  176,  128,  165,  145,  147,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* opening bad bishop pawns */
               0,    0,    0,    0,    0,    0,    0,    0,
              -7,   -4,   -9,   -5,  -16,  -21,  -15,    3,
              -6,  -13,  -17,  -13,  -18,  -16,  -16,   -6,
              -8,    7,   -1,   -2,  -21,  -12,   -6,   -5,
               4,    7,    3,    4,   -1,   -1,   -1,   -1,
              19,    9,   10,   10,   19,    5,   55,   -1,
              29,   66,   61,   48,   38,  105,   37,   84,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* opening block passed pawn */
               0,    0,    0,    0,    0,    0,    0,    0,  // blocked by pawns
               0,   60,   76,   70,   66,   79,   90,    0,  // blocked by knights
               0,   35,   41,   26,   23,   60,  113,    0,  // blocked by bishops
               0,  -39,  -14,  -26,  -10,   23,  164,    0,  // blocked by rooks
               0,    8,   45,   19,    9,    6, -106,    0,  // blocked by queens
               0,  -21,   95,   77,   98,  191,  181,    0,  // blocked by kings

            /* opening supported pawn chain */
               0,    0,    0,    0,    0,    0,    0,    0,
               0,    0,    0,    0,    0,    0,    0,    0,
              21,   31,   45,   53,   36,   40,   33,   61,
               0,   17,   23,   31,   29,   20,   28,    9,
             -22,   25,   53,   43,   61,   63,   27,   19,
              51,  134,  118,  170,  147,   94,  108,   80,
              97,  238,  237,  200,  311,  150,  188,  -17,
               0,    0,    0,    0,    0,    0,    0,    0,


            /*------------------- END GAME WEIGHTS -------------------*/

            /* end game piece values */
            152, 472, 534, 927, 1609, 0,

            /* end game piece square values */

            #region end game piece square values

            /* pawns: KK */
               0,    0,    0,    0,    0,    0,    0,    0,
               3,    1,   -2,  -24,   26,    6,   -6,  -32,
               1,    1,  -16,   -7,  -11,   -8,  -13,  -25,
              13,   12,  -19,  -39,  -31,  -16,   -6,  -25,
              37,   13,   -3,  -29,  -30,  -14,    1,  -10,
              66,   46,   37,   35,   24,  -14,    7,  -14,
             119,  109,  122,   27,   66,   29,   67,    7,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* pawns: KQ */
               0,    0,    0,    0,    0,    0,    0,    0,
              18,   30,   18,   61,  -23,  -15,  -19,  -14,
               7,   24,   -5,   -4,  -33,  -15,  -21,  -19,
               5,   -5,   -9,  -25,  -17,   10,    8,    1,
               8,  -39,  -39,  -14,   -3,   58,   62,   54,
             -33,  -71,  -68,   35,   75,  125,  168,  154,
              14,   25,   16,   63,  147,  206,  189,  173,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* pawns: QK */
               0,    0,    0,    0,    0,    0,    0,    0,
              -9,  -24,  -14,   23,   10,   28,   32,   31,
             -13,  -19,  -15,   -7,   -6,   -2,   15,   17,
              19,   22,    8,  -18,   -3,   -4,  -10,   -9,
              76,   67,   46,    6,  -17,  -17,   -5,    4,
             111,  118,  121,   98,   37,  -17,    0,   10,
             140,  176,  198,   93,   95,   38,   10,   20,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* pawns: QQ */
               0,    0,    0,    0,    0,    0,    0,    0,
             -16,  -24,  -15,   19,   -3,   -5,    5,    3,
             -14,   -6,  -17,  -10,  -20,  -22,   -8,   -7,
              -7,    3,  -29,  -18,  -25,  -26,   -6,   -3,
             -15,  -38,  -31,   -2,  -14,    0,   12,   23,
             -45,  -54,  -61,   28,   47,   55,   89,   89,
              17,   59,   17,   65,   72,  134,  117,  132,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* knights: KK */
             -54, -104,  -43,  -19,  -29,  -26,  -42,  -55,
             -56,  -12,  -26,  -33,  -14,  -15,  -12,  -25,
             -58,  -15,   -3,   21,   16,   -4,   -1,   14,
              -2,   19,   42,   44,   49,   44,   47,   11,
              31,   16,   28,   53,   54,   69,   72,   24,
              40,   13,   24,   24,    5,    7,   52,   47,
               8,   30,   24,   17,   36,    5,    0,   16,
             -21,   13,   35,   21,   23,   12,   32,  -64,

            /* knights: KQ */
             -94,  -36,  -11,   -2,  -12,  -11,   17, -118,
             -37,    2,  -30,  -30,  -12,   -2,    0,  -11,
             -19,  -24,    6,   -2,  -11,  -32,   22,    6,
              -5,   -6,   17,    4,   12,   28,    7,   11,
             -33,  -17,    7,   21,    3,   12,    4,    7,
             -22,  -41,  -33,  -10,    1,    4,   24,   11,
             -32,  -26,  -38,    4,   -4,   -5,    9,   -5,
            -118,  -34,  -17,    1,  -24,   37,   41,  -44,

            /* knights: QK */
             -94,   -7,    4,   17,   -4,  -24,  -36,  -78,
             -58,   -5,  -16,   -4,   -9,    0,  -12,  -65,
               2,  -20,  -12,    4,   -3,   10,  -28,  -46,
              -1,   24,   25,   36,   15,   19,  -13,  -11,
              47,   28,   17,   -5,   13,   25,   14,  -11,
              38,   30,   36,    6,   -5,  -23,   -8,  -22,
              19,   33,   24,   52,   14,    2,   -4,  -13,
             -81,   22,   20,   13,   10,   -9,  -35,  -67,

            /* knights: QQ */
             -87,  -10,  -11,   13,    2,    6,  -46,  -75,
             -33,   29,  -19,   -1,   -9,  -36,  -26,  -51,
               3,   -8,  -12,    4,   -1,  -13,   -3,  -42,
              43,   16,   18,   -2,    5,   -7,   14,    0,
              22,   16,    0,    8,   10,   -6,   -8,   10,
               3,   18,   -8,    2,  -26,   33,   -4,    4,
              43,   10,   14,   46,   23,   18,   23,   17,
             -68,   62,   27,   56,    4,   49,   43,  -18,

            /* bishops: KK */
             -47,  -57,  -34,    2,   -4,   -2,  -23,  -68,
             -36,  -24,  -20,    2,   -4,  -17,  -33,  -65,
             -10,   10,   32,   15,   46,   13,   -5,  -10,
              14,   30,   40,   27,   12,   29,   -1,  -27,
              30,   12,   18,    7,   21,   10,   18,   33,
               2,   21,   20,   19,   23,   22,    9,   54,
              41,   22,   22,   36,   26,   20,   14,    1,
              44,   46,   55,   51,   31,    6,    2,   15,

            /* bishops: KQ */
             -18,    8,    4,    7,  -14,   -9,  -67,  -60,
             -28,  -15,   14,   -1,   -1,  -27,  -13,  -51,
               5,   12,   14,    5,   17,   -3,   -7,  -20,
              12,   16,    2,  -10,    8,   37,   33,   20,
              13,   -2,  -11,  -43,   10,   25,   27,   18,
              -3,  -22,   -4,    5,    5,   17,   37,   29,
             -38,  -16,    4,    6,   19,   18,   42,   52,
              11,   -6,   -3,   17,   17,   26,   40,   28,

            /* bishops: QK */
             -63,  -31,   11,   11,   16,  -13,    6,  -34,
             -29,  -29,    4,   15,    5,    8,   -6,  -31,
              -3,   19,   17,   16,   38,    1,    6,  -20,
               2,   22,   42,   -1,   -7,   21,   19,   -4,
              42,   20,   17,   -4,   10,   21,  -10,   21,
              29,   39,   11,   32,   30,   17,  -24,    4,
              28,   40,   34,   36,   34,   22,  -13,  -14,
              19,   12,   40,   35,    9,   -2,   -2,   15,

            /* bishops: QQ */
             -21,    2,  -17,   -9,  -17,  -22,  -39,  -66,
             -57,  -25,   -5,   -8,   12,  -29,  -17,  -89,
               0,    1,   -2,  -13,    1,  -10,  -46,  -18,
               8,   13,    0,  -13,  -12,    0,   -4,   -1,
               3,   -5,  -15,  -28,  -36,  -19,  -14,   12,
               5,   -1,  -24,   -7,   11,   -4,   -1,   -1,
             -19,   -3,   -8,   18,  -11,    1,    2,   16,
             -19,  -12,   -8,    5,   33,    1,   14,   51,

            /* rooks: KK */
              29,   24,   10,  -15,  -23,   -1,   -4,  -18,
              24,   16,    3,  -14,  -21,  -35,  -12,   -3,
              31,   28,    9,  -16,  -36,  -17,  -21,  -14,
              58,   34,   30,    8,   -3,   26,    9,   24,
              69,   58,   37,    3,   25,   21,   24,   25,
              70,   45,   30,    6,   -1,   18,   19,   28,
              68,   69,   33,    2,   23,   18,   36,   20,
              41,   60,   34,    8,   40,   65,   58,   36,

            /* rooks: KQ */
             -13,  -28,  -36,  -25,  -19,  -22,   26,   -1,
             -10,  -31,  -24,  -25,  -13,   -7,   10,   10,
             -24,  -24,  -24,  -32,  -25,  -15,    5,   -3,
               3,  -19,   -1,  -18,  -23,   21,   20,    2,
               2,  -12,  -13,    7,  -24,   12,   20,   24,
              -3,   -6,  -14,  -19,  -29,   11,   -2,   26,
               6,   14,  -33,  -11,    1,    2,   29,   13,
              35,   20,  -19,  -18,   -8,   25,   47,   12,

            /* rooks: QK */
              22,    5,    5,  -29,  -41,  -26,  -41,  -11,
               9,   10,    9,  -25,  -23,  -25,  -34,  -30,
              -2,  -10,   -4,  -20,  -18,  -28,  -38,  -47,
               2,   10,   16,  -14,   -3,   -3,  -13,  -24,
              25,   40,    7,    9,   -1,    2,   -8,  -20,
              29,   28,   43,   14,  -26,  -12,  -14,   -3,
              25,   15,    6,    3,   -2,   -6,    6,    6,
             -58,   20,   30,    9,   16,  -10,    4,    4,

            /* rooks: QQ */
             -13,  -15,  -42,  -58,  -37,   -8,   -7,   31,
             -30,   -2,  -23,  -62,  -52,  -51,  -34,  -12,
              -6,  -16,  -34,  -48,  -39,  -38,  -20,    8,
               3,    2,   -5,  -12,  -41,  -21,    8,   13,
               1,   -4,   -6,  -20,   -8,    5,   27,   25,
              16,   11,   -5,   -8,  -23,    2,   25,   32,
               3,   21,   15,  -19,  -43,  -18,    7,   26,
              40,   40,   26,   18,  -13,   -1,   41,   31,

            /* queens: KK */
             -34,  -26,  -59,  -46, -106, -111,  -77,  -32,
              -3,  -17,  -26,  -41,  -45,  -67,  -89,  -48,
             -25,   -4,    6,   -2,  -15,   20,   25,  -26,
              11,   27,   24,   30,   17,   47,   41,   75,
              22,   43,   59,   34,   74,   91,  115,  129,
              10,   54,   61,   74,   79,   87,  111,  148,
              67,  100,   67,   54,  102,   60,  155,  119,
              51,   44,   40,   47,   54,   68,  117,  115,

            /* queens: KQ */
             -17,   11,  -63,  -22,   24,  -36,  -17,   11,
              17,  -10,  -47,  -44,   -5,  -34,  -45,  -48,
              14,   -9,  -33,  -12,   -6,   15,    7,  -19,
              14,  -47,    1,    5,    0,   11,   17,   58,
             -10,  -20,   24,  -11,  -13,   11,   53,   68,
              32,   23,   20,   19,   25,   61,   77,   43,
              51,   20,   39,   26,    9,   49,   69,   54,
              44,   60,   40,   71,   87,   29,   44,   10,

            /* queens: QK */
             -58,  -17,  -84,  -52,  -58,  -53,  -46,  -11,
             -35,  -64,  -60,  -37,  -56,  -39,  -28,   -4,
             -46,    4,    3,   14,  -33,   10,  -28,    0,
             -42,   47,   46,   12,   -6,  -14,   -9,   25,
              28,   13,   42,   31,   17,   15,   31,    2,
              10,   65,   34,   42,   20,   32,   32,   20,
               0,   37,   57,   34,   18,    7,    3,   39,
              32,   50,   28,   38,   11,   43,   55,   27,

            /* queens: QQ */
             -51,  -29,  -17,  -13,   15,  -17,  -51,   -8,
              -9,  -36,  -19,   -5,    1,  -35,  -29,  -42,
              10,   24,   -2,  -10,  -19,   34,  -10,  -18,
              25,  -16,   33,  -15,  -34,  -61,  -23,   25,
              37,   25,    5,  -19,   40,  -23,    0,   -9,
              63,   50,   67,   20,   19,   34,   33,   61,
              35,   65,   53,   61,   14,    8,   41,    0,
              -7,   58,   36,   30,   -1,  -10,    2,  -14,

            /* kings: KK */
               0,    0,    0,    0,  -30,    5,  -36,  -80,
               0,    0,    0,    0,   11,    5,   -9,  -27,
               0,    0,    0,    0,   24,   23,    7,  -17,
               0,    0,    0,    0,   22,   23,   19,    6,
               0,    0,    0,    0,   23,    1,   27,    7,
               0,    0,    0,    0,   18,   20,   37,   16,
               0,    0,    0,    0,    4,   15,   70,   26,
               0,    0,    0,    0,    8,   19,   37, -105,

            /* kings: KQ */
               0,    0,    0,    0,   -4,    9,  -49,  -74,
               0,    0,    0,    0,    8,   -5,   -1,  -39,
               0,    0,    0,    0,   33,   12,    9,  -15,
               0,    0,    0,    0,   35,   47,   41,   13,
               0,    0,    0,    0,   46,   70,   63,   51,
               0,    0,    0,    0,   47,   64,   63,   44,
               0,    0,    0,    0,   22,   15,   52,   18,
               0,    0,    0,    0,    6,    9,  -17,  -25,

            /* kings: QK */
             -90,  -80,  -52,  -17,    0,    0,    0,    0,
             -88,  -15,  -25,  -11,    0,    0,    0,    0,
             -42,    3,   -1,   12,    0,    0,    0,    0,
               5,   23,   27,   17,    0,    0,    0,    0,
              10,   45,   38,   25,    0,    0,    0,    0,
              34,   46,   39,   27,    0,    0,    0,    0,
               3,   31,   27,   34,    0,    0,    0,    0,
             -39,   23,    1,    3,    0,    0,    0,    0,

            /* kings: QQ */
              11,  -25,  -18,  -17,    0,    0,    0,    0,
             -15,   -2,  -13,  -14,    0,    0,    0,    0,
             -20,   -8,    3,    4,    0,    0,    0,    0,
             -16,   12,   21,   23,    0,    0,    0,    0,
              12,   17,   13,   22,    0,    0,    0,    0,
              29,   42,    9,   38,    0,    0,    0,    0,
              -1,   62,   18,   22,    0,    0,    0,    0,
            -112,   43,   10,   17,    0,    0,    0,    0,

            #endregion

            /* end game mobility weights */

               9, // knights
               5, // bishops
               3, // rooks
               4, // queens

            /* end game squares attacked near enemy king */
              -6, // attacks to squares 1 from king
              -2, // attacks to squares 2 from king
               0, // attacks to squares 3 from king

            /* end game pawn shield/king safety */
              13, // # friendly pawns 1 from king
              17, // # friendly pawns 2 from king
              11, // # friendly pawns 3 from king

            /* end game isolated pawns */
            -7,

            /* end game backward pawns */
            2,

            /* end game doubled pawns */
            -44,

            /* end game adjacent/connected pawns */
               0,    0,    0,    0,    0,    0,    0,    0,
               5,   -1,    6,   -2,    9,  -17,    9,  -23,
              -3,    1,   15,   24,   -1,   12,    5,  -20,
              10,   13,   50,   75,   22,   31,   14,    6,
              81,   46,   97,   78,   71,   43,   46,   37,
              89,  136,  168,  143,  146,  127,   65,   18,
             238,  322,  362,  354,  408,  259,  254,  229,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* end game knight on outpost */
            32,

            /* end game bishop on outpost */
            32,

            /* end game bishop pair */
            109,

            /* end game rook on open file */
            5,

            /* end game rook on half-open file */
            32,

            /* end game rook behind passed pawn */
            39,

            /* end game doubled rooks on file */
            14,

            /* end game king on open file */
            9,

            /* end game king on half-open file */
            33,

            /* end game castling rights available */
            -26,

            /* end game castling complete */
            -18,

            /* end game center control */
               8, // D0
               6, // D1

            /* end game queen on open file */
            31,

            /* end game queen on half-open file */
            15,

            /* end game rook on seventh rank */
            30,

            /* end game passed pawn */
               0,    0,    0,    0,    0,    0,    0,    0,
              21,   35,   26,   17,    6,   11,   34,    1,
              29,   32,   29,   13,   23,   27,   53,   18,
              71,   68,   57,   37,   37,   45,   85,   70,
             105,  110,   80,   68,   72,   62,   79,   90,
             165,  178,  130,   88,   92,   95,  120,  122,
             152,  176,  168,  172,  158,  159,  187,  179,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* end game bad bishop pawns */
               0,    0,    0,    0,    0,    0,    0,    0,
               3,    4,    0,  -38,   -4,   -4,   -7,   -8,
             -10,   -6,  -11,  -11,  -14,   -8,   -4,   -6,
             -12,  -27,  -42,  -59,  -34,  -30,  -23,   -7,
             -29,  -34,  -40,  -58,  -44,  -38,  -35,  -23,
             -43,  -52,  -64,  -68,  -73,  -57, -104,  -33,
             -44,  -87, -100, -119, -137, -126, -147, -131,
               0,    0,    0,    0,    0,    0,    0,    0,

            /* end game block passed pawn */
               0,    0,    0,    0,    0,    0,    0,    0,  // blocked by pawns
               0,  -27,   -2,   26,   50,   44,   63,    0,  // blocked by knights
               0,   10,   37,   57,  101,  122,  169,    0,  // blocked by bishops
               0,   11,  -26,  -28,   -6,   -2,  -32,    0,  // blocked by rooks
               0,   -3,   23,   25,   -4, -113, -176,    0,  // blocked by queens
               0,   -4,    1,   36,   66,  101,  241,    0,  // blocked by kings

            /* end game supported pawn chain */
               0,    0,    0,    0,    0,    0,    0,    0,
               0,    0,    0,    0,    0,    0,    0,    0,
              19,   24,   34,   49,   42,   23,   10,    3,
              -3,   25,    9,   34,   14,   11,    9,    3,
              14,   25,   33,   25,   31,   11,   20,    7,
              56,   29,   92,   67,   78,   77,    8,    5,
              79,   70,  127,   54,  111,  103,   37,   93,
               0,    0,    0,    0,    0,    0,    0,    0,
        };
    }
}