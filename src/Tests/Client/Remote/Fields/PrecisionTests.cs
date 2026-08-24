namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class PrecisionTests
{
    [Fact]
    public void SizeInBits_IsEight()
    {
        Precision.Microsecond.SizeInBits.ShouldBe(8);
    }

    [Fact]
    public void Microsecond_HasValueNegativeEighteen()
    {
        Precision.Microsecond.Value.ShouldBe((sbyte)-18);
    }

    [Theory]
    [InlineData((sbyte)-18)]
    [InlineData((sbyte)-10)]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)10)]
    [InlineData((sbyte)127)]
    [InlineData((sbyte)-128)]
    public void Reconstitute_RoundTrip_PreservesValue(sbyte value)
    {
        var precision = Precision.Reconstitute(value);
        precision.Value.ShouldBe(value);
    }

    [Theory]
    [InlineData((sbyte)-18)]
    [InlineData((sbyte)-10)]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)10)]
    public void Encode_ProducesSingleByteWithCorrectValue(sbyte value)
    {
        var precision = Precision.Reconstitute(value);
        var bytes = precision.Encode();

        bytes.Length.ShouldBe(1);
        ((int)bytes[0]).ShouldBe((byte)value);
    }

    [Fact]
    public void Encode_WithNegativeValue_ProducesTwoComplementByte()
    {
        var precision = Precision.Reconstitute(-18);
        var bytes = precision.Encode();

        ((int)bytes[0]).ShouldBe(unchecked((byte)-18));
    }

    [Fact]
    public void ToString_WithMicrosecond_ProducesApproximatelyOneMicrosecond()
    {
        var str = Precision.Microsecond.ToString();

        var value = double.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
        var microsecond = 1e-6;
        Math.Abs(value - microsecond).ShouldBeLessThan(1e-5);
    }

    [Theory]
    [InlineData((sbyte)0, "1.00e+000")]
    [InlineData((sbyte)1, "2.00e+000")]
    [InlineData((sbyte)10, "1.02e+003")]
    public void ToString_WithPositiveValue_ReturnsScientificNotation(sbyte value, string expected)
    {
        Precision.Reconstitute(value).ToString().ShouldBe(expected);
    }
}
