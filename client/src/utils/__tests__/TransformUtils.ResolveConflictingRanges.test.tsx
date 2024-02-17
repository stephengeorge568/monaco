import { Operation } from "../../models/Operation";
import { resolveConflictingRanges } from "../TransformUtils";

type CaseInput = {
  prev: Operation,
  next: Operation,
  expected: Operation[]
};

const casesSC: CaseInput[] = [
  {prev: op(1, 1, 1, 1, "1", "1"), next: op(1, 1, 1, 1, "2"), expected: [op(1, 1, 1, 1, "2")]},
  {prev: op(1, 4, 1, 1, "1", "2"), next: op(2, 6, 1, 1, "2"), expected: [op(4, 6, 1, 1, "2")]},
  {prev: op(6, 10, 1, 1, "1", "3"), next: op(8, 8, 1, 1, "2"), expected: [op(8, 8, 1, 1, "2")]},
  {prev: op(10, 10, 1, 1, "1", "4"), next: op(2, 14, 1, 1, "2"), expected: [op(2, 14, 1, 1, "2")]},
  {prev: op(6, 11, 1, 1, "1", "5"), next: op(2, 14, 1, 1, "2"), expected: [op(2, 6, 1, 1, "2"), op(11, 14, 1, 1, "")]},
  {prev: op(6, 11, 1, 1, "1", "6"), next: op(14, 18, 1, 1, "2"), expected: [op(14, 18, 1, 1, "2")]},
  {prev: op(6, 11, 1, 1, "1", "7"), next: op(7, 18, 2, 2, "2"), expected: [op(7, 18, 2, 2, "2")]},
  {prev: op(1, 5, 1, 1, "1", "8"), next: op(4, 18, 1, 1, "2"), expected: [op(5, 18, 1, 1, "2")]},
];

describe("resolveConflictingRanges", () => {
  test.each(casesSC)('case $prev.originatorId', ({prev, next, expected}) => {
    expect(arraysAreEqual(resolveConflictingRanges(prev, next), expected)).toEqual(true);
  });
});

function op(
  startColumn: number,
  endColumn: number,
  startLine: number,
  endLine: number,
  text: string,
  originatorId?: string
): Operation {
  return {
    text: text,
    endColumn: endColumn,
    startColumn: startColumn,
    endLine: endLine,
    startLine: startLine,
    originatorId: originatorId
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
  const keys: (keyof Operation)[] = ['text', 'endColumn', 'startColumn', 'endLine', 'startLine', 'originatorId'];

  for (const key of keys) {
      if (obj1[key] !== obj2[key]) {
          return false;
      }
  }

  return true;
}
