import { Operation } from "../models/Operation";

export function transform(prev: Operation, next: Operation): void {

}

// TODO simplify after unit tests
export function isPreviousOperationRelevent(prev: Operation, next: Operation): boolean {
  // let isPrevStartLineAfterNextEndLine: boolean = prev.startLineNumber > next.endLineNumber;
  // let isSameLine: boolean = prev.startLineNumber === next.endLineNumber

  // if (isPrevStartLineAfterNextEndLine) return false;
  // if (isSameLine) { 
  //   if(isInsert(next)) {
  //     if (next.endColumn < prev.startColumn) return false;
  //   } else {
  //     if (next.endColumn <= prev.startColumn) return false;
  //   }
  // }
  return true;
}

export function isInsert(op: Operation): boolean {
  //return op.endColumn === op.startColumn && op.endLineNumber === op.startLineNumber;
  return true;
}