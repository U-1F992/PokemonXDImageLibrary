using Xunit;

namespace MatExtension.Tests;

using System.IO;
using OpenCvSharp;
using PokemonXDImageLibrary;

public class MatExtension_Tests
{
    static readonly string TestImageRoot = @"D:\Documents\PokemonXDImageLibrary\MatExtension.Tests\img\";

    [Theory]
    [InlineData(@"emulator\stats\1.png", 159, 100, 89, 186, 133, 161)]
    [InlineData(@"emulator\stats\11.png", 153, 100, 94, 136, 110, 108)]
    [InlineData(@"emulator\stats\12.png", 159, 115, 98, 123, 99, 105)]
    [InlineData(@"emulator\stats\13.png", 160, 90, 132, 105, 142, 105)]
    [InlineData(@"emulator\stats\14.png", 181, 85, 165, 110, 174, 130)]
    [InlineData(@"wii\4-3\stats\1.png", 105, 103, 64, 59, 63, 64)]
    [InlineData(@"wii\16-9\stats\1.png", 159, 100, 89, 186, 133, 161)]
    [InlineData(@"gc-rca-hdmi\4-3\stats\1.png", 153, 100, 94, 136, 110, 108)]
    [InlineData(@"gc-rca-hdmi\4-3\stats\2.png", 159, 115, 98, 123, 99, 105)]
    [InlineData(@"gc-rca-hdmi\4-3\stats\3.png", 160, 90, 132, 105, 142, 105)]
    [InlineData(@"gc-rca-hdmi\16-9\stats\1.png", 46, 19, 27, 25, 31, 44)]
    [InlineData(@"gc-progressive-gcplug\4-3\stats\1.png", 201, 110, 55, 61, 105, 45)]
    [InlineData(@"gc-rca\4-3\stats\1.png", 201, 110, 55, 61, 105, 45)]
    [InlineData(@"gc-separate\4-3\stats\1.png", 201, 110, 55, 61, 105, 45)]
    public void Implement_GetStats(string fileName, int h, int a, int b, int c, int d, int s)
    {
        var ret = new Mat(Path.Join(MatExtension_Tests.TestImageRoot, fileName)).GetStats();
        Assert.Equal(h, ret.HP);
        Assert.Equal(a, ret.Attack);
        Assert.Equal(b, ret.Defense);
        Assert.Equal(c, ret.SpAtk);
        Assert.Equal(d, ret.SpDef);
        Assert.Equal(s, ret.Speed);
    }

    [Theory]
    [InlineData(@"emulator\quickbattle\1.png", 3, 3, 366, 239, 365, 291)]
    [InlineData(@"wii\4-3\quickbattle\1.png", 2, 3, 253, 650, 367, 316)]
    [InlineData(@"wii\16-9\quickbattle\1.png", 3, 2, 340, 271, 326, 289)]
    [InlineData(@"gc-rca-hdmi\4-3\quickbattle\1.png", 4, 0, 344, 357, 372, 359)]
    [InlineData(@"gc-rca-hdmi\4-3\quickbattle\2.png", 4, 2, 345, 340, 332, 271)]
    [InlineData(@"gc-rca-hdmi\4-3\quickbattle\3.png", 4, 0, 329, 358, 294, 365)]
    [InlineData(@"gc-rca-hdmi\16-9\quickbattle\1.png", 2, 1, 257, 685, 343, 287)]
    [InlineData(@"gc-progressive-gcplug\4-3\quickbattle\1.png", 1, 4, 349, 318, 286, 262)]
    [InlineData(@"gc-rca\4-3\quickbattle\1.png", 2, 1, 245, 706, 342, 278)]
    public void Implement_GetQuickBattleParties(string fileName, int pIndex, int eIndex, int pHP_1, int pHP_2, int eHP_1, int eHP_2)
    {
        var ret = new Mat(Path.Join(MatExtension_Tests.TestImageRoot, fileName)).GetQuickBattleParties();
        Assert.Equal(pIndex, ret.P1.Index);
        Assert.Equal(eIndex, ret.COM.Index);
        Assert.Equal(pHP_1, ret.P1.HP[0]);
        Assert.Equal(pHP_2, ret.P1.HP[1]);
        Assert.Equal(eHP_1, ret.COM.HP[0]);
        Assert.Equal(eHP_2, ret.COM.HP[1]);
    }
}