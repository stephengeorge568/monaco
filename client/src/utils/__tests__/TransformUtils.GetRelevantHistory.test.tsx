import { Operation } from "../../models/Operation";
import { getRelevantHistory } from "../TransformUtils";

type CaseInput = {
  revId: number,
  history: Map<number, Operation[]>,
  expected: Operation[]
};

const history: Map<number, Operation[]> = new Map([
  [1, [
    op(1, 1, 1, 1, "1")
  ]],
  [2, [
    op(1, 1, 1, 1, "2")
  ]],
  [3, [
    op(1, 1, 1, 1, "3")
  ]],
  [4, [
    op(1, 1, 1, 1, "4")
  ]],
]);

const cases: CaseInput[] = [
  {revId: 2, history: history, expected: [op(1, 1, 1, 1, "2"), op(1, 1, 1, 1, "3"), op(1, 1, 1, 1, "4")]},
];

describe("getRelevantHistory", () => {
  test.each(cases)('given case revId', ({revId, history, expected}) => {
    expect(getRelevantHistory(revId, history)).toStrictEqual(expected);
  });
});

function op(
  startColumn: number,
  endColumn: number,
  startLine: number,
  endLine: number,
  originatorId: string,
): Operation {
  return {
    revisionId: -1,
    text: "irrelevant",
    endColumn: endColumn,
    startColumn: startColumn,
    endLine: endLine,
    startLine: startLine,
    originatorId: originatorId,
  };
}
