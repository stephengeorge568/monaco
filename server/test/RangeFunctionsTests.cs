namespace test;
using Xunit;
using Moq;
using server.models;
using Monaco.Services.Interfaces;
using Monaco.Services;

public class RangeFunctionsTests
{
    private readonly ITransformService ts;

    public RangeFunctionsTests()
    {
        ts = new TransformService();
    }

    public static TheoryData<Operation, Operation, bool> IsECWithinRangeData = new TheoryData<Operation, Operation, bool> {
        { Op(2, 5, 1, 1), Op(2, 4, 1, 1), true },
        { Op(4, 8, 1, 2), Op(1, 5, 1, 2), true },
        { Op(4, 8, 1, 1), Op(1, 4, 1, 1), false },
        { Op(1, 1, 1, 1), Op(1, 1, 1, 1), false },
    };

    [Theory(DisplayName = "IsECWithinRange"), MemberData(nameof(IsECWithinRangeData))]
    public void IsECWithinRangeTests(Operation prev, Operation next, bool expected)
    {
        Assert.Equal(ts.IsECWithinRange(prev, next), expected);
    }

    public static TheoryData<Operation, Operation, bool> IsSCWithinRangeData = new TheoryData<Operation, Operation, bool> {
        { Op(1, 3, 1, 1), Op(1, 1, 1, 1), true },
        { Op(1, 3, 1, 3), Op(2, 2, 3, 3), true },
        { Op(1, 2, 1, 2), Op(4, 6, 1, 2), true },
        { Op(1, 2, 1, 3), Op(4, 6, 1, 2), true },

        { Op(1, 1, 1, 1), Op(1, 1, 1, 1), false },
        { Op(1, 1, 1, 1), Op(2, 2, 1, 1), false },
        { Op(1, 3, 1, 1), Op(1, 1, 4, 4), false },
        { Op(1, 3, 1, 1), Op(3, 3, 1, 1), false },
    };

    [Theory(DisplayName = "IsECWithinRange"), MemberData(nameof(IsSCWithinRangeData))]
    public void IsSCWithinRangeTests(Operation prev, Operation next, bool expected)
    {
        Assert.Equal(ts.IsSCWithinRange(prev, next), expected);
    }

    private static Operation Op(int sc, int ec, int sl, int el)
    {
        return new Operation
        {
            StartColumn = sc,
            EndColumn = ec,
            StartLine = sl,
            EndLine = el
        };
    }
}
