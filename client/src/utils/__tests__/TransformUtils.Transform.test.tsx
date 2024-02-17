import { Operation } from "../../models/Operation";
import { transform } from "../TransformUtils";

type CaseInput = {
  op: Operation,
  history: Map<number, Operation[]>,
  expected: Operation[]
};

const casesSC: CaseInput[] = [
  {
    // "" - now revisionId 1
    op: op(1, 1, 1, 1, "1", 1,"1"),
    history: new Map([
      [0, [
        op(1, 1, 1, 1, "xxxxxxxxx", 0, "2")
      ]],
      [1, [
        op(1, 1, 1, 1, "cases", 1, "2")
      ]],
      [2, [
        op(5, 5, 1, 1, "B", 2, "2")
      ]],
      [3, [
        op(6, 6, 1, 1, "Q", 3, "2")
      ]],
    ]),
    expected: [op(8, 8, 1, 1, "1", 1, "1")]
    //"casesBQreso" - now revisionId 5
  },
  {
    // "Lorem ipsum" - now revisionId 0
    op: op(1, 12, 1, 1, "", 0, "2"),
    history: new Map([
      [1, [
        op(7, 12, 1, 1, "", 0, "3") // "Lorem "
      ]],
      [2, [
        op(7, 7, 1, 1, "X", 1, "3") // "Lorem X"
      ]],
    ]),
    expected: [op(1, 7, 1, 1, "", 1, "2")]
    //"X" - now revisionId 3
  },
];

describe("transform", () => {
  test.each(casesSC)('case $op.originatorId', ({op, history, expected}) => {
    expect(arraysAreEqual(transform(op, history), expected)).toEqual(true);
  });
});

function op(
  startColumn: number,
  endColumn: number,
  startLine: number,
  endLine: number,
  text: string,
  revisionId: number,
  originatorId?: string
): Operation {
  return {
    text: text,
    endColumn: endColumn,
    startColumn: startColumn,
    endLine: endLine,
    startLine: startLine,
    originatorId: originatorId,
    revisionId: revisionId,
  };
}

function arraysAreEqual(arr1: Operation[], arr2: Operation[]): boolean {
  if (arr1.length !== arr2.length) {
      return false;
  }

  for (let i = 0; i < arr1.length; i++) {
      if (!objectsAreEqual(arr1[i], arr2[i])) {
          return false;
      }
  }

  return true;
}

function objectsAreEqual(obj1: Operation, obj2: Operation): boolean {
  const keys: (keyof Operation)[] = ['text', 'endColumn', 'startColumn', 'endLine', 'startLine'];

  for (const key of keys) {
      if (obj1[key] !== obj2[key]) {
          return false;
      }
  }

  return true;
}
