using System.Reflection;
using OpenCvSharp;

namespace PokemonXDImageLibrary;

/// <summary>
/// <see cref="OpenCvSharp.Mat"/>クラスに対して、「ポケモンXD 闇の旋風ダーク・ルギア」のゲーム画面キャプチャ画像を扱う拡張メソッドを提供します。
/// </summary>
public static class MatExtension
{
    /// <summary>
    /// 個体実数値を取得する。
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static (int HP, int Attack, int Defense, int Speed, int SpAtk, int SpDef) GetStats(this Mat mat)
    {
        return mat.GetStats(new Rect[]
        {
            new Rect(1138, 197, 120, 58),
            new Rect(1160, 329, 120, 58),
            new Rect(1160, 407, 120, 58),
            new Rect(1160, 635, 120, 58),
            new Rect(1160, 481, 120, 58),
            new Rect(1160, 561, 120, 58)
        });
    }
    /// <summary>
    /// 個体実数値を取得する。
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="rects"></param>
    /// <returns></returns>
    public static (int HP, int Attack, int Defense, int Speed, int SpAtk, int SpDef) GetStats(this Mat mat, Rect[] rects)
    {
        var source = mat.Optimize();
        var results = source.GetNumbers(rects, new int[] { 3, 3, 3, 3, 3, 3 }, out var actual);

        (int HP, int Attack, int Defense, int Speed, int SpAtk, int SpDef) result;
        result.HP = results[0];
        result.Attack = results[1];
        result.Defense = results[2];
        result.Speed = results[3];
        result.SpAtk = results[4];
        result.SpDef = results[5];

#if DEBUG
        for (var i = 0; i < rects.Length; i++)
        {
            // 再検出で位置調整されている場合、その旨を表示
            var scalar = actual[i] != rects[i] ? Scalar.Green : Scalar.Red;

            source.RectangleDivided(actual[i], 3, scalar, 2);
            source.PutText(results[i].ToString(), new Point(actual[i].X - 5, actual[i].Y - 5), HersheyFonts.HersheySimplex, 1, scalar, 2);
        }
        using (var window = new Window("result"))
        {
            window.ShowImage(source.Resize(new Size(), 0.5, 0.5));
            Cv2.WaitKey(-1);
        }
#endif
        return result;
    }
    /// <summary>
    /// いますぐバトル生成結果を取得する。
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static ((int Index, int[] HP) P1, (int Index, int[] HP) COM) GetQuickBattleParties(this Mat mat)
    {
        return mat.GetQuickBattleParties(new Rect[]
        {
            new Rect(322, 681, 117, 58),
            new Rect(322, 896, 117, 58),
            new Rect(949, 681, 117, 58),
            new Rect(949, 896, 117, 58),
            new Rect(109, 603, 110, 117),
            new Rect(736, 603, 110, 117)
        });
    }
    /// <summary>
    /// いますぐバトル生成結果を取得する。
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="rects"></param>
    /// <returns></returns>
    public static ((int Index, int[] HP) P1, (int Index, int[] HP) COM) GetQuickBattleParties(this Mat mat, Rect[] rects)
    {
        var source = mat.Optimize();
        var results = source.GetNumbers(rects.Take(4).ToArray(), new int[] { 3, 3, 3, 3 }, out var actual);

        ((int Index, int[] HP) P1, (int Index, int[] HP) COM) result;
        result.P1.HP = new int[] { results[0], results[1] };
        result.COM.HP = new int[] { results[2], results[3] };
        result.P1.Index = source.Clone(rects[4]).MatchIcon(QuickBattleSide.P1);
        result.COM.Index = source.Clone(rects[5]).MatchIcon(QuickBattleSide.COM);

#if DEBUG
        for (var i = 0; i < rects.Length; i++)
        {
            if (i < 4)
            {
                // 再検出で位置調整されている場合、その旨を表示
                var scalar = actual[i] != rects[i] ? Scalar.Green : Scalar.Red;

                source.RectangleDivided(actual[i], 3, scalar, 2);
                source.PutText(results[i].ToString(), new Point(actual[i].X - 5, actual[i].Y - 5), HersheyFonts.HersheySimplex, 1, scalar, 2);
            }
            else
            {
                source.Rectangle(rects[i], Scalar.Red, 2);
                source.PutText
                (
                    i == 4 ?
                        (new string[] { "Mewtwo", "Mew", "Deoxys", "Rayquaza", "Jirachi" })[result.P1.Index] :
                        (new string[] { "Articuno", "Zapdos", "Moltres", "Kangaskhan", "Latias" })[result.COM.Index],
                    new Point(rects[i].X - 5, rects[i].Y - 5),
                    HersheyFonts.HersheySimplex,
                    1,
                    Scalar.Red,
                    2
                );
            }
        }
        using (var window = new Window("result"))
        {
            window.ShowImage(source.Resize(new Size(), 0.5, 0.5));
            Cv2.WaitKey(-1);
        }
#endif
        return result;
    }
    /// <summary>
    /// ある入力の複数範囲に対するGetNumberを並列処理で捌く。<br/>
    /// 検出できなかった場合、自動で範囲を広げて再検出を試みる。
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="rects"></param>
    /// <param name="numberOfDigits"></param>
    /// <param name="actual">実際に再検出したRect</param>
    /// <returns></returns>
    private static int[] GetNumbers(this Mat mat, Rect[] rects, int[] numberOfDigits, out Rect[] actual)
    {
        if (rects.Length != numberOfDigits.Length) throw new ArgumentException();

        var result = new int[rects.Length];
        var _actual = new Rect[rects.Length];

        Parallel.For(0, rects.Length, i =>
        {
            var detected = false;
            var correction = 0;
            var upper = true;
            while (!detected && correction < 40)
            {
                // correctionに応じてy軸方向に平行移動する
                var rect = new Rect(rects[i].X, rects[i].Y - correction, rects[i].Width, rects[i].Height);
                try
                {
                    lock (result)
                    {
                        result[i] = (mat.Clone(rect).GetNumber(numberOfDigits[i]));
                        _actual[i] = rect;
                    }
                    detected = true;
                }
                catch (NoValidDigitsException)
                {
                    // y軸方向に上下させる
                    // 0 -> 10 -> -10 -> 20 -> -20 -> 30 -> -30 -> 40(終了)
                    if (upper && correction != 0)
                    {
                        correction *= -1;
                        upper = false;
                    }
                    else
                    {
                        correction = (-1 * correction) + 10;
                        upper = true;
                    }
                }
            }
            if (!detected) throw new NoValidDigitsException();
        });

        actual = _actual;
        return result;
    }
    /// <summary>
    /// 右揃えで高々指定された桁数の数値を検出する。
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="numberOfDigits">桁数</param>
    /// <returns></returns>
    private static int GetNumber(this Mat mat, int numberOfDigits)
    {
        // 横に等分する範囲のリスト
        var rects = new List<Rect>();
        for (var i = 0; i < numberOfDigits; i++) rects.Add(new Rect(mat.Width / numberOfDigits * i, 0, mat.Width / numberOfDigits, mat.Height));

        // 各範囲で数字の判定を行う
        var digits = new int?[numberOfDigits];
        Parallel.For(0, numberOfDigits, i => { lock (digits) digits[i] = (mat.Clone(rects[i]).MatchDigit()); });

        // 最小の桁がnullは異常
        if (digits[digits.Length - 1] == null) throw new NoValidDigitsException();
        // 数字が検出された後にnullが検出されていたら異常
        var digitDetected = false;
        for (var i = 0; i < digits.Length; i++)
        {
            if (digits[i] != null && !digitDetected) digitDetected = true;
            else if (digits[i] == null && digitDetected) throw new NoValidDigitsException();
        }

        // 各範囲の数字検出の結果から、最終的な数値を捻出する
        var resultString = "";
        foreach (var digit in digits) resultString += (digit == null ? string.Empty : digit.ToString());
        return Convert.ToInt32(resultString);
    }
    /// <summary>
    /// 埋め込み済みリソースの数字画像を用いて、テンプレートマッチングで数字の判定を行う。<br/>
    /// 最大類似度が0.8未満の場合は、未検出としてnullを返す。
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    private static int? MatchDigit(this Mat mat)
    {
        var asm = Assembly.GetExecutingAssembly();

        // 下処理 (二値化+反転+拡大)
        var digit = mat;
        Cv2.CvtColor(digit, digit, ColorConversionCodes.RGB2GRAY);
        Cv2.BitwiseNot(digit, digit);
        Cv2.Threshold(digit, digit, 127, 255, ThresholdTypes.Binary);
        Cv2.Resize(digit, digit, new Size(), 2, 2);

        // 各数字5つずつ用意しているので、最大類似度の和を算出する。
        var similarity = new double[10];
        Parallel.For(0, 10, i =>
        {
            lock (similarity) similarity[i] = 0;
            Parallel.For(0, 5, j =>
            {
                var fileName = string.Format("resources.digits.{0}.{1}.png", i, j);
                var stream = asm.GetManifestResourceStream(fileName);
                if (stream == null) throw new Exception(string.Format("The resource specified is not embedded. {0}", fileName));
                var template = Mat.FromStream(stream, ImreadModes.Unchanged);

                var result = digit.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
                result.MinMaxLoc(out double minVal, out double maxVal);

                lock (similarity) similarity[i] += maxVal;
            });
        });

        var max = similarity.Max();
        return (max / 5) < 0.8 ? null : Array.IndexOf(similarity, max);
    }
    /// <summary>
    /// いますぐバトルの手持ちを表す。
    /// </summary>
    private enum QuickBattleSide
    {
        /// <summary>
        /// プレイヤー側
        /// </summary>
        P1 = 0,
        /// <summary>
        /// COM側
        /// </summary>
        COM = 1
    }
    /// <summary>
    /// 埋め込み済みリソースの数字画像を用いて、テンプレートマッチングで手持ちの判定を行う。
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="side">どちら側の手持ちか</param>
    /// <returns></returns>
    private static int MatchIcon(this Mat mat, QuickBattleSide side)
    {
        var asm = Assembly.GetExecutingAssembly();

        var similarity = new double[5];
        Parallel.For(0, 5, i =>
        {
            var fileName = string.Format("resources.icons.{0}.{1}.png", (new string[] { "p1", "com" })[(int)side], i);
            var stream = asm.GetManifestResourceStream(fileName);
            if (stream == null) throw new FileNotFoundException(string.Format("The resource specified is not embedded. {0}", fileName));
            var template = Mat.FromStream(stream, ImreadModes.Color);

            var result = mat.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
            result.MinMaxLoc(out double minVal, out double maxVal);

            lock (similarity) similarity[i] = maxVal;
        });

        return Array.IndexOf(similarity, similarity.Max());
    }
    private static void RectangleDivided(this Mat mat, Rect rect, int divide, Scalar color, int thickness = 1, LineTypes lineType = LineTypes.Link8, int shift = 0)
    {
        for (var i = 0; i < divide; i++)
        {
            var tmp = new Rect(rect.X + (i == 0 ? 0 : rect.Width / divide * i), rect.Y, rect.Width / divide, rect.Height);
            mat.Rectangle(tmp, color, thickness, lineType, shift);
        }
    }
}

/// <summary>
/// 有効な数字が検出されなかった場合にthrowされる例外。
/// </summary>
[Serializable()]
public class NoValidDigitsException : Exception
{
    /// <summary>
    /// 既定のエラーメッセージ
    /// </summary>
    private static readonly string _message = "No valid digits were found.";
    /// <summary>
    /// NoValidDigitsExceptionクラスの新規インスタンスを初期化する。
    /// </summary>
    public NoValidDigitsException() : base(_message) { }
    /// <summary>
    /// NoValidDigitsExceptionクラスの新規インスタンスを初期化する。
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public NoValidDigitsException(string? message) : base(message == null ? _message : message) { }
    /// <summary>
    /// NoValidDigitsExceptionクラスの新規インスタンスを初期化する。
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    /// <returns></returns>
    public NoValidDigitsException(string? message, Exception inner) : base(message == null ? _message : message, inner) { }
    /// <summary>
    /// NoValidDigitsExceptionクラスの新規インスタンスを初期化する。
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected NoValidDigitsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
