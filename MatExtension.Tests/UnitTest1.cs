using Xunit;

namespace MatExtension.Tests;

using System.IO;
using OpenCvSharp;
using PokemonXDImageLibrary;

public class MatExtension_Tests
{
    static readonly string TestImageRoot = @"D:\Documents\PokemonXDImageLibrary\MatExtension.Tests\img\";

    [Theory]
    [InlineData(@"emulator\quickbattle\1.png")]
    [InlineData(@"wii\4-3\quickbattle\1.png")]
    [InlineData(@"wii\16-9\quickbattle\1.png")]
    [InlineData(@"wii\4-3\stats\1.png")]
    [InlineData(@"wii\16-9\stats\1.png")]
    public void Implement_Optimize(string fileName)
    {
        new Mat(Path.Join(MatExtension_Tests.TestImageRoot, fileName)).Optimize();
    }

    [Theory]
    [InlineData(@"quickbattle\229.png", 229)]
    [InlineData(@"quickbattle\341.png", 341)]
    [InlineData(@"quickbattle\348.png", 348)]
    [InlineData(@"quickbattle\376.png", 376)]
    [InlineData(@"quickbattle\395.png", 395)]
    [InlineData(@"stats\18.png", 18)]
    [InlineData(@"stats\28.png", 28)]
    [InlineData(@"stats\50.png", 50)]
    [InlineData(@"stats\54.png", 54)]
    [InlineData(@"stats\107.png", 107)]
    [InlineData(@"stats\111.png", 111)]
    [InlineData(@"stats\255.png", 255)]
    public void Implement_GetNumber(string fileName, int expected)
    {
        var path = Path.Join(MatExtension_Tests.TestImageRoot, @"\number\", fileName);
        Assert.Equal(expected, new Mat(path, ImreadModes.Unchanged).GetNumber(3));
    }

    [Theory]
    [InlineData(@"emulator\stats\1.png", 159, 100, 89, 186, 133, 161)]
    [InlineData(@"emulator\stats\11.png", 153, 100, 94, 136, 110, 108)]
    [InlineData(@"emulator\stats\12.png", 159, 115, 98, 123, 99, 105)]
    [InlineData(@"emulator\stats\13.png", 160, 90, 132, 105, 142, 105)]
    [InlineData(@"emulator\stats\14.png", 181, 85, 165, 110, 174, 130)]
    [InlineData(@"wii\4-3\stats\1.png", 105, 103, 64, 59, 63, 64)]
    [InlineData(@"wii\16-9\stats\1.png", 159, 100, 89, 186, 133, 161)]
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