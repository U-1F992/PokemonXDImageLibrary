# PokemonXDImageLibrary

「ポケモンXD 闇の旋風ダーク・ルギア」用 画像認識関連ライブラリ

## Summary

OpenCvSharp.Matクラスを拡張して画像を確認させる。

内部でリファレンス画像と条件を揃えるため、利用者はアスペクト比や余白について考慮する必要がない。

- ステータス実数値
- いますぐバトル生成結果

## Diagram

```mermaid
classDiagram
    class MatExtension {
        -int ReferenceWidth$
        -int ReferenceHeight$
        -double DefaultThreshold$

        -Trim(Mat mat, int threshold)$ Mat
        -MatchDigit(Mat mat)$ int
        -MatchIcon(Mat mat, QuickBattleSide side)$ int
        +Optimize(Mat mat)$ Mat
        -Optimize(Mat mat, int threshold)$ Mat
        +GetNumber(Mat mat, int numberOfDigits)$ int
        +GetStats(Mat mat)$ object
        +GetQuickBattleParties(Mat mat)$ object
    }
```

> GetStats returns (int HP, int Attack, int Defense, int Speed, int SpAtk, int SpDef)
> 
> GetQuickBattleParties returns ((int Index, int[] HP) P1, (int Index, int[] HP) COM)

## Note

- MatExtensionは `OpenCvSharp4` だけ参照させて、テストに `OpenCvSharp4.runtime.win` を参照させればライブラリはクロスプラットフォームになるはず...？