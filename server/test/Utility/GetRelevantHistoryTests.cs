namespace test;
using Xunit;
using server.models;
using Monaco.Utility;

public class GetRelevantHistoryTests
{

    public GetRelevantHistoryTests()
    {
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
            Op(1, 1, 1, 1, "4"),
            Op(1, 1, 1, 1, "5")
        };
        Assert.Equivalent(expected, Transformer.GetRelevantHistory(2, history), true);
    }

    private static Operation Op(int sc, int ec, int sl, int el, string id)
    {
        return new Operation
        {
            Id = "Id_Not_Relevant",
            OriginatorId = id,
            StartColumn = sc,
            EndColumn = ec,
            StartLine = sl,
            EndLine = el
        };
    }
}
