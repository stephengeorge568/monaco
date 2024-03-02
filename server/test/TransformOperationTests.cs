namespace test;
using Xunit;
using Moq;
using server.models;
using Monaco.Services.Interfaces;
using Monaco.Services;

public class TransformOperationTests
{
    private readonly ITransformService ts;

    public TransformOperationTests()
    {
        ts = new TransformService();
    }

    public static TheoryData<Operation, Operation, Operation> TransformOperationData = new TheoryData<Operation, Operation, Operation> {
        { Op(1, 1, 1, 1, "1", "1"), Op(1, 1, 1, 1, "2", ""), Op(2, 2, 1, 1, "2", "") },
        { Op(1, 1, 1, 1, "1a", "2"), Op(1, 1, 1, 1, "2", ""), Op(3, 3, 1, 1, "2", "") },
        { Op(1, 1, 1, 1, "1a", "3"), Op(5, 5, 1, 1, "2", ""), Op(7, 7, 1, 1, "2", "") },
        { Op(1, 1, 1, 1, "1a", "4"), Op(5, 5, 3, 3, "2", ""), Op(5, 5, 3, 3, "2", "") },
        { Op(1, 1, 1, 1, "1a", "5"), Op(5, 5, 3, 3, "2", ""), Op(5, 5, 3, 3, "2", "") },
        { Op(2, 2, 1, 1, "1a", "6"), Op(1, 1, 1, 1, "2", ""), Op(1, 1, 1, 1, "2", "") },
        { Op(1, 1, 1, 1, "34\n", "7"), Op(1, 1, 1, 1, "2", ""), Op(1, 1, 2, 2, "2", "") },
        { Op(1, 1, 1, 1, "34\n234", "8"), Op(1, 1, 1, 1, "2", ""), Op(4, 4, 2, 2, "2", "") },
        { Op(1, 1, 1, 1, "34\n234", "9"), Op(1, 1, 1, 1, "2", ""), Op(4, 4, 2, 2, "2", "") },
        { Op(1, 1, 1, 1, "34\n234\n2", "10"), Op(1, 1, 1, 1, "2", ""), Op(2, 2, 3, 3, "2", "") },
        { Op(2, 2, 1, 1, "34\n234\n\n2", "11"), Op(1, 1, 1, 1, "2", ""), Op(1, 1, 1, 1, "2", "") },
        { Op(1, 1, 1, 1, "34\n234\n\n2", "12"), Op(4, 4, 4, 8, "2", ""), Op(4, 4, 7, 11, "2", "") },
        { Op(1, 1, 1, 1, "34\r\n234\r\n\r\n2", "13"), Op(4, 4, 4, 8, "2", ""), Op(4, 4, 7, 11, "2", "") },
        { Op(1, 1, 1, 3, "34\r\n234\r\n\r\n2", "14"), Op(4, 4, 4, 8, "2", ""), Op(4, 4, 5, 9, "2", "") },
    };

    [Theory(DisplayName = "TransformOperation"), MemberData(nameof(TransformOperationData))]
    public void TransformOperationTest(Operation prev, Operation next, Operation expected)
    {
        Assert.Equivalent(ts.TransformOperation(prev, next), expected, true);
    }

    private static Operation Op(int sc, int ec, int sl, int el, string text, string originatorId)
    {
        return new Operation
        {
            StartColumn = sc,
            EndColumn = ec,
            StartLine = sl,
            EndLine = el,
            Text = text,
            RevisionId = -1,
            OriginatorId = originatorId
        };
    }
}
