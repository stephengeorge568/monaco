namespace test;
using Xunit;
using Moq;
using server.models;
using Monaco.Services.Interfaces;
using Monaco.Services;

public class IsPreviousOperationRelevantTests
{
    private readonly ITransformService ts;

    public IsPreviousOperationRelevantTests()
    {
        ts = new TransformService();
    }

    public static TheoryData<Operation, Operation, bool> IsPreviousOperationRelevantData = new TheoryData<Operation, Operation, bool> {
        { Op(1, 1, 1, 1, "1"), Op(1, 1, 1, 1, "2"), true },
        { Op(1, 1, 1, 1, "1"), Op(2, 2, 1, 1, "2"), true },
        { Op(1, 1, 2, 2, "1"), Op(1, 4, 1, 2, "2"), true },
        { Op(1, 3, 1, 2, "1"), Op(2, 4, 2, 2, "2"), true },
        { Op(2, 2, 1, 1, "1"), Op(9, 12, 1, 4, "2"), true },

        { Op(3, 3, 1, 1, "1"), Op(2, 2, 1, 1, "2"), false },
        { Op(1, 1, 6, 9, "1"), Op(2, 2, 2, 5, "2"), false },
        { Op(3, 3, 3, 3, "1"), Op(1, 3, 3, 3, "2"), false },
    };

    [Theory(DisplayName = "IsPreviousOperationRelevant"), MemberData(nameof(IsPreviousOperationRelevantData))]
    public void IsPreviousOperationRelevantTest(Operation prev, Operation next, bool expected)
    {
        Assert.Equal(ts.IsPreviousOperationRelevant(prev, next), expected);
    }

    private static Operation Op(int sc, int ec, int sl, int el, string id)
    {
        return new Operation
        {
            OriginatorId = id,
            StartColumn = sc,
            EndColumn = ec,
            StartLine = sl,
            EndLine = el
        };
    }
}
