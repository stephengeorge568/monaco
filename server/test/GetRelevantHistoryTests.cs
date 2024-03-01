namespace test;
using Xunit;
using Moq;
using server.models;
using Monaco.Services.Interfaces;
using Monaco.Services;

public class GetRelevantHistoryTests
{
    private readonly ITransformService ts;

    public GetRelevantHistoryTests()
    {
        ts = new TransformService();
    }


    [Fact]
    public void GetRelevantHistoryTest()
    {
        Dictionary<int, List<Operation>> history = new(){
            { 1, [ Op(1, 1, 1, 1, "1") ] },
            { 2, [ Op(1, 1, 1, 1, "2"),  Op(1, 1, 1, 1, "3") ] },
            { 3, [ Op(1, 1, 1, 1, "4") ] },
            { 4, [ Op(1, 1, 1, 1, "5") ] }
        };

        var expected = new List<Operation>() {
            Op(1, 1, 1, 1, "2"),
            Op(1, 1, 1, 1, "3"),
            Op(1, 1, 1, 1, "4"),
            Op(1, 1, 1, 1, "5")
        };
        Assert.Equivalent(expected, ts.GetRelevantHistory(2, history), true);
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
