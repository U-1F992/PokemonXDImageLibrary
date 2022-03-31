namespace PokemonXDImageLibrary;

/// <summary>
/// いますぐバトル生成結果を表す構造体
/// </summary>
public struct QuickBattleParties
{
    /// <summary>
    /// プレイヤー側手持ちの基礎HP
    /// <see href="https://github.com/yatsuna827/XDDatabase/blob/abdd685a8da927a73735ab5d3110d73c35ef9237/XDDatabase/Program.cs#L111-L118"/>
    /// </summary>
    private readonly (uint First, uint Second)[] pBaseHP = new (uint, uint)[]
    {
        (322, 340),
        (310, 290),
        (210, 620),
        (320, 230),
        (310, 310),
    };
    /// <summary>
    /// COM側手持ちの基礎HP
    /// <see href="https://github.com/yatsuna827/XDDatabase/blob/abdd685a8da927a73735ab5d3110d73c35ef9237/XDDatabase/Program.cs#L119-L126"/>
    /// </summary>
    private readonly (uint First, uint Second)[] eBaseHP = new (uint, uint)[]
    {
        (290, 310),
        (290, 270),
        (290, 250),
        (320, 270),
        (270, 230),
    };

    /// <summary>
    /// プレイヤー側
    /// </summary>
    public QuickBattleParty P1 { get; }
    /// <summary>
    /// COM側
    /// </summary>
    public QuickBattleParty COM { get; }

    /// <summary>
    /// 手持ちのインデックスと4体のHPを集約した <see cref="UInt32"/> で、 <see cref="QuickBattleParties"/> の新規インスタンスを初期化します。<br/>
    /// <see href="https://github.com/yatsuna827/XDDatabase"/>
    /// </summary>
    /// <param name="pIndex">ミュウツー | ミュウ | デオキシス | レックウザ | ジラーチ</param>
    /// <param name="eIndex">フリーザー | サンダー | ファイヤー | ガルーラ | ラティアス</param>
    /// <param name="HP">
    /// 基礎HPとの差分を1byteずつ詰めた値
    /// <see href="https://github.com/yatsuna827/XDDatabase/blob/abdd685a8da927a73735ab5d3110d73c35ef9237/XDDatabase/Program.cs#L238"/>
    /// </param>
    public QuickBattleParties(QuickBattleIndex_P1 pIndex, QuickBattleIndex_COM eIndex, UInt32 HP) : this((int)pIndex, (int)eIndex, HP) { }
    /// <summary>
    /// 手持ちのインデックスと4体のHPを集約した <see cref="UInt32"/> で、 <see cref="QuickBattleParties"/> の新規インスタンスを初期化します。<br/>
    /// <see href="https://github.com/yatsuna827/XDDatabase"/>
    /// </summary>
    /// <param name="pIndex">ミュウツー | ミュウ | デオキシス | レックウザ | ジラーチ</param>
    /// <param name="eIndex">フリーザー | サンダー | ファイヤー | ガルーラ | ラティアス</param>
    /// <param name="HP">
    /// 基礎HPとの差分を1byteずつ詰めた値
    /// <see href="https://github.com/yatsuna827/XDDatabase/blob/abdd685a8da927a73735ab5d3110d73c35ef9237/XDDatabase/Program.cs#L238"/>
    /// </param>
    public QuickBattleParties(int pIndex, int eIndex, UInt32 HP)
    {
        var pHP_1 = (int)(pBaseHP[pIndex].First + ((HP & 0x0000ff00) >> 8));
        var pHP_2 = (int)(pBaseHP[pIndex].Second + ((HP & 0x000000ff)));
        var eHP_1 = (int)(eBaseHP[eIndex].First + ((HP & 0xff000000) >> 24));
        var eHP_2 = (int)(eBaseHP[eIndex].Second + ((HP & 0x00ff0000) >> 16));
        P1 = new QuickBattleParty(pIndex, pHP_1, pHP_2);
        COM = new QuickBattleParty(eIndex, eHP_1, eHP_2);
    }
    /// <summary>
    /// 手持ちのインデックスと4体のHP実数値で、 <see cref="QuickBattleParties"/> の新規インスタンスを初期化します。<br/>
    /// </summary>
    /// <param name="pIndex">ミュウツー | ミュウ | デオキシス | レックウザ | ジラーチ</param>
    /// <param name="eIndex">フリーザー | サンダー | ファイヤー | ガルーラ | ラティアス</param>
    /// <param name="pHP_1">プレイヤー側 1体目</param>
    /// <param name="pHP_2">プレイヤー側 2体目</param>
    /// <param name="eHP_1">COM側 1体目</param>
    /// <param name="eHP_2">COM側 2体目</param>
    public QuickBattleParties(QuickBattleIndex_P1 pIndex, QuickBattleIndex_COM eIndex, int pHP_1, int pHP_2, int eHP_1, int eHP_2) : this((int)pIndex, (int)eIndex, pHP_1, pHP_2, eHP_1, eHP_2) { }
    /// <summary>
    /// 手持ちのインデックスと4体のHP実数値で、 <see cref="QuickBattleParties"/> の新規インスタンスを初期化します。<br/>
    /// </summary>
    /// <param name="pIndex">ミュウツー | ミュウ | デオキシス | レックウザ | ジラーチ</param>
    /// <param name="eIndex">フリーザー | サンダー | ファイヤー | ガルーラ | ラティアス</param>
    /// <param name="pHP_1">プレイヤー側 1体目</param>
    /// <param name="pHP_2">プレイヤー側 2体目</param>
    /// <param name="eHP_1">COM側 1体目</param>
    /// <param name="eHP_2">COM側 2体目</param>
    public QuickBattleParties(int pIndex, int eIndex, int pHP_1, int pHP_2, int eHP_1, int eHP_2)
    {
        P1 = new QuickBattleParty(pIndex, pHP_1, pHP_2);
        COM = new QuickBattleParty(eIndex, eHP_1, eHP_2);
    }

    /// <summary>
    /// いますぐバトルの手持ちを表す構造体
    /// </summary>
    public struct QuickBattleParty
    {
        /// <summary>
        /// 手持ちを表すインデックス<br/>
        /// プレイヤー側: ミュウツー | ミュウ | デオキシス | レックウザ | ジラーチ<br/>
        /// COM側: フリーザー | サンダー | ファイヤー | ガルーラ | ラティアス
        /// </summary>
        public int Index { get; }
        /// <summary>
        /// HP実数値
        /// </summary>
        public int[] HP { get; }
        /// <summary>
        /// 手持ちのインデックスと2体のHP実数値で、 <see cref="QuickBattleParty"/> の新規インスタンスを初期化します。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hp_1"></param>
        /// <param name="hp_2"></param>
        public QuickBattleParty(int index, int hp_1, int hp_2)
        {
            Index = index;
            HP = new int[] { hp_1, hp_2 };
        }
    }
}

/// <summary>
/// いますぐバトルのプレイヤー側手持ちのインデックスを表す
/// </summary>
public enum QuickBattleIndex_P1
{
    /// <summary>
    /// ミュウツー
    /// </summary>
    Mewtwo,
    /// <summary>
    /// ミュウ
    /// </summary>
    Mew,
    /// <summary>
    /// デオキシス
    /// </summary>
    Deoxys,
    /// <summary>
    /// レックウザ
    /// </summary>
    Rayquaza,
    /// <summary>
    /// ジラーチ
    /// </summary>
    Jirachi
}
/// <summary>
/// いますぐバトルのCOM側手持ちのインデックスを表す
/// </summary>
public enum QuickBattleIndex_COM
{
    /// <summary>
    /// フリーザー
    /// </summary>
    Articuno,
    /// <summary>
    /// サンダー
    /// </summary>
    Zapdos,
    /// <summary>
    /// ファイヤー
    /// </summary>
    Moltres,
    /// <summary>
    /// ガルーラ
    /// </summary>
    Kangaskhan,
    /// <summary>
    /// ラティアス
    /// </summary>
    Latias
}