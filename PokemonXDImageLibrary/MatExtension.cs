using System.Reflection;
using OpenCvSharp;

namespace PokemonXDImageLibrary;

/// <summary>
/// <see cref="OpenCvSharp.Mat"/>クラスに対して、「ポケモンXD 闇の旋風ダーク・ルギア」のゲーム画面キャプチャ画像を扱う拡張メソッドを提供します。
/// </summary>
public static class MatExtension
{
    private static readonly int ReferenceWidth = 1352;
    private static readonly int ReferenceHeight = 1080;
    private static readonly double DefaultThreshold = 0.00175;

    /// <summary>
    /// キャリブレーション元の画像群と条件を揃える。<br/>
    /// 1. 上下左右の余白部分をいい感じに切り落とす。<br/>
    /// 2. 入力画像のサイズをリファレンスに合わせる。
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static Mat Optimize(this Mat mat) { return mat.Optimize(MatExtension.DefaultThreshold); }
    /// <summary>
    /// キャリブレーション元の画像群と条件を揃える。<br/>
    /// 1. 上下左右の余白部分をいい感じに切り落とす。<br/>
    /// 2. 入力画像のサイズをリファレンスに合わせる。
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="threshold">
    /// 同色と見なす色差(0~1)<br/>
    /// 0に設定するとRGB完全一致のみ、1に設定すると全ての色を同色として扱います。
    /// </param>
    /// <returns></returns>
    private static Mat Optimize(this Mat mat, double threshold)
    {
        var result = mat.Trim(threshold);
        result = result.Resize(new Size(MatExtension.ReferenceWidth, MatExtension.ReferenceHeight));

#if DEBUG
        result.SaveImage(string.Format("optimized-{0}.png", DateTime.Now.Ticks));
#endif
        return result;
    }
    /// <summary>
    /// 上下左右の余白部分をいい感じに切り落とす。<br/>
    /// <see href="https://qiita.com/yoya/items/62879e6e03d5a70eed09#-trim"/><br/>
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="threshold">
    /// 同色と見なす色差(0~1)<br/>
    /// 0に設定するとRGB完全一致のみ、1に設定すると全ての色を同色として扱います。
    /// </param>
    /// <returns></returns>
    private static Mat Trim(this Mat mat, double threshold)
    {
        var result = mat;

        // 一番左上のピクセルの色を基準にして、画像の左の列から色が一致したら狭める
        var flag = false;
        var shave = 0;
        var edge = result.At<Vec3b>(0, 0);
        var column = 0;
        var cts = new CancellationTokenSource();
        while (!flag && column < result.Width)
        {
            try
            {
                Parallel.For(0, result.Height, new ParallelOptions() { CancellationToken = cts.Token }, row =>
                {
                    if (GetDistance(result.At<Vec3b>(row, column), edge) > threshold)
                    {
                        flag = true;
                        cts.Cancel();
                    }
                });
            }
            catch (OperationCanceledException) { }

            shave = column;
            column++;
        }
        result = result.Clone(new Rect(shave == 0 ? 0 : shave - 1, 0, result.Width - shave, result.Height));

        // 一番左上のピクセルの色を基準にして、画像の上の行から色が一致したら狭める
        flag = false;
        shave = 0;
        edge = result.At<Vec3b>(0, 0);
        var row = 0;
        cts = new CancellationTokenSource();
        while (!flag && row < result.Height)
        {
            try
            {
                Parallel.For(0, result.Width, new ParallelOptions() { CancellationToken = cts.Token }, column =>
                {
                    if (GetDistance(result.At<Vec3b>(row, column), edge) > threshold)
                    {
                        flag = true;
                        cts.Cancel();
                    }
                });
            }
            catch (OperationCanceledException) { }

            shave = row;
            row++;
        }
        result = result.Clone(new Rect(0, shave == 0 ? 0 : shave - 1, result.Width, result.Height - shave));

        // 一番右上のピクセルを基準にして、画像の右の列から色が一致したら狭める
        flag = false;
        shave = 0;
        edge = result.At<Vec3b>(0, result.Width - 1);
        column = result.Width - 1;
        cts = new CancellationTokenSource();
        while (!flag && 0 < column)
        {
            try
            {
                Parallel.For(0, result.Height, new ParallelOptions() { CancellationToken = cts.Token }, row =>
                {
                    if (GetDistance(result.At<Vec3b>(row, column), edge) > threshold)
                    {
                        flag = true;
                        cts.Cancel();
                    }
                });
            }
            catch (OperationCanceledException) { }

            shave = column;
            column--;
        }
        result = result.Clone(new Rect(0, 0, shave + 1, result.Height));

        // 一番左下のピクセルを基準にして、画像の下の行から色が一致したら狭める
        flag = false;
        shave = 0;
        edge = result.At<Vec3b>(result.Height - 1, 0);
        row = result.Height - 1;
        cts = new CancellationTokenSource();
        while (!flag && 0 < row)
        {
            try
            {
                Parallel.For(0, result.Width, new ParallelOptions() { CancellationToken = cts.Token }, column =>
                {
                    if (GetDistance(result.At<Vec3b>(row, column), edge) > threshold)
                    {
                        flag = true;
                        cts.Cancel();
                    }
                });
            }
            catch (OperationCanceledException) { }

            shave = row;
            row--;
        }
        result = result.Clone(new Rect(0, 0, result.Width, shave + 1));

        return result;

        double GetDistance(Vec3b color1, Vec3b color2)
        {
            (double r, double g, double b) c1 = ((int)color1.Item2 / 255.0, (int)color1.Item1 / 255.0, (int)color1.Item0 / 255.0);
            (double r, double g, double b) c2 = ((int)color2.Item2 / 255.0, (int)color2.Item1 / 255.0, (int)color2.Item0 / 255.0);

            // ユークリッド距離を求める
            // https://ja.wikipedia.org/wiki/%E8%89%B2%E5%B7%AE
            return (Math.Pow((c1.r - c2.r), 2) + Math.Pow((c1.g - c2.g), 2) + Math.Pow((c1.b - c2.b), 2)) / 3;
        }
    }
    /// <summary>
    /// 個体実数値を取得する。
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static (int HP, int Attack, int Defense, int Speed, int SpAtk, int SpDef) GetStats(this Mat mat)
    {
        var source = mat.Optimize();
        var mats = new Mat[]
        {
            source.Clone(new Rect(1145, 209, 111, 42)),
            source.Clone(new Rect(1168, 337, 111, 42)),
            source.Clone(new Rect(1168, 415, 111, 42)),
            source.Clone(new Rect(1168, 643, 111, 42)),
            source.Clone(new Rect(1168, 489, 111, 42)),
            source.Clone(new Rect(1168, 569, 111, 42))
        };

        // currentHP 1000,210,108,40
        var hp = mats[0].GetNumber(3);
        var attack = mats[1].GetNumber(3);
        var defense = mats[2].GetNumber(3);
        var speed = mats[3].GetNumber(3);
        var spatk = mats[4].GetNumber(3);
        var spdef = mats[5].GetNumber(3);

#if DEBUG
        using (var w0 = new Window())
        using (var w1 = new Window())
        using (var w2 = new Window())
        using (var w3 = new Window())
        using (var w4 = new Window())
        using (var w5 = new Window())
        using (var w6 = new Window())
        {
            w0.ShowImage(source);
            w1.ShowImage(mats[0]);
            w2.ShowImage(mats[1]);
            w3.ShowImage(mats[2]);
            w4.ShowImage(mats[3]);
            w5.ShowImage(mats[4]);
            w6.ShowImage(mats[5]);
            Cv2.WaitKey(2000);
        }
#endif

        return (hp, attack, defense, speed, spatk, spdef);
    }
    /// <summary>
    /// いますぐバトル生成結果を取得する。
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static ((int Index, int[] HP) P1, (int Index, int[] HP) COM) GetQuickBattleParties(this Mat mat)
    {
        var source = mat.Optimize();
        var mats = new Mat[]
        {
            source.Clone(new Rect(328, 689, 111, 42)),
            source.Clone(new Rect(328, 898, 111, 42)),
            source.Clone(new Rect(955, 689, 111, 42)),
            source.Clone(new Rect(958, 898, 111, 42)),
            source.Clone(new Rect(115, 606, 104, 111)),
            source.Clone(new Rect(742, 606, 104, 111))
        };

        ((int Index, int[] HP) P1, (int Index, int[] HP) COM) result;
        result.P1.HP = new int[]
        {
            mats[0].GetNumber(3),
            mats[1].GetNumber(3)
        };
        result.COM.HP = new int[]
        {
            mats[2].GetNumber(3),
            mats[3].GetNumber(3)
        };

        result.P1.Index = mats[4].MatchIcon(QuickBattleSide.P1);
        result.COM.Index = mats[5].MatchIcon(QuickBattleSide.COM);

#if DEBUG
        using (var w0 = new Window())
        using (var w1 = new Window())
        using (var w2 = new Window())
        using (var w3 = new Window())
        using (var w4 = new Window())
        using (var w5 = new Window())
        using (var w6 = new Window())
        {
            w0.ShowImage(source);
            w1.ShowImage(mats[0]);
            w2.ShowImage(mats[1]);
            w3.ShowImage(mats[2]);
            w4.ShowImage(mats[3]);
            w5.ShowImage(mats[4]);
            w6.ShowImage(mats[5]);
            Cv2.WaitKey(-1);
        }
#endif
        return result;
    }
    /// <summary>
    /// 右揃えで高々指定された桁数の数値を検出する。
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="numberOfDigits">桁数</param>
    /// <returns></returns>
    public static int GetNumber(this Mat mat, int numberOfDigits)
    {
        // 横に等分する範囲のリスト
        var rects = new List<Rect>();
        for (var i = 0; i < numberOfDigits; i++) rects.Add(new Rect(mat.Width / numberOfDigits * i, 0, mat.Width / numberOfDigits, mat.Height));

        // 各範囲で数字の判定を行う
        var digits = new int?[numberOfDigits];
        Parallel.For(0, numberOfDigits, i => { digits[i] = (mat.Clone(rects[i]).MatchDigit()); });

        // 最小の桁がnullは異常
        if (digits[digits.Length - 1] == null) throw new Exception("No valid digits were found.");
        // 数字が検出された後にnullが検出されていたら異常
        var digitDetected = false;
        for (var i = 0; i < digits.Length; i++)
        {
            if (digits[i] != null && !digitDetected) digitDetected = true;
            else if (digits[i] == null && digitDetected) throw new Exception("No valid digits were found.");
        }

        // 各範囲の数字検出の結果から、最終的な数値を捻出する
        var resultString = "";
        foreach (var digit in digits) resultString += (digit == null ? string.Empty : digit.ToString());
        return Convert.ToInt32(resultString);
    }
    /// <summary>
    /// 埋め込み済みリソースの数字画像を用いて、テンプレートマッチングで数字の判定を行う。<br/>
    /// 最大類似度が0.5未満の場合は、未検出としてnullを返す。
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
            similarity[i] = 0;
            Parallel.For(0, 5, j =>
            {
                var fileName = string.Format("resources.digits.{0}.{1}.png", i, j);
                var stream = asm.GetManifestResourceStream(fileName);
                if (stream == null) throw new Exception(string.Format("The resource specified is not embedded. {0}", fileName));
                var template = Mat.FromStream(stream, ImreadModes.Unchanged);

                var result = digit.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
                result.MinMaxLoc(out double minVal, out double maxVal);

                similarity[i] += maxVal;
            });
        });

        var max = similarity.Max();
        return max < 0.5 ? null : Array.IndexOf(similarity, max);
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
            if (stream == null) throw new Exception(string.Format("The resource specified is not embedded. {0}", fileName));
            var template = Mat.FromStream(stream, ImreadModes.Color);

            var result = mat.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
            result.MinMaxLoc(out double minVal, out double maxVal);

            similarity[i] = maxVal;
        });

        var max = similarity.Max();
        return Array.IndexOf(similarity, max);
    }
}
