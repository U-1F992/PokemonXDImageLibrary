using OpenCvSharp;

namespace PokemonXDImageLibrary;

/// <summary>
/// <see cref="OpenCvSharp.Mat"/>クラスに対して、「ポケモンXD 闇の旋風ダーク・ルギア」のゲーム画面キャプチャ画像の余白を自動でトリミング / 変形して情報取得用に最適化する拡張メソッドを提供します。<br/>
/// <see href="https://qiita.com/yoya/items/62879e6e03d5a70eed09#-trim"/><br/>
/// </summary>
public static class MatOptimizeExtension
{
    private static readonly int ReferenceWidth = 1352;
    private static readonly int ReferenceHeight = 1080;
    private static readonly double DefaultThreshold = 0.005;

    /// <summary>
    /// キャリブレーション元の画像群と条件を揃える。<br/>
    /// 1. 上下左右の余白部分をいい感じに切り落とす。<br/>
    /// 2. 入力画像のサイズをリファレンスに合わせる。
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static Mat Optimize(this Mat mat) { return mat.Optimize(DefaultThreshold); }
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
        result = result.Resize(new Size(ReferenceWidth, ReferenceHeight));

#if DEBUG
        result.SaveImage(string.Format("optimized-{0}.png", DateTime.Now.Ticks));
#endif
        return result;
    }
    /// <summary>
    /// 上下左右の余白部分をいい感じに切り落とす。
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="threshold">
    /// 同色と見なす色差(0~1)<br/>
    /// 0に設定するとRGB完全一致のみ、1に設定すると全ての色を同色として扱います。
    /// </param>
    /// <returns></returns>
    private static Mat Trim(this Mat mat, double threshold)
    {
        var result = mat.Clone();

        bool trimmed = true;
        while (trimmed) result = result.TrimH(threshold, out trimmed);
        result = result.TrimV(threshold, out var _);

        return result;
    }
    private static Mat TrimH(this Mat mat, double threshold, out bool trimmed)
    {
        var result = mat.Clone();

        // 一番左上のピクセルの色を基準にして、画像の左の列から色が一致したら狭める
        var taskL = Task.Run<int>(() =>
        {
            var shave = 0;
            var column = 0;
            var edge = result.At<Vec3b>(0, column);
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Parallel.For(0, result.Height, new ParallelOptions() { CancellationToken = ct }, row =>
                    {
                        if (GetDistance(result.At<Vec3b>(row, column + shave), edge) > threshold)
                        {
                            cts.Cancel();
                        }
                    });
                }
                catch (OperationCanceledException) { }
                if (!ct.IsCancellationRequested) shave++;
            }
            return shave;
        });

        // 一番右上のピクセルを基準にして、画像の右の列から色が一致したら狭める
        var taskR = Task.Run<int>(() =>
        {
            var shave = 0;
            var column = result.Width - 1;
            var edge = result.At<Vec3b>(0, column);
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Parallel.For(0, result.Height, new ParallelOptions() { CancellationToken = ct }, row =>
                    {
                        if (GetDistance(result.At<Vec3b>(row, column - shave), edge) > threshold)
                        {
                            cts.Cancel();
                        }
                    });
                }
                catch (OperationCanceledException) { }
                if (!ct.IsCancellationRequested) shave++;
            }
            return shave;
        });

        (int L, int R) shave = (taskL.Result, taskR.Result);
        result = result.Clone(new Rect(shave.L, 0, result.Width - shave.L, result.Height));
        result = result.Clone(new Rect(0, 0, result.Width - shave.R, result.Height));

        trimmed = mat.Width != result.Width ? true : false;
        return result;
    }
    private static Mat TrimV(this Mat mat, double threshold, out bool trimmed)
    {
        var result = mat.Clone();

        // 一番左上のピクセルの色を基準にして、画像の上の行から色が一致したら狭める
        var taskU = Task.Run<int>(() =>
        {
            var shave = 0;
            var row = 0;
            var edge = result.At<Vec3b>(row, 0);
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Parallel.For(0, result.Width, new ParallelOptions() { CancellationToken = ct }, column =>
                    {
                        if (GetDistance(result.At<Vec3b>(row + shave, column), edge) > threshold) cts.Cancel();
                    });
                }
                catch (OperationCanceledException) { }
                if (!ct.IsCancellationRequested) shave++;
            }
            return shave;
        });

        // 一番左下のピクセルを基準にして、画像の下の行から色が一致したら狭める
        var taskD = Task.Run<int>(() =>
        {
            var shave = 0;
            var row = result.Height - 1;
            var edge = result.At<Vec3b>(row, 0);
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Parallel.For(0, result.Width, new ParallelOptions() { CancellationToken = ct }, column =>
                    {
                        if (GetDistance(result.At<Vec3b>(row - shave, column), edge) > threshold) cts.Cancel();
                    });
                }
                catch (OperationCanceledException) { }
                if (!ct.IsCancellationRequested) shave++;
            }
            return shave;
        });

        (int U, int D) shave = (taskU.Result, taskD.Result);
        result = result.Clone(new Rect(0, shave.U, result.Width, result.Height - shave.U));
        result = result.Clone(new Rect(0, 0, result.Width, result.Height - shave.D));

        trimmed = mat.Height != result.Height ? true : false;
        return result;
    }
    private static double GetDistance(Vec3b color1, Vec3b color2)
    {
        (double r, double g, double b) c1 = ((int)color1.Item2 / 255.0, (int)color1.Item1 / 255.0, (int)color1.Item0 / 255.0);
        (double r, double g, double b) c2 = ((int)color2.Item2 / 255.0, (int)color2.Item1 / 255.0, (int)color2.Item0 / 255.0);

        // ユークリッド距離を求める
        // https://ja.wikipedia.org/wiki/%E8%89%B2%E5%B7%AE
        return (Math.Pow((c1.r - c2.r), 2) + Math.Pow((c1.g - c2.g), 2) + Math.Pow((c1.b - c2.b), 2)) / 3;
    }
}