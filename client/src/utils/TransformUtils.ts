import { Operation } from "../models/Operation";

export function transform(
  op: Operation,
  history: Map<number, Operation[]>
): Operation {}

function transformAgainst(
  prev: Operation,
  next: Operation,
  history: Map<number, Operation[]>
): Operation {}

// Relies on assumption that revisionIds have already been considered
export function isPreviousOperationRelevent(
  prev: Operation,
  next: Operation
): boolean {
  if (prev.originatorId === next.originatorId) return false;

  let isPrevStartLineAfterNextEndLine: boolean = prev.startLine > next.endLine;
  let isSameLine: boolean = prev.startLine === next.endLine;

  if (isPrevStartLineAfterNextEndLine) return false;
  if (isSameLine) {
    if (isInsert(next)) {
      if (next.endColumn < prev.startColumn) return false;
    } else {
      if (next.endColumn <= prev.startColumn) return false;
    }
  }
  return true;
}

export function isInsert(op: Operation): boolean {
  return op.endColumn === op.startColumn && op.endLine === op.startLine;
}
