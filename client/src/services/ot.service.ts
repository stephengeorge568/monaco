import { Operation, fromEvent } from "../models/Operation";
import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';

export function registerManualChange(event: monaco.editor.IModelContentChangedEvent) {
  let op: Operation = fromEvent(event, '');
}

export function onOperationTransformedAck(newRevisionId: string) {
  console.log(newRevisionId);
}

