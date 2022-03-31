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
        -MatchDigit(Mat mat)$ int
        -MatchIcon(Mat mat, QuickBattleSide side)$ int
        -GetNumber(Mat mat, int numberOfDigits)$ int
        -GetNumbers(this Mat mat, Rect[] rects, int[] numberOfDigits, out Rect[] actual)$ int[]
        +GetStats(Mat mat)$ object
        +GetStats(Mat mat, Rect[] rects)$ object
        +GetQuickBattleParties(Mat mat)$ QuickBattleParties
        +GetQuickBattleParties(Mat mat, Rect[] rects)$ QuickBattleParties
    }
    class MatOptimizeExtension {
        -int ReferenceWidth$
        -int ReferenceHeight$
        -double DefaultThreshold$

        +Optimize(Mat mat)$ Mat
        -Optimize(Mat mat, int threshold)$ Mat
        -Trim(Mat mat, int threshold)$ Mat
        -TrimH(Mat mat, int threshold, out bool trimmed)$ Mat
        -TrimV(Mat mat, int threshold, out bool trimmed)$ Mat
    }
```

### Structures


```mermaid
classDiagram
    class QuickBattleParties {
        +QuickBattleParty P1
        +QuickBattleParty COM

        +QuickBattleParties(int pIndex, int eIndex, UInt32 HP)
        +QuickBattleParties(QuickBattleIndex_P1 pIndex, QuickBattleIndex_COM eIndex, UInt32 HP)
        +QuickBattleParties(int pIndex, int eIndex, int pHP_1, int pHP_2, int eHP_1, int eHP_2)
        +QuickBattleParties(QuickBattleIndex_P1 pIndex, QuickBattleIndex_COM eIndex, int pHP_1, int pHP_2, int eHP_1, int eHP_2)
    }
    class QuickBattleParty {
        +int Index
        +int[] HP

        +QuickBattleParty(int index, int hp_1, int hp_2)
    }
    class QuickBattleIndex_P1 {
        Mewtwo
        Mew
        Deoxys
        Rayquaza
        Jirachi
    }
    class QuickBattleIndex_COM {
        Articuno
        Zapdos
        Moltres
        Kangaskhan
        Latias
    }
```

> GetStats returns (int HP, int Attack, int Defense, int Speed, int SpAtk, int SpDef)

## Note

- [テスト画像](https://drive.google.com/drive/folders/1VmbFF6gG3jAFbgS2-pjO5bKdyyW2Gfuk?usp=sharing)
- MatExtensionは `OpenCvSharp4` だけ参照させて、テストに `OpenCvSharp4.runtime.win` を参照させればライブラリはクロスプラットフォームになるはず...？
