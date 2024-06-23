import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';

export interface Operation {
  text: string,
  endColumn: number,
  startColumn: number;
  endLine: number;
  startLine: number;
  revisionId: number;
  originatorId?: string;
}

export function fromEvent(event: monaco.editor.IModelContentChangedEvent, originatorId: string, revisionId: number = -1): Operation {
  return {
    revisionId: revisionId,
    text: event.changes[0].text,
    endColumn: event.changes[0].range.endColumn,
    startColumn: event.changes[0].range.startColumn,
    endLine: event.changes[0].range.endLineNumber,
    startLine: event.changes[0].range.startLineNumber,
    originatorId: originatorId,
  };
}

export function copy(op: Operation): Operation {
  return {
    revisionId: op.revisionId,
    text: op.text,
    endColumn: op.endColumn,
    startColumn: op.startColumn,
    startLine: op.startLine,
    endLine: op.endLine,
    originatorId: op.originatorId,
  };
}

export type OpHistory = { [revId: number]: Operation };
